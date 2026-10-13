#!/usr/bin/env python3
"""Fox art-review conversion: weld, reduce to 30000 triangles, and rebake paint.
Run: blender -b --python tools/import-meshy-fox.py -- /path/to/owner.glb
The original upload stays outside Assets; Unity needs no GLB package.
"""
import collections
import hashlib
import json
import math
import sys
from pathlib import Path
import bpy

ROOT = Path(__file__).resolve().parents[1]
source = Path(sys.argv[sys.argv.index('--') + 1])
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(source))
objects = [o for o in bpy.context.scene.objects if o.type == 'MESH']
assert len(objects) == 1, 'Expected a single-mesh fox GLB'
original = objects[0]
original_triangles = sum(len(p.vertices) - 2 for p in original.data.polygons)

# glTF duplicates vertices at UV seams. Collapsing those disconnected charts
# independently opens physical holes. Weld positions while retaining loop UVs.
o = original.copy()
o.data = original.data.copy()
bpy.context.collection.objects.link(o)
bpy.ops.object.select_all(action='DESELECT')
o.select_set(True)
bpy.context.view_layer.objects.active = o
before = len(o.data.vertices)
bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.mesh.remove_doubles(threshold=.000001)
bpy.ops.object.mode_set(mode='OBJECT')
welded_count = before - len(o.data.vertices)
reduce = o.modifiers.new('Phone deformation budget', 'DECIMATE')
reduce.ratio = min(1.0, 30000 / original_triangles)
bpy.ops.object.modifier_apply(modifier=reduce.name)

# Retain source UVs and paint for this art-review import.
material = o.active_material
texture = next(n for n in material.node_tree.nodes if n.type == "BSDF_PRINCIPLED").inputs["Base Color"].links[0].from_node
paint = texture.image
folder = ROOT / 'Assets/Pockle/Art/Models/Fox'
folder.mkdir(parents=True, exist_ok=True)
paint.filepath_raw = str(folder / 'FoxBaseColor.png')
paint.file_format = 'PNG'
paint.save()

# Save a native interchange model for Unity; no catalog or save identity assigned.
bpy.ops.object.select_all(action='DESELECT')
o.select_set(True)
bpy.context.view_layer.objects.active = o
# Keep a uniform scale with a floor at zero, preserving original proportions.
points = [o.matrix_world @ v.co for v in o.data.vertices]
bottom = min(v.z for v in points)
height = max(v.z for v in points) - bottom
for vertex in o.data.vertices:
    vertex.co = o.matrix_world @ vertex.co
    vertex.co.z -= bottom
    vertex.co *= 1.94 / height
o.matrix_world.identity()
for poly in o.data.polygons:
    poly.use_smooth = True
principled = next(n for n in material.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
material.node_tree.links.new(texture.outputs['Color'], principled.inputs['Base Color'])
principled.inputs['Metallic'].default_value = 0
principled.inputs['Roughness'].default_value = .4
bpy.ops.export_scene.fbx(filepath=str(folder / 'Fox.fbx'), use_selection=True,
                        object_types={'MESH'}, add_leaf_bones=False, axis_forward='-Z', axis_up='Y')
triangles = sum(len(p.vertices)-2 for p in o.data.polygons)
assert triangles <= 35000
provenance = {
    'source_file': source.name,
    'source_sha256': hashlib.sha256(source.read_bytes()).hexdigest(),
    'source_triangles': original_triangles,
    'runtime_triangles': triangles,
    'runtime_vertices_before_uv_splits': len(o.data.vertices),
    'welded_source_duplicates': welded_count,
    'texture_size': [2048, 2048],
    'conversion': 'Weld, decimate to approximately 30000 triangles, source UVs and base-color texture retained',
    'material': 'Base color preserved; review material uses opaque nonmetallic vinyl, roughness 0.4. Original packed metallic/roughness and normal maps archived separately; available for future material tuning.',
    'status': 'Separate art review asset; not assigned to a catalog ID or available in inventory; Unity and phone review pending'
}
(folder / 'PROVENANCE.json').write_text(json.dumps(provenance, indent=2)+'\n')
print('FOX_EXPORT', json.dumps(provenance))
