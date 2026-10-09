"""Build character studies with Blender 4.3+: blender -b --python this.py -- --render.

Exports original, joined mobile meshes in Pip's coordinate/anchor contract.
Studio and procedural Blender materials are authoring references, not Unity shaders.
"""
import importlib.util
import json
import math
import sys
from pathlib import Path
import bpy
import bmesh
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
sys.dont_write_bytecode = True
spec = importlib.util.spec_from_file_location("pip_authoring", ROOT / "tools/build-pip.py")
art = importlib.util.module_from_spec(spec)
spec.loader.exec_module(art)


def clear_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for collection in list(bpy.data.collections):
        if collection.users == 0 or not collection.objects: bpy.data.collections.remove(collection)


def join_body(parts, name):
    bpy.ops.object.select_all(action='DESELECT')
    for part in parts: part.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    body = parts[0]; body.name = name + "Body"
    remesh = body.modifiers.new("One continuous toy surface", 'REMESH')
    remesh.mode = 'VOXEL'; remesh.voxel_size = .031; remesh.use_smooth_shade = True
    bpy.ops.object.modifier_apply(modifier=remesh.name)
    smooth = body.modifiers.new("Soft molded joins", 'SMOOTH')
    smooth.factor = .7; smooth.iterations = 4
    bpy.ops.object.modifier_apply(modifier=smooth.name)
    reduce = body.modifiers.new("Mobile triangle budget", 'DECIMATE'); reduce.ratio = .19
    bpy.ops.object.modifier_apply(modifier=reduce.name)
    bm = bmesh.new(); bm.from_mesh(body.data)
    bmesh.ops.triangulate(bm, faces=list(bm.faces))
    # Collapse tiny triangles before export; reject topology damage below.
    for _ in range(150):
        tiny = next((f for f in bm.faces if f.calc_area() < 6e-7), None)
        if tiny is None: break
        bmesh.ops.collapse(bm, edges=[min(tiny.edges, key=lambda e: e.calc_length())])
        bmesh.ops.triangulate(bm, faces=list(bm.faces))
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    if any(not e.is_manifold for e in bm.edges) or any(f.calc_area() < 6e-7 for f in bm.faces):
        raise RuntimeError("Model is not a closed nondegenerate manifold: " + name)
    bm.to_mesh(body.data); bm.free()
    # Blender sphere locations become local coordinates before normalizing.
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    low = min(v.co.z for v in body.data.vertices); high = max(v.co.z for v in body.data.vertices)
    factor = 2 / (high - low); center = (high + low) / 2
    for vertex in body.data.vertices:
        vertex.co = Vector((vertex.co.x * factor, vertex.co.y * factor, (vertex.co.z - center) * factor))
    for face in body.data.polygons: face.use_smooth = True
    body.data.update()
    return body


def export_body(body, name, colors):
    logical = [(float(v.co.x), float(v.co.z), float(-v.co.y)) for v in body.data.vertices]
    positions, uv, groups, triangles = [], [], [], []
    keyed = {}; layer = body.data.uv_layers.new(name=name + "UV")
    if name == "Nook":
        bpy.ops.object.select_all(action='DESELECT'); body.select_set(True); bpy.context.view_layer.objects.active = body
        bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
        bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=.015, scale_to_bounds=True)
        bpy.ops.object.mode_set(mode='OBJECT')
    for polygon in body.data.polygons:
        us = [(math.atan2(logical[i][2], logical[i][0]) / (2 * math.pi)) % 1 for i in polygon.vertices]
        if max(us) - min(us) > .5: us = [u + 1 if u < .5 else u for u in us]
        if max(us) > 1: us = [u - min(us) for u in us]
        for vertex, loop, u in zip(polygon.vertices, polygon.loop_indices, us):
            v = (logical[vertex][1] + 1) / 2
            if name == "Nook": u, v = layer.data[loop].uv
            else: layer.data[loop].uv = (u, v)
            key = (vertex, round(u, 7), round(v, 7))
            if key not in keyed:
                keyed[key] = len(positions); positions.append(logical[vertex]); uv.append((u, v)); groups.append(vertex)
            triangles.append(keyed[key])
    def project(x, y, offset):
        hit, location, _, _ = body.ray_cast(Vector(art.blender_point((x, y, -2))), Vector((0, -1, 0)))
        if not hit: raise RuntimeError("Face anchor missed " + name)
        return (x, y, -float(location.y) - offset)
    face_y = {"Moss": -.10, "Bop": .16, "Nook": .04}[name]
    eyes = [project(side * .265, face_y, .034) for side in [-1, 1]]
    glints = [(x - .022, y + .038, z - .038) for x, y, z in eyes]
    cheeks = [project(side * .415, face_y - .15, .027) for side in [-1, 1]]
    mouth = [project((i / 12 - .5) * .19, face_y - .17 + .06 * ((i / 12 - .5) * 2) ** 2, .043) for i in range(13)]
    # Pip importer retains its old fixed anchor lengths for compatibility. Opaque
    # studies have no crown accents or filling; placeholders are never instantiated.
    data = dict(schemaVersion=1, name=name + " - draft material study", characterId=name.lower(), integratedCrown=True,
                positions=art.flat(positions), triangles=triangles, uv=art.flat(uv), normalGroups=groups,
                eyes=art.flat(eyes), eyeGlints=art.flat(glints), cheeks=art.flat(cheeks), mouth=art.flat(mouth),
                crowns=[0, .6, 0, 0, .7, 0], crownScales=[.1] * 6,
                pearls=[0] * 18, flecks=[0] * 18,
                bodyColor=[*colors[0], 1], topColor=[*colors[1], 1], bottomColor=[*colors[2], 1])
    output = ROOT / "Assets/Pockle/Resources/Characters" / name; output.mkdir(parents=True, exist_ok=True)
    (output / (name + ".pocklemesh")).write_text(json.dumps(data, separators=(',', ':')) + '\n')
    return data


def pillow_body(finish, character):
    bpy.ops.mesh.primitive_cube_add(size=2)
    body = bpy.context.object; body.name = "Puffy square cushion"
    body.scale = (.74, .46, .73)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel = body.modifiers.new("Soft pillow corners", 'BEVEL'); bevel.width = .24; bevel.segments = 5
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    subdivide = body.modifiers.new("Padded face", 'SUBSURF'); subdivide.levels = 2
    bpy.ops.object.modifier_apply(modifier=subdivide.name)
    for vertex in body.data.vertices:
        x, depth, y = vertex.co
        puff = .14 * max(0, 1 - (x / .74) ** 2) * max(0, 1 - (y / .73) ** 2)
        vertex.co.y += math.copysign(puff * min(1, abs(depth) / .35), depth)
    for old in list(body.users_collection): old.objects.unlink(body)
    character.objects.link(body); body.data.materials.append(finish)
    return body


def boucle_material(color):
    mat = art.material("Nook oat boucle - looped fabric study", color, 0, .94)
    nodes = mat.node_tree.nodes; links = mat.node_tree.links; bsdf = nodes.get("Principled BSDF")
    bsdf.inputs['Coat Weight'].default_value = 0; bsdf.inputs['Sheen Weight'].default_value = .6
    uv = nodes.new('ShaderNodeTexCoord')
    scale = nodes.new('ShaderNodeVectorMath'); scale.operation = 'SCALE'; scale.inputs[3].default_value = 64
    links.new(uv.outputs['UV'], scale.inputs[0])
    fraction = nodes.new('ShaderNodeVectorMath'); fraction.operation = 'FRACTION'; links.new(scale.outputs['Vector'], fraction.inputs[0])
    center = nodes.new('ShaderNodeVectorMath'); center.operation = 'SUBTRACT'; center.inputs[1].default_value = (.5, .5, 0)
    links.new(fraction.outputs['Vector'], center.inputs[0])
    ellipse = nodes.new('ShaderNodeVectorMath'); ellipse.operation = 'MULTIPLY'; ellipse.inputs[1].default_value = (1, .72, 0)
    links.new(center.outputs['Vector'], ellipse.inputs[0])
    radius = nodes.new('ShaderNodeVectorMath'); radius.operation = 'LENGTH'; links.new(ellipse.outputs['Vector'], radius.inputs[0])
    difference = nodes.new('ShaderNodeMath'); difference.operation = 'SUBTRACT'; difference.inputs[1].default_value = .26
    links.new(radius.outputs['Value'], difference.inputs[0])
    distance = nodes.new('ShaderNodeMath'); distance.operation = 'ABSOLUTE'; links.new(difference.outputs[0], distance.inputs[0])
    loop = nodes.new('ShaderNodeMath'); loop.operation = 'LESS_THAN'; loop.inputs[1].default_value = .07
    links.new(distance.outputs[0], loop.inputs[0])
    bump = nodes.new('ShaderNodeBump'); bump.inputs['Strength'].default_value = .40; bump.inputs['Distance'].default_value = .018
    links.new(loop.outputs[0], bump.inputs['Height']); links.new(bump.outputs['Normal'], bsdf.inputs['Normal'])
    return mat


def build(name):
    clear_scene()
    character = bpy.data.collections.new(name + " - exportable character")
    bpy.context.scene.collection.children.link(character)
    moss = name == "Moss"
    nook = name == "Nook"
    colors = ((.70, .53, .39), (.88, .72, .54), (.49, .33, .23)) if nook else (((.48, .65, .36), (.72, .82, .50), (.28, .45, .22)) if moss else ((.97, .58, .20), (1, .78, .38), (.84, .33, .13)))
    finish = boucle_material(colors[0]) if nook else art.material(name + (" short flock study" if moss else " coated vinyl study"), colors[0], 0, .88 if moss else .20)
    bsdf = finish.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Coat Weight"].default_value = 0 if moss or nook else .42
    if moss:
        bsdf.inputs["Sheen Weight"].default_value = .65
        noise = finish.node_tree.nodes.new('ShaderNodeTexNoise'); noise.inputs['Scale'].default_value = 145
        bump = finish.node_tree.nodes.new('ShaderNodeBump'); bump.inputs['Strength'].default_value = .35; bump.inputs['Distance'].default_value = .025
        finish.node_tree.links.new(noise.outputs['Fac'], bump.inputs['Height'])
        finish.node_tree.links.new(bump.outputs['Normal'], bsdf.inputs['Normal'])
    shapes = [
        ((0, -.23, 0), (.68, .69, .55)),
        ((-.36, .39, .04), (.53, .32, .58)),
        ((.36, .39, .04), (.53, .32, .58)),
        ((0, .67, .06), (.31, .30, .42)),
        ((-.28, -.83, -.02), (.25, .22, .34)),
        ((.28, -.83, -.02), (.25, .22, .34))
    ] if moss else [
        ((0, .24, 0), (.98, .59, .64)),
        ((0, -.22, 0), (.46, .54, .44)),
        ((-.34, -.83, -.02), (.29, .24, .38)),
        ((.34, -.83, -.02), (.29, .24, .38)),
        ((0, .85, 0), (.17, .20, .17))
    ]
    if nook: shapes = []
    parts = [art.sphere("Molded part " + str(i), point, scale, finish, character) for i, (point, scale) in enumerate(shapes)]
    if nook:
        # Nook has an original square cushion, not the Bop or Moss silhouette.
        parts = [pillow_body(finish, character)]
        parts += [art.sphere("Tucked corner paw", (x * .60, y * .56, -.13), (.24, .24, .33), finish, character)
                  for x in [-1, 1] for y in [-1, 1]]
    body = join_body(parts, name)
    data = export_body(body, name, colors)
    plum = art.material("Plum eyes", (.075, .012, .04), 0, .20)
    cream = art.material("Cream glints", (1, .96, .82), 0, .25)
    blush = art.material("Warm cheeks", (.95, .34, .30), 0, .65)
    for key, scale, mat in [("eyes", (.092, .127, .042), plum), ("eyeGlints", (.020, .024, .012), cream), ("cheeks", (.115, .06, .025), blush)]:
        points = data[key]
        for i in range(0, len(points), 3): art.sphere(key + str(i), points[i:i+3], scale, mat, character)
    curve = bpy.data.curves.new(name + " smile", 'CURVE'); curve.dimensions = '3D'
    curve.bevel_depth = .009; curve.bevel_resolution = 2
    spline = curve.splines.new('POLY'); spline.points.add(12)
    for i, point in enumerate(spline.points): point.co = (*art.blender_point(data['mouth'][i*3:i*3+3]), 1)
    smile = bpy.data.objects.new(name + " smile", curve); character.objects.link(smile); curve.materials.append(plum)
    bpy.ops.object.select_all(action='DESELECT'); smile.select_set(True); bpy.context.view_layer.objects.active = smile
    bpy.ops.object.convert(target='MESH')
    bpy.ops.object.select_all(action='DESELECT')
    for obj in character.objects: obj.select_set(True)
    bpy.context.view_layer.objects.active = body
    exports = ROOT / "Assets/Pockle/Art" / name; exports.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.fbx(filepath=str(exports / (name + ".fbx")), use_selection=True, axis_forward='-Z', axis_up='Y', bake_anim=False, object_types={'MESH'}, add_leaf_bones=False)
    bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, -1.005))
    bpy.context.object.name = "Studio - not exported"
    bpy.context.object.data.materials.append(art.material("Cream studio", (.88, .83, .75), 0, .65))
    for light_name, location, energy, size in [('Key', (-3, 4, 5), 650, 4), ('Fill', (4, 1, 3), 300, 3), ('Back', (0, -4, 4), 700, 3)]:
        light = bpy.data.lights.new(light_name, 'AREA'); light.energy = energy; light.size = size
        obj = bpy.data.objects.new(light_name, light); bpy.context.scene.collection.objects.link(obj)
        obj.location = location; art.look_at(obj, (0, 0, 0))
    camera_data = bpy.data.cameras.new("Study camera"); camera_data.type = 'ORTHO'; camera_data.ortho_scale = 3.15
    camera = bpy.data.objects.new("Study camera", camera_data); bpy.context.scene.collection.objects.link(camera)
    camera.location = (1.3, 6, 1.6); art.look_at(camera, (0, 0, -.02))
    scene = bpy.context.scene; scene.camera = camera; scene.world.use_nodes = True
    scene.world.node_tree.nodes['Background'].inputs['Color'].default_value = (.95, .90, .81, 1)
    scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value = .4
    scene.render.engine = 'CYCLES'; scene.cycles.samples = 96; scene.cycles.use_denoising = False
    scene.render.threads_mode = 'FIXED'; scene.render.threads = 4
    scene.render.resolution_x = 640; scene.render.resolution_y = 640; scene.render.resolution_percentage = 100
    scene.view_settings.view_transform = 'AgX'; scene.render.image_settings.file_format = 'PNG'
    previews = ROOT / "docs/concepts/roster-studies"; previews.mkdir(parents=True, exist_ok=True)
    scene.render.filepath = str(previews / (name.lower() + "-study-01.png"))
    source = ROOT / "ArtSource" / name; source.mkdir(parents=True, exist_ok=True)
    if nook:
        foam = art.material("Nook lilac mochi foam study", (.70, .60, .82), 0, .82)
        foam.node_tree.nodes.get('Principled BSDF').inputs['Coat Weight'].default_value = 0
        foam.use_fake_user = True
    bpy.ops.wm.save_as_mainfile(filepath=str(source / (name + ".blend")))
    print('CHARACTER_EXPORT ' + json.dumps(dict(name=name, vertices=len(data['positions'])//3, triangles=len(data['triangles'])//3)))
    if '--render' in sys.argv: bpy.ops.render.render(write_still=True)
    if nook and '--render' in sys.argv:
        body.data.materials.clear(); body.data.materials.append(foam)
        scene.render.filepath = str(previews / 'nook-foam-study-01.png')
        bpy.ops.render.render(write_still=True)


names = ['Moss', 'Bop', 'Nook']
if '--character' in sys.argv:
    selected = sys.argv[sys.argv.index('--character') + 1]
    if selected not in names: raise ValueError('Unknown character: ' + selected)
    names = [selected]
for name in names: build(name)
