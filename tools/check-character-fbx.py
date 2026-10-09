"""Blender FBX round-trip check for the new studies; excludes studio geometry."""
import bpy
import json
from pathlib import Path
from mathutils.kdtree import KDTree

root = Path(__file__).resolve().parents[1]
for name in ['Moss', 'Bop', 'Nook']:
    bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=str(root / 'Assets/Pockle/Art' / name / (name + '.fbx')))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH']
    assert len(meshes) == 8, f'{name}: missing character parts'
    assert all(obj.type == 'MESH' for obj in bpy.context.scene.objects), 'Studio objects leaked into export'
    body = next(obj for obj in meshes if obj.name == name + 'Body')
    source = json.loads((root / 'Assets/Pockle/Resources/Characters' / name / (name + '.pocklemesh')).read_text())
    expected = {}
    for i in range(0, len(source['positions']), 3):
        x, y, z = source['positions'][i:i+3]
        expected[source['normalGroups'][i//3]] = (x, -z, y)
    tree = KDTree(len(expected))
    for index, point in enumerate(expected.values()): tree.insert(point, index)
    tree.balance(); matched = set(); error = 0
    for vertex in body.data.vertices:
        _, index, distance = tree.find(body.matrix_world @ vertex.co)
        error = max(error, distance); matched.add(index)
    assert error < 1e-6 and len(matched) == len(expected) == len(body.data.vertices), f'{name}: FBX changed body positions'
    assert len(body.data.polygons) == len(source['triangles'])//3 and body.data.uv_layers, f'{name}: changed topology/UVs'
    print(f'PASS: {name} FBX round trip; {len(meshes)} parts, {len(expected)} logical body vertices, max error {error:.9f} m')
