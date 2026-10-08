"""Rebuild Pip's first art pass with Blender 4.3+. Run via blender --background --python.

The Unity source has explicit Y-up / negative-Z-front coordinates. Blender uses
Z-up. Export both a standard FBX and a small character source for our Unity importer.
"""
import bpy
import json
import math
import sys
from pathlib import Path
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[1]
SOURCE_DIR = ROOT / "ArtSource" / "Pip"
EXPORT_DIR = ROOT / "Assets" / "Pockle" / "Art" / "Pip"
UNITY_DIR = ROOT / "Assets" / "Pockle" / "Resources" / "Pip"
PREVIEW_DIR = ROOT / "docs" / "concepts"
RINGS = 28
SIDES = 40
BASE_COSINE = -.85
THETA_END = math.acos(BASE_COSINE)


def blender_point(point):
    x, y, z = point
    return (x, -z, y)


def body_point(theta, phi):
    c = math.cos(theta)
    radius = math.sin(theta)
    y = -1 + max(0, (c - BASE_COSINE) / (1 - BASE_COSINE)) ** .8 * 1.75
    return (.86 * (1 - .24 * c) * radius * math.cos(phi), y,
            .64 * (1 - .16 * c) * radius * math.sin(phi))


def front_surface(x, y, offset):
    c = min(1, max(BASE_COSINE, (1 - BASE_COSINE) * ((y + 1) / 1.75) ** (1 / .8) + BASE_COSINE))
    radial = math.sqrt(max(0, 1 - c * c))
    width = .86 * (1 - .24 * c) * radial
    depth = .64 * (1 - .16 * c) * radial
    return (x, y, -depth * math.sqrt(max(0, 1 - (x / width) ** 2)) - offset)


def flat(points):
    return [round(component, 7) for point in points for component in point]


def material(name, color, transmission=0, roughness=.28):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color[:3], 1)
    mat.use_nodes = True
    shader = mat.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color[:3], 1)
    shader.inputs["Roughness"].default_value = roughness
    shader.inputs["Transmission Weight"].default_value = transmission
    shader.inputs["IOR"].default_value = 1.34
    shader.inputs["Coat Weight"].default_value = .28 if transmission else .08
    shader.inputs["Coat Roughness"].default_value = .18
    return mat


def sphere(name, anchor, scale, mat, collection):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=12, location=blender_point(anchor))
    obj = bpy.context.object
    obj.name = name
    obj.scale = (scale[0], scale[2], scale[1])
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    for old in list(obj.users_collection): old.objects.unlink(obj)
    collection.objects.link(obj)
    obj.data.materials.append(mat)
    for face in obj.data.polygons: face.use_smooth = True
    return obj


def look_at(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat('-Z', 'Y').to_euler()


def main():
    for directory in [SOURCE_DIR, EXPORT_DIR, UNITY_DIR, PREVIEW_DIR]: directory.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for datablocks in [bpy.data.meshes, bpy.data.materials, bpy.data.curves]:
        for block in list(datablocks):
            if block.users == 0: datablocks.remove(block)
    character = bpy.data.collections.new("Pip - exportable character")
    bpy.context.scene.collection.children.link(character)

    positions = [(0, .75, 0)]
    positions += [body_point(THETA_END * ring / RINGS, 2 * math.pi * side / SIDES)
                  for ring in range(1, RINGS + 1) for side in range(SIDES)]
    positions.append((0, -1, 0))
    faces = []
    for side in range(SIDES): faces.append((0, 1 + (side + 1) % SIDES, 1 + side))
    for ring in range(RINGS - 1):
        for side in range(SIDES):
            a = 1 + ring * SIDES + side
            b = 1 + ring * SIDES + (side + 1) % SIDES
            c = a + SIDES
            d = b + SIDES
            faces.extend([(a, b, c), (b, d, c)])
    for side in range(SIDES):
        a = 1 + (RINGS - 1) * SIDES + side
        b = 1 + (RINGS - 1) * SIDES + (side + 1) % SIDES
        faces.append((a, b, len(positions) - 1))
    # Correct every winding convention once, using a nondegenerate upper-body face.
    a, b, c = (Vector(positions[i]) for i in faces[SIDES * 5])
    if (b - a).cross(c - a).dot(Vector((a.x, 0, a.z))) < 0:
        faces = [(a, c, b) for a, b, c in faces]

    # Split the UV seam while retaining logical vertex IDs for welded Unity normals.
    export_positions, export_uv, groups, indices = [], [], [], []
    keyed_vertices = {}
    for face in faces:
        us = [((i - 1) % SIDES) / SIDES if i not in [0, len(positions) - 1] else 0 for i in face]
        if max(us) - min(us) > .5: us = [u + 1 if u < .5 else u for u in us]
        for vertex, u in zip(face, us):
            if vertex in [0, len(positions) - 1]:
                u = sum(value for index, value in zip(face, us) if index != vertex) / 2
            v = (positions[vertex][1] + 1) / 1.75
            key = (vertex, round(u, 7), round(v, 7))
            if key not in keyed_vertices:
                keyed_vertices[key] = len(export_positions)
                export_positions.append(positions[vertex]); export_uv.append((u, v)); groups.append(vertex)
            indices.append(keyed_vertices[key])

    eyes = [front_surface(side * .29, .045, .034) for side in [-1, 1]]
    glints = [(x - .022, y + .038, z - .038) for x, y, z in eyes]
    cheeks = [front_surface(side * .435, -.10, .024) for side in [-1, 1]]
    mouth = [front_surface((i / 12 - .5) * .17, -.108 + .053 * ((i / 12 - .5) * 2) ** 2, .043)
             for i in range(13)]
    crowns = [(-.26, .64, .015), (.095, .71, .015)]
    crown_scales = [(.165, .215, .16), (.25, .40, .205)]
    pearls = [(-.39, -.43, -.22), (.12, -.59, -.22), (.47, -.24, -.08),
              (-.11, .36, -.10), (.30, .18, -.14), (-.42, -.05, -.06)]
    flecks = [(-.25, .31, -.30), (.38, .23, -.26), (-.49, -.18, -.30),
              (.37, -.40, -.27), (-.20, -.64, -.24), (.16, -.34, -.36)]
    data = dict(schemaVersion=1, name="Pip - Peach Jelly", positions=flat(export_positions),
                triangles=indices, uv=flat(export_uv), normalGroups=groups,
                eyes=flat(eyes), eyeGlints=flat(glints), cheeks=flat(cheeks), mouth=flat(mouth),
                crowns=flat(crowns), crownScales=flat(crown_scales), pearls=flat(pearls), flecks=flat(flecks),
                bodyColor=[1, .64, .40, .76], topColor=[1, .82, .60, 1], bottomColor=[1, .39, .28, 1])
    (UNITY_DIR / "Pip.pocklemesh").write_text(json.dumps(data, separators=(',', ':')) + '\n')

    jelly = material("Pip peach jelly - Blender material study", (1, .64, .40), .55, .22)
    plum = material("Pip plum eyes", (.09, .025, .058), 0, .24)
    cream = material("Pip cream glints and pearls", (1, .92, .72), .05, .25)
    blush = material("Pip warm blush", (1, .34, .30), 0, .40)
    gold = material("Pip apricot flecks", (1, .62, .15), .1, .20)
    mesh = bpy.data.meshes.new("PipBody - closed deformable pear")
    mesh.from_pydata([blender_point(p) for p in positions], [], faces)
    mesh.update()
    body = bpy.data.objects.new("PipBody", mesh); character.objects.link(body)
    body.data.materials.append(jelly)
    for face in mesh.polygons: face.use_smooth = True
    uv_layer = mesh.uv_layers.new(name="PipUV")
    for polygon in mesh.polygons:
        face = [mesh.loops[i].vertex_index for i in polygon.loop_indices]
        us = [((i - 1) % SIDES) / SIDES if i not in [0, len(positions) - 1] else 0 for i in face]
        if max(us) - min(us) > .5: us = [u + 1 if u < .5 else u for u in us]
        for loop, vertex, u in zip(polygon.loop_indices, face, us):
            uv_layer.data[loop].uv = (u, (positions[vertex][1] + 1) / 1.75)
    for i, eye in enumerate(eyes): sphere("Eye " + str(i), eye, (.092, .127, .042), plum, character)
    for i, point in enumerate(glints): sphere("Glint " + str(i), point, (.020, .024, .012), cream, character)
    for i, point in enumerate(cheeks): sphere("Blush " + str(i), point, (.104, .048, .026), blush, character)
    for i, (point, scale) in enumerate(zip(crowns, crown_scales)): sphere("Crown " + str(i), point, scale, jelly, character)
    for i, point in enumerate(pearls):
        radius = .032 + (i % 3) * .011
        sphere("Pearl " + str(i), point, (radius,) * 3, cream, character)
    for i, point in enumerate(flecks):
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=.024 + (i % 2) * .006, location=blender_point(point))
        obj = bpy.context.object; obj.name = "Apricot fleck " + str(i)
        for old in list(obj.users_collection): old.objects.unlink(obj)
        character.objects.link(obj); obj.data.materials.append(gold)
    curve = bpy.data.curves.new("Pip smile", type='CURVE')
    curve.dimensions = '3D'; curve.resolution_u = 1; curve.bevel_depth = .009; curve.bevel_resolution = 2
    spline = curve.splines.new('POLY'); spline.points.add(len(mouth) - 1)
    for point, rest in zip(spline.points, mouth): point.co = (*blender_point(rest), 1)
    smile = bpy.data.objects.new("Pip smile", curve); character.objects.link(smile); curve.materials.append(plum)
    bpy.ops.object.select_all(action='DESELECT'); smile.select_set(True); bpy.context.view_layer.objects.active = smile
    bpy.ops.object.convert(target='MESH')

    # Export only character objects; the studio never enters the FBX or Unity data.
    bpy.ops.object.select_all(action='DESELECT')
    for obj in character.objects: obj.select_set(True)
    bpy.context.view_layer.objects.active = body
    bpy.ops.export_scene.fbx(filepath=str(EXPORT_DIR / "Pip.fbx"), use_selection=True,
                             axis_forward='-Z', axis_up='Y', global_scale=1,
                             apply_unit_scale=True, bake_anim=False, object_types={'MESH'},
                             use_mesh_modifiers=True, add_leaf_bones=False)

    bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, -1.005))
    floor = bpy.context.object; floor.name = "Studio - excluded from export"
    floor.data.materials.append(material("Warm cream studio", (.88, .83, .75), 0, .6))
    for name, location, energy, size in [('Key', (-3, 4, 5), 600, 4), ('Fill', (4, 1, 3), 350, 3), ('Back', (0, -4, 4), 800, 3)]:
        light = bpy.data.lights.new(name, type='AREA'); light.energy = energy; light.shape = 'DISK'; light.size = size
        obj = bpy.data.objects.new(name, light); bpy.context.scene.collection.objects.link(obj)
        obj.location = location; look_at(obj, (0, 0, 0))
    camera_data = bpy.data.cameras.new("Pip study camera")
    camera = bpy.data.objects.new("Pip study camera", camera_data); bpy.context.scene.collection.objects.link(camera)
    camera.location = (2.6, 6, 2.1); look_at(camera, (0, 0, .02))
    camera_data.type = 'ORTHO'; camera_data.ortho_scale = 3.35
    scene = bpy.context.scene; scene.camera = camera
    scene.world.use_nodes = True
    scene.world.node_tree.nodes['Background'].inputs['Color'].default_value = (.95, .90, .81, 1)
    scene.world.node_tree.nodes['Background'].inputs['Strength'].default_value = .4
    scene.render.engine = 'CYCLES'; scene.cycles.samples = 256; scene.cycles.use_denoising = False
    scene.render.threads_mode = 'FIXED'; scene.render.threads = 4
    scene.render.resolution_x = 768; scene.render.resolution_y = 768; scene.render.resolution_percentage = 100
    scene.view_settings.view_transform = 'AgX'
    scene.render.image_settings.file_format = 'PNG'
    scene.render.filepath = str(PREVIEW_DIR / "pip-model-study-01.png")
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE_DIR / "Pip.blend"))
    print('PIP_EXPORT ' + json.dumps(dict(logicalVertices=len(positions), unityVertices=len(export_positions),
                                       triangles=len(indices) // 3, characterObjects=len(character.objects))))
    if '--render' in sys.argv: bpy.ops.render.render(write_still=True)


if __name__ == '__main__': main()
