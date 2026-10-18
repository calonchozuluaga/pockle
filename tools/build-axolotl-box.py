#!/usr/bin/env python3
"""Create an original opening-ready Axolotl carton and PNG renders from the same 3D model."""
from pathlib import Path
import bpy, math, json
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
SOURCE=ROOT/'ArtSource/AxolotlBox';MODELS=ROOT/'models';UNITY=ROOT/'Assets/Pockle/Art/Models/AxolotlBox';PNG=ROOT/'Assets/Pockle/Resources/UI/Boxes'
for folder in [SOURCE,MODELS,UNITY,PNG]:folder.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.context.preferences.filepaths.save_version=0
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=48;scene.render.film_transparent=True
scene.render.resolution_x=1024;scene.render.resolution_y=1024;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('AxolotlBox studio');scene.world.color=(.3,.3,.3);scene.view_settings.view_transform='Standard';scene.view_settings.look='None';scene.view_settings.exposure=0;scene.view_settings.gamma=1
objects=[]
def material(name,color):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;n=m.node_tree.nodes.get('Principled BSDF');n.inputs['Base Color'].default_value=(*color,1);n.inputs['Roughness'].default_value=.65;return m
paper=material('AxolotlBox · ivory paper',(1,.84,.74));ink=material('AxolotlBox · woodland ink',(.10,.22,.15));copper=material('AxolotlBox · apricot accent',(.77,.24,.09))
def image_material(name,path):
 m=material(name,(1,1,1));n=m.node_tree.nodes.new('ShaderNodeTexImage');n.image=bpy.data.images.load(str(path));emission=m.node_tree.nodes.new('ShaderNodeEmission');emission.inputs['Strength'].default_value=1;m.node_tree.links.new(n.outputs['Color'],emission.inputs['Color']);m.node_tree.links.new(emission.outputs[0],m.node_tree.nodes.get('Material Output').inputs['Surface']);return m
front=image_material('AxolotlBox · mystery front',ROOT/'Assets/Pockle/Resources/Boxes/AxolotlFront.png')
side=image_material('AxolotlBox · mystery pattern',ROOT/'Assets/Pockle/Resources/Boxes/AxolotlSide.png')
lidprint=image_material('AxolotlBox · branded lid',ROOT/'Assets/Pockle/Resources/Boxes/AxolotlLid.png')
root=bpy.data.objects.new('AxolotlCollectionBox',None);scene.collection.objects.link(root);objects.append(root)
def cube(name,location,dimensions,mat,parent=root):
 bpy.ops.mesh.primitive_cube_add(size=1,location=location);o=bpy.context.object;o.name=name;o.dimensions=dimensions;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat)
 if parent:o.parent=parent
 bevel=o.modifiers.new('Paper edge','BEVEL');bevel.width=.006;bevel.segments=2;bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bevel.name)
 objects.append(o);return o
def empty(name,location):
 o=bpy.data.objects.new(name,None);scene.collection.objects.link(o);o.parent=root;o.location=location;objects.append(o);return o
def quad(name,verts,mat,parent=root):
 mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],[(0,1,2,3)]);mesh.update();o=bpy.data.objects.new(name,mesh);scene.collection.objects.link(o);o.parent=parent;o.data.materials.append(mat)
 uv=mesh.uv_layers.new(name='UVMap')
 for loop,coord in zip(uv.data,[(0,0),(1,0),(1,1),(0,1)]):loop.uv=coord
 objects.append(o);return o
W=1.72;D=1.65;H=1.65
cube('Carton floor',(0,0,.018),(W,D,.036),paper)
cube('Front paper wall',(0,-D/2,H/2),(W,.025,H),paper)
cube('Back paper wall',(0,D/2,H/2),(W,.025,H),paper)
cube('Left paper wall',(-W/2,0,H/2),(.025,D,H),paper)
cube('Right paper wall',(W/2,0,H/2),(.025,D,H),paper)
quad('Front mystery print',[(-W/2+.015,-D/2-.016,.015),(W/2-.015,-D/2-.016,.015),(W/2-.015,-D/2-.016,H-.015),(-W/2+.015,-D/2-.016,H-.015)],front)
quad('Left pattern print',[(-W/2-.016,D/2-.015,.015),(-W/2-.016,-D/2+.015,.015),(-W/2-.016,-D/2+.015,H-.015),(-W/2-.016,D/2-.015,H-.015)],side)
quad('Right pattern print',[(W/2+.016,-D/2+.015,.015),(W/2+.016,D/2-.015,.015),(W/2+.016,D/2-.015,H-.015),(W/2+.016,-D/2+.015,H-.015)],side)
quad('Back pattern print',[(W/2-.015,D/2+.016,.015),(-W/2+.015,D/2+.016,.015),(-W/2+.015,D/2+.016,H-.015),(W/2-.015,D/2+.016,H-.015)],side)
lid=empty('AxolotlBox_LidHinge',(0,D/2,H+.015));cube('Printed folding lid',(0,-D/2,0),(W+.025,D+.015,.025),paper,lid)
quad('Lid brand print',[(-W/2,-D,.017),(W/2,-D,.017),(W/2,0,.017),(-W/2,0,.017)],lidprint,lid)
left=empty('AxolotlBox_LeftFlapHinge',(-W/2,0,H));right=empty('AxolotlBox_RightFlapHinge',(W/2,0,H))
cube('Left dust flap',(.21,0,0),(.42,D-.08,.018),paper,left);cube('Right dust flap',(-.21,0,0),(.42,D-.08,.018),paper,right)
scene.frame_start=1;scene.frame_end=48;scene.render.fps=24
for frame,angle in [(1,0),(12,0),(36,-1.90),(48,-1.90)]:
 lid.rotation_euler.x=angle;left.rotation_euler.y=angle*.85;right.rotation_euler.y=-angle*.85
 for hinge in [lid,left,right]:hinge.keyframe_insert(data_path='rotation_euler',frame=frame)
for hinge in [lid,left,right]:
 if hinge.animation_data and hinge.animation_data.action:hinge.animation_data.action.name='AxolotlBox_Open_'+hinge.name
scene.frame_set(1)
# Exports contain the carton only, with real hinged parts and opening animation.
bpy.ops.object.select_all(action='DESELECT')
for o in objects:o.select_set(True)
bpy.context.view_layer.objects.active=root
bpy.ops.export_scene.gltf(filepath=str(MODELS/'AxolotlCollectionBox.glb'),export_format='GLB',use_selection=True,export_animations=True)
bpy.ops.export_scene.fbx(filepath=str(UNITY/'AxolotlCollectionBox.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=False,bake_anim_use_nla_strips=False,add_leaf_bones=False,path_mode='COPY',embed_textures=True)
# Product renders use this exported geometry and printed materials, not AI box mockups.
def area(name,location,power,size):
 bpy.ops.object.light_add(type='AREA',location=location);l=bpy.context.object;l.name=name;l.data.energy=power;l.data.shape='DISK';l.data.size=size;l.rotation_euler=(Vector((0,0,.8))-l.location).to_track_quat('-Z','Y').to_euler()
area('Warm product key',(-3,-4,5),650,4);area('Cool product fill',(4,-2,3),400,3);area('Back rim',(2,4,4),450,3)
bpy.ops.object.camera_add(location=(3,-4,2.8));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,.85))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=3.35;scene.camera=camera
scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA'
scene.render.filepath=str(PNG/'Collection-axolotl.png');bpy.ops.render.render(write_still=True)
scene.frame_set(48);camera.data.ortho_scale=4.9;camera.rotation_euler=(Vector((0,0,1.45))-camera.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(PNG/'Collection-axolotl-open.png');bpy.ops.render.render(write_still=True)
scene.frame_set(1);camera.data.ortho_scale=3.35;camera.rotation_euler=(Vector((0,0,.85))-camera.location).to_track_quat('-Z','Y').to_euler();bpy.ops.file.pack_all();bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'AxolotlCollectionBox.blend'))
triangles=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in objects if o.type=='MESH')
(SOURCE/'README.md').write_text('Opening-ready original Axolotl carton. Source: AxolotlCollectionBox.blend; GLB: models/AxolotlCollectionBox.glb; Unity FBX: Resources/Boxes/AxolotlCollectionBox.fbx. Frame 1 closed, 48 open; 24 fps. Lid and two dust flaps have independent real hinges. Product PNG renders come from the same geometry/materials. '+str(triangles)+' triangles.\n')
print('AXOLOTL_BOX_BUILT',triangles,'triangles',len(objects),'objects')
