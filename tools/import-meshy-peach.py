#!/usr/bin/env python3
"""Blender conversion: weld before reducing, then rebake paint onto padded UVs.
Run: blender -b --python tools/import-meshy-peach.py -- /path/to/owner.glb
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
assert len(objects) == 1, 'Expected the owner-uploaded single-mesh Peach GLB'
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
reduce.ratio = .06
bpy.ops.object.modifier_apply(modifier=reduce.name)

# Reproject the original paint, rather than stretching its old UV charts over
# collapsed triangles. New islands have room for filtering and 32px dilation.
bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=.025,
                         margin_method='FRACTION', area_weight=0, scale_to_bounds=True)
bpy.ops.object.mode_set(mode='OBJECT')
original_nodes = original.active_material.node_tree
base = next(n for n in original_nodes.nodes if n.type == 'BSDF_PRINCIPLED').inputs['Base Color'].links[0].from_node
emission = original_nodes.nodes.new('ShaderNodeEmission')
original_nodes.links.new(base.outputs['Color'], emission.inputs['Color'])
output = next(n for n in original_nodes.nodes if n.type == 'OUTPUT_MATERIAL')
original_nodes.links.new(emission.outputs[0], output.inputs['Surface'])
o.data.materials.clear()
material = bpy.data.materials.new('Padded original Peach paint')
material.use_nodes = True
o.data.materials.append(material)
paint = bpy.data.images.new('PeachBaseColor', width=2048, height=2048, alpha=False)
texture = material.node_tree.nodes.new('ShaderNodeTexImage')
texture.image = paint
material.node_tree.nodes.active = texture
original.select_set(True)
scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.samples = 1
scene.cycles.use_denoising = False
scene.render.bake.use_selected_to_active = True
scene.render.bake.cage_extrusion = .08
scene.render.bake.max_ray_distance = .16
scene.render.bake.margin = 32
scene.render.bake.margin_type = 'EXTEND'
bpy.ops.object.bake(type='EMIT')
folder = ROOT / 'Assets/Pockle/Resources/Pip/Meshy'
folder.mkdir(parents=True, exist_ok=True)
paint.filepath_raw = str(folder / 'PeachBaseColor.png')
paint.file_format = 'PNG'
paint.save()

m = o.data
m.calc_loop_triangles()
uv = m.uv_layers.active.data
points = [o.matrix_world @ v.co for v in m.vertices]
bottom = min(v.z for v in points)
top = max(v.z for v in points)
scale = 1.94 / (top - bottom)
positions, texcoords, triangles, groups = [], [], [], []
keys, logical = {}, {}
for tri in m.loop_triangles:
    ids = []
    for loop in tri.loops:
        vertex = m.loops[loop].vertex_index
        p, t = points[vertex], uv[loop].uv
        # Blender Z-up/right-handed -> Unity Y-up/left-handed. Front is -Z.
        xyz = (round(p.x * scale, 7), round((p.z - bottom) * scale - 1, 7), round(p.y * scale, 7))
        key = (vertex, round(t.x, 7), round(t.y, 7))
        if key not in keys:
            index = len(groups)
            keys[key] = index
            positions.extend(xyz)
            texcoords.extend((round(t.x, 7), round(t.y, 7)))
            if xyz not in logical:
                logical[xyz] = index
            groups.append(logical[xyz])
        ids.append(keys[key])
    triangles.extend((ids[0], ids[2], ids[1]))
assert len(triangles) // 3 <= 6000 and len(groups) <= 4000
edges = collections.Counter()
for a, b, c in zip(*[iter(triangles)] * 3):
    for i, j in [(a, b), (b, c), (c, a)]:
        edges[tuple(sorted((groups[i], groups[j])))] += 1
assert all(count == 2 for count in edges.values()), 'Reduced Peach must be a closed surface'
asset = json.loads((ROOT / 'Assets/Pockle/Resources/Pip/Pip.pocklemesh').read_text())
asset.update(name='Pip Meshy Peach phone study', characterId='pip', integratedCrown=True, bakedSurface=True,
             baseColorTexturePath='Assets/Pockle/Resources/Pip/Meshy/PeachBaseColor.png', positions=positions,
             triangles=triangles, uv=texcoords, normalGroups=groups,
             bodyColor=[1, 1, 1, 1], topColor=[1, 1, 1, 1], bottomColor=[1, 1, 1, 1])
(folder / 'Peach.pocklemesh').write_text(json.dumps(asset, separators=(',', ':')) + '\n')
provenance = {
    'source_file': source.name, 'source_sha256': hashlib.sha256(source.read_bytes()).hexdigest(),
    'source_triangles': original_triangles, 'welded_source_duplicates': welded_count,
    'runtime_triangles': len(triangles) // 3, 'runtime_vertices': len(groups), 'open_edges': 0,
    'texture_size': [2048, 2048], 'texture_format': 'lossless PNG', 'decimate_ratio': .06,
    'uv_rebake': 'Smart UV 66 degrees, .025 fractional island gap; original emission selected-to-active, .08 cage/.16 ray, 32px EXTEND dilation',
    'coordinate_conversion': 'Blender (x,y,z) to Unity (x,z,y), winding reversed; uniform scale, floor y=-1, crown y=.94',
    'source_material': 'opaque textured metallic/roughness + normal maps; no transmission extension',
    'unity_material': 'single-pass textured wet gloss, wrapped lighting and backlight transmission approximation; painted gold speck surface emission; no real refraction or internal pearl geometry',
    'geometry_changes': 'weld original UV-split positions before decimation, uniform scale; owner silhouette retained'
}
folder = ROOT / 'ArtSource/Pip/Meshy'
folder.mkdir(parents=True, exist_ok=True)
(folder / 'PROVENANCE.json').write_text(json.dumps(provenance, indent=2) + '\n')
print('MESHY_EXPORT', json.dumps(provenance))
