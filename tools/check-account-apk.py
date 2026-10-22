#!/usr/bin/env python3
"""Verify actual DEX definitions for the account JNI bridge and OAuth callback activity."""
import argparse,importlib.util,pathlib,zipfile,subprocess
parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('apk');parser.add_argument('--aapt',required=True);args=parser.parse_args()
spec=importlib.util.spec_from_file_location('dexcheck',pathlib.Path(__file__).with_name('check-walking-apk.py'));dexcheck=importlib.util.module_from_spec(spec);spec.loader.exec_module(dexcheck)
required={'Lstudio/pockle/account/SecureSession;':{'put','get'},'Lstudio/pockle/account/AuthCallbackActivity;':{'onCreate'}}
with zipfile.ZipFile(args.apk) as apk:
 dexes=[apk.read(n) for n in apk.namelist() if n.startswith('classes') and n.endswith('.dex')]
 for descriptor,methods in required.items():
  dexcheck.DESCRIPTOR=descriptor;found=[dexcheck.bridge_api(data) for data in dexes];assert any(api is not None and methods<=api for api in found),descriptor+' JNI API missing'
manifest=subprocess.check_output([args.aapt,'dump','xmltree',args.apk,'AndroidManifest.xml'],text=True)
for value in ['studio.pockle.account.AuthCallbackActivity','com.playpockle.app','/callback','android.permission.INTERNET']:assert value in manifest,value+' absent from manifest'
print('PASS: secure token JNI methods, exported OAuth callback activity, exact scheme/path and Internet permission packaged.')
