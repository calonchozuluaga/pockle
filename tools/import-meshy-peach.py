#!/usr/bin/env python3
"""Run with Blender: blender -b --python tools/import-meshy-peach.py -- /path/to/owner.glb.
Exports Unity coordinates + split UVs for the existing native mesh importer.
The original owner upload is kept outside Assets; Unity needs no GLB package.
"""
import hashlib, json, sys
from pathlib import Path
import bpy
ROOT=Path(__file__).resolve().parents[1]
source=Path(sys.argv[sys.argv.index('--')+1])
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(source))
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
assert len(objects)==1, 'Expected the owner-uploaded single-mesh Peach GLB'
o=objects[0];bpy.context.view_layer.objects.active=o;o.select_set(True)
original_triangles=sum(len(p.vertices)-2 for p in o.data.polygons)
reduce=o.modifiers.new('Phone deformation budget','DECIMATE');reduce.ratio=.06
bpy.ops.object.modifier_apply(modifier=reduce.name)
o.data.calc_loop_triangles();m=o.data;uv=m.uv_layers.active.data
points=[o.matrix_world@v.co for v in m.vertices];bottom=min(v.z for v in points);top=max(v.z for v in points);scale=1.94/(top-bottom)
positions=[];texcoords=[];triangles=[];groups=[];keys={};logical={}
for tri in m.loop_triangles:
 ids=[]
 for loop in tri.loops:
  vertex=m.loops[loop].vertex_index;p=points[vertex];t=uv[loop].uv
  # Blender Z-up/right-handed -> Unity Y-up/left-handed. Front is -Z.
  xyz=(round(p.x*scale,7),round((p.z-bottom)*scale-1,7),round(p.y*scale,7))
  key=(vertex,round(t.x,7),round(t.y,7))
  if key not in keys:
   index=len(groups);keys[key]=index;positions.extend(xyz);texcoords.extend((round(t.x,7),round(t.y,7)))
   if xyz not in logical:logical[xyz]=index
   groups.append(logical[xyz])
  ids.append(keys[key])
 triangles.extend((ids[0],ids[2],ids[1]))
assert len(triangles)//3<=6000 and len(groups)<=4000
asset=json.loads((ROOT/'Assets/Pockle/Resources/Pip/Pip.pocklemesh').read_text())
asset.update(name='Pip Meshy Peach phone study',characterId='pip',integratedCrown=True,bakedSurface=True,
 baseColorTexturePath='Assets/Pockle/Resources/Pip/Meshy/PeachBaseColor.jpg',positions=positions,triangles=triangles,uv=texcoords,normalGroups=groups,
 bodyColor=[1,1,1,1],topColor=[1,1,1,1],bottomColor=[1,1,1,1])
folder=ROOT/'Assets/Pockle/Resources/Pip/Meshy';folder.mkdir(parents=True,exist_ok=True)
(folder/'Peach.pocklemesh').write_text(json.dumps(asset,separators=(',',':'))+'\n')
base=next(n for n in o.active_material.node_tree.nodes if n.type=='BSDF_PRINCIPLED').inputs['Base Color'].links[0].from_node.image
base.scale(2048,2048);base.filepath_raw=str(folder/'PeachBaseColor.jpg');base.file_format='JPEG';bpy.context.scene.render.image_settings.quality=95;base.save()
provenance={'source_file':source.name,'source_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'source_triangles':original_triangles,'runtime_triangles':len(triangles)//3,'runtime_vertices':len(groups),'texture_size':[2048,2048],'decimate_ratio':.06,'coordinate_conversion':'Blender (x,y,z) to Unity (x,z,y), winding reversed; uniform scale, floor y=-1, crown y=.94','source_material':'opaque textured metallic/roughness + normal maps; no transmission extension','unity_material':'single-pass textured wet gloss, wrapped lighting and backlight transmission approximation; painted gold speck surface emission; no real refraction or internal pearl geometry','geometry_changes':'decimation and uniform scale only; owner silhouette retained'}
folder=ROOT/'ArtSource/Pip/Meshy';folder.mkdir(parents=True,exist_ok=True);(folder/'PROVENANCE.json').write_text(json.dumps(provenance,indent=2)+'\n')
print('MESHY_EXPORT',json.dumps(provenance))
