"""Rebuild Pip's settled-belly art pass with Blender 4.3+. Run via blender --background --python.

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
# Sculpt a settled lower belly, with a shallow flat centre and rounded corners.
# The full pear alone reads as a pointed egg; do not trim it to a tiny pole patch.
BASE_COSINE = -.995
CONTACT_CUT = -.985
BELLY_JOIN = -.10
BODY_TOP = .68
THETA_END = math.acos(BASE_COSINE)


def blender_point(point):
    x, y, z = point
    return (x, -z, y)


def body_point(theta, phi):
    c = math.cos(theta)
    radius = math.sin(theta)
    y = -1 + max(0, (c - BASE_COSINE) / (1 - BASE_COSINE)) ** .8 * (BODY_TOP + 1)
    if y < BELLY_JOIN:
        u = (y + 1) / (BELLY_JOIN + 1)
        # Zero slope at the floor and unit slope where the pear resumes:
        # lower volume settles into the support instead of tapering to a point.
        y = -1 + (BELLY_JOIN + 1) * (4 * u ** 4 - 3 * u ** 5)
    return (.84 * (1 - .42 * c) * radius * math.cos(phi), y,
            .65 * (1 - .22 * c) * radius * math.sin(phi))


def front_surface(x, y, offset):
    # Invert the authored profile, including the settled belly. Final anchors
    # are ray-projected again after the crown union and mobile reduction.
    lo, hi = 0, THETA_END
    for _ in range(40):
        mid = (lo + hi) / 2
        if body_point(mid, 0)[1] > y: lo = mid
        else: hi = mid
    width = body_point((lo + hi) / 2, 0)[0]
    depth = body_point((lo + hi) / 2, math.pi / 2)[2]
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

    positions = [(0, BODY_TOP, 0)]
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

    eyes = [front_surface(side * .335, -.055, .038) for side in [-1, 1]]
    glints = [(x + .026, y + .045, z - .043) for x, y, z in eyes]
    cheeks = [front_surface(side * .46, -.22, .028) for side in [-1, 1]]
    mouth = [front_surface((i / 12 - .5) * .17, -.175 + .052 * ((i / 12 - .5) * 2) ** 2, .043)
             for i in range(13)]
    crowns = [(-.20, .74, .015), (.24, .755, .015)]
    crown_scales = [(.165, .165, .16), (.24, .24, .225)]
    pearls = [(-.43, -.48, -.37), (.08, -.64, -.38), (.47, -.30, -.25),
              (-.16, -.75, -.24), (.29, -.76, -.18), (-.48, -.12, -.18)]
    flecks = [(-.24, .26, -.36), (.38, -.32, -.36), (-.49, -.58, -.34),
              (.36, -.65, -.29), (-.20, -.79, -.26), (.16, -.42, -.43)]
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
    # Voxel union removes intersecting transparent shells. Smooth before reducing,
    # keeping the crown in the SAME closed mesh and deformation as the body.
    crown_objects = [sphere("Crown " + str(i), point, scale, jelly, character)
                     for i, (point, scale) in enumerate(zip(crowns, crown_scales))]
    bpy.ops.object.select_all(action='DESELECT')
    for obj in [body, *crown_objects]: obj.select_set(True)
    bpy.context.view_layer.objects.active = body
    bpy.ops.object.join()
    union = body.modifiers.new("Joined jelly shell", 'REMESH')
    union.mode = 'VOXEL'; union.voxel_size = .032; union.use_smooth_shade = True
    bpy.ops.object.modifier_apply(modifier=union.name)
    smooth = body.modifiers.new("Soft crown transitions", 'SMOOTH')
    smooth.factor = .8; smooth.iterations = 5
    bpy.ops.object.modifier_apply(modifier=smooth.name)
    reduce = body.modifiers.new("Mobile surface budget", 'DECIMATE')
    reduce.ratio = .22
    bpy.ops.object.modifier_apply(modifier=reduce.name)
    # Level the shallow resting centre; the surrounding belly keeps its roundover.
    import bmesh
    bm = bmesh.new(); bm.from_mesh(body.data)
    cut = bmesh.ops.bisect_plane(bm, geom=list(bm.verts) + list(bm.edges) + list(bm.faces),
                                dist=.000001, plane_co=(0, 0, CONTACT_CUT), plane_no=(0, 0, 1), clear_inner=True)
    boundary = [edge for edge in cut['geom_cut'] if isinstance(edge, bmesh.types.BMEdge) and edge.is_boundary]
    bmesh.ops.holes_fill(bm, edges=boundary, sides=0)
    bmesh.ops.triangulate(bm, faces=list(bm.faces))
    # Cutting triangulated sides can leave nearly collinear slivers at the foot.
    # Collapse them while maintaining a closed surface, then triangulate again.
    bmesh.ops.dissolve_degenerate(bm, dist=.0005, edges=list(bm.edges))
    bmesh.ops.triangulate(bm, faces=list(bm.faces))
    for _ in range(20):
        slivers = [face for face in bm.faces if face.calc_area() < .0000006]
        if not slivers: break
        edge = min(slivers[0].edges, key=lambda item: item.calc_length())
        bmesh.ops.collapse(bm, edges=[edge])
        bmesh.ops.triangulate(bm, faces=list(bm.faces))
    if any(face.calc_area() < .0000006 for face in bm.faces):
        raise RuntimeError('Contact-patch cleanup left degenerate triangles')
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(body.data); bm.free()
    # Move the contact patch onto the unchanged y=-1 runtime contact plane.
    for vertex in body.data.vertices: vertex.co.z += -1 - CONTACT_CUT
    positions = [(float(v.co.x), float(v.co.z), float(-v.co.y)) for v in body.data.vertices]
    positions = [(x, -1.0 if abs(y + 1) < .00001 else y, z) for x, y, z in positions]
    for vertex, point in zip(body.data.vertices, positions): vertex.co = blender_point(point)
    faces = [tuple(polygon.vertices) for polygon in body.data.polygons]
    # Mesh UVs are cylindrical, with split seam IDs retained for runtime welding.
    export_positions, export_uv, groups, indices = [], [], [], []
    keyed_vertices = {}
    per_face_uv = []
    for face in faces:
        us = [(math.atan2(positions[i][2], positions[i][0]) / (2 * math.pi)) % 1 for i in face]
        if max(us) - min(us) > .5: us = [u + 1 if u < .5 else u for u in us]
        # This mesh has no textures: keep seam-crossing UVs in range with a local
        # continuous chart. Logical IDs still weld the normals across chart edges.
        if max(us) > 1: us = [u - min(us) for u in us]
        uvs = [(u, (positions[i][1] + 1) / 2) for i, u in zip(face, us)]
        per_face_uv.append(uvs)
        for vertex, (u, v) in zip(face, uvs):
            key = (vertex, round(u, 7), round(v, 7))
            if key not in keyed_vertices:
                keyed_vertices[key] = len(export_positions)
                export_positions.append(positions[vertex]); export_uv.append((u, v)); groups.append(vertex)
            indices.append(keyed_vertices[key])
    mesh = body.data
    for face in mesh.polygons: face.use_smooth = True
    uv_layer = mesh.uv_layers.new(name="PipUV")
    for polygon, uvs in zip(mesh.polygons, per_face_uv):
        for loop, uv in zip(polygon.loop_indices, uvs): uv_layer.data[loop].uv = uv
    # Ray-project facial marks onto the final smoothed shell, rather than keeping
    # anchors calculated against the pre-union analytic pear.
    def project(point, offset):
        x, y, _ = point
        hit, location, _, _ = body.ray_cast(Vector(blender_point((x, y, -2))), Vector((0, -1, 0)))
        if not hit: raise RuntimeError('Face anchor missed joined body')
        return (x, y, -float(location.y) - offset)
    eyes = [project(point, .034) for point in eyes]
    glints = [project((x + .026, y + .045, 0), .085) for x, y, z in eyes]
    cheeks = [project(point, .025) for point in cheeks]
    mouth = [project(point, .043) for point in mouth]
    data = dict(schemaVersion=1, name="Pip - Peach Jelly", integratedCrown=True,
                positions=flat(export_positions), triangles=indices, uv=flat(export_uv), normalGroups=groups,
                eyes=flat(eyes), eyeGlints=flat(glints), cheeks=flat(cheeks), mouth=flat(mouth),
                crowns=flat([(x, y - 1 - CONTACT_CUT, z) for x, y, z in crowns]), crownScales=flat(crown_scales), pearls=flat(pearls), flecks=flat(flecks),
                bodyColor=[1, .65, .40, .44], topColor=[1, .85, .66, 1], bottomColor=[1, .30, .16, 1])
    (UNITY_DIR / "Pip.pocklemesh").write_text(json.dumps(data, separators=(',', ':')) + '\n')
    for i, eye in enumerate(eyes): sphere("Eye " + str(i), eye, (.102, .14, .045), plum, character)
    for i, point in enumerate(glints): sphere("Glint " + str(i), point, (.020, .024, .012), cream, character)
    for i, point in enumerate(cheeks): sphere("Blush " + str(i), point, (.20, .112, .026), blush, character)
    for i, point in enumerate(pearls):
        radius = .062 + (i % 3) * .014
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
    scene.render.filepath = str(PREVIEW_DIR / "pip-model-study-03.png")
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE_DIR / "Pip.blend"))
    print('PIP_EXPORT ' + json.dumps(dict(logicalVertices=len(positions), unityVertices=len(export_positions),
                                       triangles=len(indices) // 3, characterObjects=len(character.objects))))
    if '--render' in sys.argv: bpy.ops.render.render(write_still=True)


if __name__ == '__main__': main()
