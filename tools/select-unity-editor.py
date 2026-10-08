#!/usr/bin/env python3
"""Record the compatible editor selected locally, without silently choosing a version."""
import re
import sys
from pathlib import Path

if len(sys.argv) != 2 or not re.fullmatch(r"6000\.6\.\d+[abf]\d+", sys.argv[1]):
    raise SystemExit("Usage: python3 tools/select-unity-editor.py <installed-version>\nEnter the exact Unity 6.6 version from About Unity or Unity Hub (6000.6.xfN, or the installed alpha/beta version). This prototype uses Built-in 3D.")
path = Path(__file__).resolve().parents[1] / "ProjectSettings" / "ProjectVersion.txt"
content = "m_EditorVersion: " + sys.argv[1] + "\n"
if path.exists() and path.read_text() != content:
    raise SystemExit("ProjectVersion.txt already exists. Review the current pin before changing editors.")
path.write_text(content)
print("Recorded Unity " + sys.argv[1] + ". Add this project from disk in Unity Hub, then use Pockle > Open tactile prototype.")
