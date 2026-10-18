#!/usr/bin/env python3
"""Author a single-color connected maki squishy base with reusable UVs.
Run with Blender 4.5.3: blender -b --python tools/build-sushi-base.py
No remote generation, painted texture, collection entries or inventory changes.
"""
import bpy
import bmesh
import json
import math
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
MODELS = ROOT / 'models'
SOURCE = ROOT / 'ArtSource/SushiBase'
PREVIEW = ROOT / 'docs/captures/sushi-base'
for folder in [MODELS, SOURCE, PREVIEW]: folder.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version = 0
parts = []

def select(obj):
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj

def ellipsoid(name, pos, scale, angle=0):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, location=pos)
    obj = bpy.context.object; obj.name=name; obj.scale=scale; obj.rotation_euler.z=angle
    select(obj); bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    parts.append(obj)
    return obj

# A wide, squat roll, with a broad soft base and rounded side wall.
bpy.ops.mesh.primitive_cylinder_add(vertices=96, radius=1, depth=1.16, location=(0,0,.58))
body = bpy.context.object; body.name = 'Maki soft body'
bevel = body.modifiers.new('Soft rounded silhouette', 'BEVEL'); bevel.width=.15; bevel.segments=6
bpy.ops.object.modifier_apply(modifier=bevel.name)
parts.append(body)
# The folded outer rim is geometry only: the same base color as every detail.
bpy.ops.mesh.primitive_torus_add(major_segments=96, minor_segments=16,
    major_radius=.79, minor_radius=.115, location=(0,0,1.115))
parts.append(bpy.context.object)
# Shallow, broad rice relief; smooth enough for a squishy, without fragile individual grains.
for row, count, radius, phase in [(0,22,.69,0),(1,17,.48,.11)]:
    for i in range(count):
        a=i*2*math.pi/count+phase
        z=1.19 + .016*math.sin(i*2.3+row)
        ellipsoid('Molded rice relief', (math.cos(a)*radius, math.sin(a)*radius,z),
            (.075,.124,.14), a+.32*math.sin(i*1.7))
# Three simple molded filling sections, slightly recessed relative to the rice ring.
for i in range(3):
    a=i*2*math.pi/3+.2
    ellipsoid('Molded filling relief', (.205*math.cos(a),.205*math.sin(a),1.18),
        (.165,.245,.15), a)

bpy.ops.object.select_all(action='DESELECT')
for obj in parts: obj.select_set(True)
bpy.context.view_layer.objects.active = body
bpy.ops.object.join()
# Weld the molded details into one closed skin suitable for shared deformation later.
remesh=body.modifiers.new('Connected squishy skin', 'REMESH'); remesh.mode='VOXEL'; remesh.voxel_size=.018
remesh.use_smooth_shade=True
bpy.ops.object.modifier_apply(modifier=remesh.name)
smooth=body.modifiers.new('Soft molding', 'SMOOTH'); smooth.factor=.65; smooth.iterations=3
bpy.ops.object.modifier_apply(modifier=smooth.name)
body.data.calc_loop_triangles()
reduce=body.modifiers.new('Mobile mesh budget', 'DECIMATE')
reduce.ratio=min(1,12000/len(body.data.loop_triangles))
bpy.ops.object.modifier_apply(modifier=reduce.name)
tri=body.modifiers.new('Stable triangle topology', 'TRIANGULATE')
bpy.ops.object.modifier_apply(modifier=tri.name)
# Keep the export upright, with origin at the floor and no hidden transform scale.
select(body); bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
world=[body.matrix_world@v.co for v in body.data.vertices]
bottom=min(v.z for v in world)
for vertex in body.data.vertices:
    vertex.co=body.matrix_world@vertex.co
    vertex.co.z-=bottom
body.matrix_world.identity()
body.name='SushiBase'; body.data.name='SushiBase connected skin'
for polygon in body.data.polygons: polygon.use_smooth=True
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=.015,
    margin_method='FRACTION', scale_to_bounds=True)
bpy.ops.object.mode_set(mode='OBJECT')

def linear(c):
    return c/12.92 if c<=.04045 else ((c+.055)/1.055)**2.4
mat=bpy.data.materials.new('SushiBase cream'); mat.use_nodes=True
cream=tuple(linear(v/255) for v in (239,229,209))+(1,)
mat.diffuse_color=cream
principled=mat.node_tree.nodes.get('Principled BSDF')
principled.inputs['Base Color'].default_value=cream
principled.inputs['Metallic'].default_value=0
principled.inputs['Roughness'].default_value=.46
body.data.materials.clear(); body.data.materials.append(mat)
# Verify the model itself, rather than inferring topology from its rendered preview.
mesh=bmesh.new(); mesh.from_mesh(body.data)
non_manifold=sum(not edge.is_manifold for edge in mesh.edges)
remaining=set(mesh.verts); components=0; groups=[]
while remaining:
    components+=1; stack=[remaining.pop()]; group=[]
    while stack:
        v=stack.pop(); group.append(v)
        for e in v.link_edges:
            other=e.other_vert(v)
            if other in remaining: remaining.remove(other); stack.append(other)
    groups.append(group)
# Decimation can leave a zero-volume pair of opposing triangles. Remove only tiny remnants.
removed_components=0
for group in groups:
    if len(group)<=8:
        bmesh.ops.delete(mesh,geom=group,context='VERTS'); removed_components+=1
components-=removed_components
non_manifold=sum(not edge.is_manifold for edge in mesh.edges)
mesh.to_mesh(body.data); mesh.free(); body.data.update()
body.data.calc_loop_triangles()
assert non_manifold==0 and components==1, (non_manifold,components)
assert len(body.data.materials)==1 and len(body.data.loop_triangles)<=12500
assert len(body.data.uv_layers)==1
# Save the authoring model without presentation-only cameras/lights/floor.
select(body)
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'SushiBase.blend'))
bpy.ops.export_scene.gltf(filepath=str(MODELS/'SushiBase.glb'), export_format='GLB',
    use_selection=True, export_materials='EXPORT', export_yup=True, export_animations=False)
bpy.ops.export_scene.fbx(filepath=str(MODELS/'SushiBase.fbx'), use_selection=True,
    object_types={'MESH'}, add_leaf_bones=False, axis_forward='-Z', axis_up='Y', bake_anim=False)
manifest={'name':'SushiBase','design':'Owner-selected maki roll, shallow molded rice and three filling sections',
    'base_color_srgb':'#EFE5D1','material_count':1,'painted_textures':0,
    'vertices':len(body.data.vertices),'triangles':len(body.data.loop_triangles),
    'connected_components':components,'non_manifold_edges':non_manifold,
    'uv_layers':len(body.data.uv_layers),'floor_z':0,'geometry_origin':'Locally authored in Blender 4.5.3',
    'status':'Review base only; not registered in Unity collections or awarded; materials/expressions deferred'}
(SOURCE/'PROVENANCE.json').write_text(json.dumps(manifest,indent=2)+'\n')
# Render the actual deliverable with a neutral soft studio, showing molded shape only.
scene=bpy.context.scene; scene.render.engine='CYCLES'; scene.cycles.samples=32
scene.cycles.use_denoising=True
scene.render.resolution_x=1024; scene.render.resolution_y=1024; scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
scene.world=bpy.data.worlds.new('Neutral studio'); scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.74,.74,.74,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.12
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.025))
floor=bpy.context.object; floor_mat=bpy.data.materials.new('Presentation floor'); floor_mat.diffuse_color=(.77,.75,.71,1)
floor.data.materials.append(floor_mat)
for location, power, size in [((2.8,-3.5,5),400,2.6),((-3,-1,3),70,3),((1,3,4),220,3)]:
    bpy.ops.object.light_add(type='AREA',location=location)
    lamp=bpy.context.object; lamp.data.energy=power; lamp.data.shape='DISK'; lamp.data.size=size
    lamp.rotation_euler=(Vector((0,0,.6))-lamp.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=(3,-4.6,4.8)); camera=bpy.context.object
camera.data.type='ORTHO'; camera.data.ortho_scale=3.1; scene.camera=camera
for name,location in [('three-quarter',(3,-4.6,4.8)),('front',(0,-5,1.8)),('top',(0,-.001,5))]:
    camera.location=location
    camera.rotation_euler=(Vector((0,0,.62))-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(PREVIEW/(name+'.png')); bpy.ops.render.render(write_still=True)
print('SUSHI_BASE',json.dumps(manifest))
