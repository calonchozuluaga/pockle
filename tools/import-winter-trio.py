#!/usr/bin/env python3
"""Import the three additional winter owner GLBs without changing originals; Blender 4.5 batch runner."""
from pathlib import Path
import bpy, hashlib, json, math
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
for name in ['reiny','guin','bundy']:
 source=ROOT/'models'/f'{name.title()}.glb'; title=name.title()
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.context.preferences.filepaths.save_version=0
 bpy.ops.import_scene.gltf(filepath=str(source))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 assert len(meshes)==1, f'{name}: expected one authored mesh'
 o=meshes[0];before=sum(len(p.vertices)-2 for p in o.data.polygons)
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.remove_doubles(threshold=.000001);bpy.ops.object.mode_set(mode='OBJECT')
 dec=o.modifiers.new('Phone mesh budget','DECIMATE');dec.ratio=min(1,30000/before);bpy.ops.object.modifier_apply(modifier=dec.name)
 points=[o.matrix_world@v.co for v in o.data.vertices];bottom=min(v.z for v in points);height=max(v.z for v in points)-bottom
 for v in o.data.vertices:
  v.co=o.matrix_world@v.co;v.co.z-=bottom;v.co*=1.94/height
 o.matrix_world.identity()
 for p in o.data.polygons:p.use_smooth=True
 folder=ROOT/'Assets/Pockle/Art/Models'/title;folder.mkdir(parents=True,exist_ok=True)
 mat=o.active_material;bsdf=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=bsdf.inputs['Base Color'].links[0].from_node
 paint=tex.image;paint.filepath_raw=str(folder/f'{title}BaseColor.png');paint.file_format='PNG';paint.save()
 bpy.ops.export_scene.fbx(filepath=str(folder/f'{title}.fbx'),use_selection=True,object_types={'MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y')
 tris=sum(len(p.vertices)-2 for p in o.data.polygons)
 (folder/'PROVENANCE.json').write_text(json.dumps({'source_file':source.name,'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'source_triangles':before,'runtime_triangles':tris,'texture_size':list(paint.size),'conversion':'Weld source UV seams, decimate to about 30k triangles, retain original paint and UVs. Unity native build normalizes floor and viewer direction.','source_rigged':False},indent=2)+'\n')
 scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=768;scene.render.resolution_y=768;scene.render.resolution_percentage=100
 scene.world=bpy.data.worlds.new('Review studio');scene.world.color=(.55,.45,.4);scene.view_settings.view_transform='AgX'
 def area(loc,power,size):
  bpy.ops.object.light_add(type='AREA',location=loc);l=bpy.context.object;l.data.energy=power;l.data.size=size;l.rotation_euler=(Vector((0,0,1))-l.location).to_track_quat('-Z','Y').to_euler()
 area((-3,-4,5),500,4);area((4,2,4),400,3)
 bpy.ops.object.camera_add(location=(0,-5,1));cam=bpy.context.object;cam.data.type='ORTHO';cam.data.ortho_scale=2.65;scene.camera=cam
 dest=ROOT/'docs/captures/winter-trio-source';dest.mkdir(parents=True,exist_ok=True)
 for label,y in [('front',-5),('back',5)]:
  cam.location=(0,y,1);cam.rotation_euler=(Vector((0,0,.97))-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(dest/f'{name}-{label}.png');bpy.ops.render.render(write_still=True)
 print('HOLIDAY_IMPORTED',name,before,tris,flush=True)
