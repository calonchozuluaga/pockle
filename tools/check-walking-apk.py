#!/usr/bin/env python3
"""Fail if the JNI walking bridge or its public API is absent from an Android APK.
Run after the Android build: python3 tools/check-walking-apk.py /path/to/pockle.apk
Reads DEX class definitions (not incidental strings), so missing Java compilation
or R8 stripping cannot produce a false pass.
"""
import argparse
import struct
import zipfile

DESCRIPTOR = 'Lstudio/pockle/walking/StepCounter;'
REQUIRED = {'<init>', 'start', 'stop', 'total', 'uptimeMillis', 'boot'}

def u32(data, offset):
    return struct.unpack_from('<I', data, offset)[0]

def uleb(data, offset):
    value = shift = 0
    for _ in range(5):
        byte = data[offset]; offset += 1
        value |= (byte & 127) << shift
        if byte < 128:
            return value, offset
        shift += 7
    raise ValueError('Invalid DEX uleb128')

def bridge_api(data):
    if data[:4] != b'dex\x0a':
        raise ValueError('Expected standard APK DEX')
    strings = []
    for index in range(u32(data, 56)):
        offset = u32(data, u32(data, 60) + 4 * index)
        _, offset = uleb(data, offset)
        strings.append(data[offset:data.index(0, offset)].decode('utf-8', errors='replace'))
    type_offset = u32(data, 68)
    method_offset = u32(data, 92)
    for index in range(u32(data, 96)):
        offset = u32(data, 100) + index * 32
        class_index = u32(data, offset)
        if strings[u32(data, type_offset + class_index * 4)] != DESCRIPTOR:
            continue
        cursor = u32(data, offset + 24)
        if cursor == 0:
            return set()
        counts = []
        for _ in range(4):
            count, cursor = uleb(data, cursor); counts.append(count)
        for _ in range(counts[0] + counts[1]):
            _, cursor = uleb(data, cursor); _, cursor = uleb(data, cursor)
        methods = set()
        for count in counts[2:]:
            method_index = 0
            for _ in range(count):
                difference, cursor = uleb(data, cursor); method_index += difference
                access, cursor = uleb(data, cursor); _, cursor = uleb(data, cursor)
                if access & 1:
                    methods.add(strings[u32(data, method_offset + method_index * 8 + 4)])
        return methods
    return None

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('apk'); args = parser.parse_args()
    with zipfile.ZipFile(args.apk) as apk:
        for name in apk.namelist():
            if name.startswith('classes') and name.endswith('.dex'):
                methods = bridge_api(apk.read(name))
                if methods is not None:
                    missing = REQUIRED - methods
                    if missing:
                        parser.exit(1, 'FAIL: walking JNI methods missing: ' + ', '.join(sorted(missing)) + '\n')
                    print('PASS: ' + DESCRIPTOR + ' and all six public JNI entry points are packaged in ' + name)
                    return
    parser.exit(1, 'FAIL: walking Java class is absent from APK DEX class definitions.\n')

if __name__ == '__main__':
    main()
