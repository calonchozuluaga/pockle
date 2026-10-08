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
        elif asset.suffix == ".pocklemesh":
            importer = guid("Assets/Pockle/Editor/PipCharacterImporter.cs")
            body = "ScriptedImporter:\n  internalIDToNameTable: []\n  externalObjects: {}\n  serializedVersion: 2\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n  script: {fileID: 11500000, guid: " + importer + ", type: 3}\n"
        elif asset.suffix == ".fbx":
            # Unity's model importer supplies remaining defaults on first import.
            body = "ModelImporter:\n  serializedVersion: 22200\n  internalIDToNameTable: []\n  externalObjects: {}\n  meshes:\n    globalScale: 1\n    isReadable: 1\n    meshCompression: 0\n    useFileUnits: 1\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
        else:
            body = "DefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
        meta.write_text('\n'.join(line.rstrip() for line in (header + body).splitlines()) + '\n')
        created += 1
    print("Created", created, "new Unity metadata files; existing GUIDs preserved.")


if __name__ == "__main__":
    main()
