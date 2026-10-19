#!/usr/bin/env python3
"""Import the approved Wally Meshy GLB through Blender; retain source paint and UVs."""
from pathlib import Path
import bpy, hashlib, json, sys
ROOT = Path(__file__).resolve().parents[1]
source = Path(sys.argv[sys.argv.index('--') + 1])
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(source))
meshes = [o for o in bpy.context.scene.objects if o.type == 'MESH']
assert len(meshes) == 1
obj = meshes[0]
original = sum(len(p.vertices)-2 for p in obj.data.polygons)
bpy.ops.object.select_all(action='DESELECT'); obj.select_set(True); bpy.context.view_layer.objects.active=obj
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT'); bpy.ops.mesh.remove_doubles(threshold=.000001); bpy.ops.object.mode_set(mode='OBJECT')
dec = obj.modifiers.new('Phone mesh budget', 'DECIMATE'); dec.ratio=min(1, 30000/original); bpy.ops.object.modifier_apply(modifier=dec.name)
points = [obj.matrix_world @ v.co for v in obj.data.vertices]
low=[min(p[i] for p in points) for i in range(3)]; high=[max(p[i] for p in points) for i in range(3)]
scale=min(2.2/(high[0]-low[0]),2.4/(high[1]-low[1]),1.94/(high[2]-low[2]))
for v in obj.data.vertices:
 p=obj.matrix_world@v.co
 v.co=((p.x-(low[0]+high[0])/2)*scale,(p.y-(low[1]+high[1])/2)*scale,(p.z-low[2])*scale)
obj.matrix_world.identity()
for poly in obj.data.polygons: poly.use_smooth=True
folder=ROOT/'Assets/Pockle/Art/Models/Wally'; folder.mkdir(parents=True,exist_ok=True)
shader=next(n for n in obj.active_material.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
paint=shader.inputs['Base Color'].links[0].from_node.image
paint.filepath_raw=str(folder/'WallyBaseColor.png'); paint.file_format='PNG'; paint.save()
bpy.ops.export_scene.fbx(filepath=str(folder/'Wally.fbx'),use_selection=True,object_types={'MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y')
# Sample dark painted front features, in the Unity coordinates used by the collection builder.
pixels=list(paint.pixels); width,height=paint.size; coords=[]
for poly in obj.data.polygons:
 for loop_i in poly.loop_indices:
  v=obj.data.vertices[obj.data.loops[loop_i].vertex_index].co; uv=obj.data.uv_layers.active.data[loop_i].uv
  x=int(uv.x*width)%width; y=int(uv.y*height)%height; rgb=pixels[(y*width+x)*4:(y*width+x)*4+3]
  if max(rgb)<.18 and v.y<-.4: coords.append([round(v.x,4),round(v.z-1,4),round(v.y,4)])
provenance={'source_file':source.name,'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'source_resource':'image-to-3d','source_task_id':'01a1546c-77f0-7110-9bef-148ea8809008','source_triangles':original,'runtime_triangles':sum(len(p.vertices)-2 for p in obj.data.polygons),'runtime_vertices_before_uv_splits':len(obj.data.vertices),'texture_size':list(paint.size),'conversion':'Weld UV seams, reduce to approximately 30000 triangles, preserve paint/UVs, normalize proportionally. No paid generation or source edits.','source_rigged':False,'source_animations':0}
(folder/'PROVENANCE.json').write_text(json.dumps(provenance,indent=2)+'\n')
Path('/private/tmp/wally-feature-samples.json').write_text(json.dumps(coords))
print('WALLY_IMPORT',json.dumps(provenance), 'BOUNDS',low,high,'SCALE',scale,flush=True)
