#!/usr/bin/env python3
"""Create stable metadata for newly added prototype assets; preserve existing GUIDs."""
import hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def guid(path):
    return hashlib.md5(("pockle-prototype:" + path).encode()).hexdigest()


def main():
    created = 0
    for asset in sorted((ROOT / "Assets").rglob("*")):
        if asset.name.endswith(".meta"):
            continue
        meta = Path(str(asset) + ".meta")
        if meta.exists():
            continue
        relative = asset.relative_to(ROOT).as_posix()
        header = "fileFormatVersion: 2\nguid: " + guid(relative) + "\n"
        if asset.is_dir():
            body = "folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
        elif asset.suffix == ".cs":
            body = "MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
        elif asset.suffix == ".shader":
            body = "ShaderImporter:\n  externalObjects: {}\n  defaultTextures: []\n  nonModifiableTextures: []\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
        elif asset.suffix == ".asmdef":
            body = "AssemblyDefinitionImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
        elif asset.suffix == ".mat":
            body = "NativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 2100000\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
        else:
            body = "DefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
        meta.write_text(header + body)
        created += 1
    print("Created", created, "new Unity metadata files; existing GUIDs preserved.")


if __name__ == "__main__":
    main()
