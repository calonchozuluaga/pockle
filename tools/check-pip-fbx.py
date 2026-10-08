"""Verify the standard FBX round trip using Blender's importer, not its file header."""
import bpy
import json
from pathlib import Path
from mathutils.kdtree import KDTree

root = Path(__file__).resolve().parents[1]
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.fbx(filepath=str(root / 'Assets/Pockle/Art/Pip/Pip.fbx'))
meshes = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH']
assert len(meshes) == 22, f'FBX lost character parts: {len(meshes)}'
body = next(obj for obj in meshes if obj.name == 'PipBody')
source = json.loads((root / 'Assets/Pockle/Resources/Pip/Pip.pocklemesh').read_text())
expected = {}
for i in range(0, len(source['positions']), 3):
    x, y, z = source['positions'][i:i + 3]
    expected[source['normalGroups'][i // 3]] = (x, -z, y)
tree = KDTree(len(expected))
for index, point in enumerate(expected.values()): tree.insert(point, index)
tree.balance()
matched = set()
largest_error = 0
for vertex in body.data.vertices:
    _, index, distance = tree.find(body.matrix_world @ vertex.co)
    largest_error = max(largest_error, distance)
    matched.add(index)
# Compare positions at float32 precision, without decimal-rounding bucket edges.
assert largest_error <= 1e-6, f'FBX changed body positions by {largest_error} meters'
assert len(matched) == len(expected) == len(body.data.vertices), 'FBX lost or duplicated logical vertices'
assert len(body.data.polygons) == len(source['triangles']) // 3, 'FBX body topology changed'
assert body.data.uv_layers, 'FBX has no UV map'
assert all(obj.type == 'MESH' for obj in bpy.context.scene.objects), 'Studio leaked into character export'
print(f'PASS: FBX imported with {len(meshes)} character parts, {len(expected)} body vertices, {len(body.data.polygons)} body triangles and a UV map. Maximum position error: {largest_error:.9f} m.')
