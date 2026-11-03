#!/usr/bin/env python3
"""Inspect the actual unsigned/signed iOS app; does not establish device runtime behavior."""
import argparse
import plistlib
import subprocess
from pathlib import Path


def check(app):
    info = plistlib.loads((app / 'Info.plist').read_bytes())
    executable = app / info['CFBundleExecutable']
    description = subprocess.check_output(['/usr/bin/file', str(executable)], text=True)
    if 'Mach-O' not in description or 'arm64' not in description:
        raise ValueError('Expected physical-device arm64 Mach-O app')
    schemes = [s for item in info.get('CFBundleURLTypes', []) for s in item.get('CFBundleURLSchemes', [])]
    if 'com.playpockle.app' not in schemes:
        raise ValueError('Account callback URL scheme missing')
    framework = app / 'Frameworks/UnityFramework.framework/UnityFramework'
    # Unity's visibility rules can make these symbols local. -g alone gives a false negative.
    symbols = subprocess.check_output(['/usr/bin/nm', '-a', str(framework)], text=True, stderr=subprocess.DEVNULL)
    names = {line.split()[-1] for line in symbols.splitlines() if line.split()}
    for symbol in ('_PockleSecureSet', '_PockleSecureGet', '_PockleSecureFree'):
        if symbol not in names:
            raise ValueError('Linked secure-session function missing: ' + symbol)
    libraries = subprocess.check_output(['/usr/bin/otool', '-L', str(framework)], text=True)
    if '/Security.framework/' not in libraries:
        raise ValueError('Keychain Security framework missing')
    print('PASS: arm64 iPhone app, OAuth callback, all three linked Keychain functions and Security framework.')
    print('Not established: code signing, installation, real sign-in/Keychain behavior or walking.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('app', type=Path)
    check(parser.parse_args().app)
