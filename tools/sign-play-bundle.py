#!/usr/bin/env python3
"""Owner-run signing step. Never run through an agent or captured terminal."""
import argparse
import getpass
import os
from pathlib import Path
import subprocess
import sys

JAVA = Path('/Applications/Unity/Hub/Editor/6000.6.5f1/PlaybackEngines/AndroidPlayer/OpenJDK/bin')
DEFAULT_KEY = Path.home() / 'Library/Application Support/Pockle/Signing/pockle-upload.jks'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('bundle', type=Path, help='Unsigned AAB prepared by Codex')
    parser.add_argument('--keystore', type=Path, default=DEFAULT_KEY)
    parser.add_argument('--alias', default='pockle-upload')
    parser.add_argument('--create-key', action='store_true', help='Create a new private upload key interactively')
    args = parser.parse_args()
    if not sys.stdin.isatty():
        parser.error('Run this yourself in Terminal; passwords must not enter chat or captured agent output.')
    bundle = args.bundle.expanduser().resolve()
    if not bundle.is_file() or not bundle.name.endswith('.unsigned.aab'):
        parser.error('Input must be the existing .unsigned.aab file.')
    key = args.keystore.expanduser().resolve()
    if args.create_key:
        if key.exists():
            parser.error('A key already exists at this path; use it without --create-key.')
        key.parent.mkdir(parents=True, exist_ok=True, mode=0o700)
        subprocess.run([str(JAVA / 'keytool'), '-genkeypair', '-v', '-storetype', 'JKS',
                        '-keystore', str(key), '-alias', args.alias, '-keyalg', 'RSA',
                        '-keysize', '3072', '-validity', '10000'], check=True)
        key.chmod(0o600)
    if not key.is_file():
        parser.error('Upload keystore not found. Use --create-key or supply your existing --keystore.')
    output = bundle.with_name(bundle.name.replace('.unsigned.aab', '.signed.aab'))
    if output.exists():
        parser.error('Signed output already exists; move it aside before signing again.')
    env = os.environ.copy()
    env['POCKLE_UPLOAD_STORE_PASSWORD'] = getpass.getpass('Upload keystore password: ')
    env['POCKLE_UPLOAD_KEY_PASSWORD'] = getpass.getpass('Upload key password (Enter = same): ') or env['POCKLE_UPLOAD_STORE_PASSWORD']
    try:
        subprocess.run([str(JAVA / 'jarsigner'), '-keystore', str(key), '-storepass:env',
                        'POCKLE_UPLOAD_STORE_PASSWORD', '-keypass:env', 'POCKLE_UPLOAD_KEY_PASSWORD',
                        '-sigalg', 'SHA256withRSA', '-digestalg', 'SHA-256',
                        '-signedjar', str(output), str(bundle), args.alias], env=env, check=True)
    except subprocess.CalledProcessError:
        output.unlink(missing_ok=True)
        raise
    finally:
        env.pop('POCKLE_UPLOAD_STORE_PASSWORD', None)
        env.pop('POCKLE_UPLOAD_KEY_PASSWORD', None)
    subprocess.run([str(JAVA / 'jarsigner'), '-verify', str(output)], check=True)
    print('Signed bundle ready:', output)
    print('Back up the keystore and passwords in your secure store. Do not send them to chat or Git.')


if __name__ == '__main__':
    main()
