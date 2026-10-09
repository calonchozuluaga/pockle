"""Validate authored mobile studies without Unity or Blender. Run from any directory."""
import collections
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def triples(values):
    assert len(values) % 3 == 0
    return [tuple(values[i:i+3]) for i in range(0, len(values), 3)]


def sub(a, b): return tuple(x-y for x, y in zip(a, b))
def cross(a, b): return (a[1]*b[2]-a[2]*b[1], a[2]*b[0]-a[0]*b[2], a[0]*b[1]-a[1]*b[0])
def dot(a, b): return sum(x*y for x, y in zip(a, b))


def check(name):
    data = json.loads((ROOT / 'Assets/Pockle/Resources/Characters' / name / (name + '.pocklemesh')).read_text())
    assert data['schemaVersion'] == 1 and data['characterId'] == name.lower() and data['integratedCrown']
    vertices = triples(data['positions']); indices = data['triangles']; groups = data['normalGroups']
    assert 4 <= len(vertices) <= 4000 and 0 < len(indices) // 3 <= 6000 and len(indices) % 3 == 0
    assert len(groups) == len(vertices) and len(data['uv']) == len(vertices)*2
    assert all(math.isfinite(c) for v in vertices for c in v)
    assert all(-1 <= v[1] <= 1 and abs(v[0]) <= 1.5 and abs(v[2]) <= 1.5 for v in vertices)
    assert all(math.isfinite(c) and 0 <= c <= 1 for c in data['uv'])
    assert min(v[1] for v in vertices) == -1 and max(v[1] for v in vertices) == 1
    edges = collections.defaultdict(list); neighbors = collections.defaultdict(set); group_positions = {}; volume = 0
    for i, group in enumerate(groups):
        assert 0 <= group < len(vertices)
        assert group_positions.setdefault(group, vertices[i]) == vertices[i], 'Normal group spans different positions'
    for offset in range(0, len(indices), 3):
        face = indices[offset:offset+3]; assert all(0 <= i < len(vertices) for i in face)
        a, b, c = (vertices[i] for i in face)
        normal = cross(sub(b, a), sub(c, a)); assert dot(normal, normal) > 1e-12
        volume += dot(a, cross(b, c)) / 6
        for i in range(3):
            start, end = groups[face[i]], groups[face[(i+1)%3]]; assert start != end
            edges[tuple(sorted((start, end)))].append(1 if start < end else -1)
            neighbors[start].add(end); neighbors[end].add(start)
    assert all(len(uses) == 2 and sum(uses) == 0 for uses in edges.values()), 'Not a closed, consistently wound surface'
    assert .5 < volume < 8, 'Invalid winding or implausible volume'
    reached = set(); pending = [groups[0]]
    while pending:
        vertex = pending.pop()
        if vertex in reached: continue
        reached.add(vertex); pending.extend(neighbors[vertex] - reached)
    assert reached == set(groups), 'Disconnected shells'
    # Project each facial anchor onto triangles at the same x/y. It must lie just
    # in front of the nearest surface, rather than embedded or floating away.
    for key, count in [('eyes', 2), ('eyeGlints', 2), ('cheeks', 2), ('mouth', 13)]:
        points = triples(data[key]); assert len(points) == count
        for x, y, z in points:
            surfaces = []
            for i in range(0, len(indices), 3):
                a, b, c = (vertices[index] for index in indices[i:i+3])
                denominator = (b[1]-c[1])*(a[0]-c[0]) + (c[0]-b[0])*(a[1]-c[1])
                if abs(denominator) < 1e-12: continue
                u = ((b[1]-c[1])*(x-c[0]) + (c[0]-b[0])*(y-c[1])) / denominator
                v = ((c[1]-a[1])*(x-c[0]) + (a[0]-c[0])*(y-c[1])) / denominator
                if u >= -1e-6 and v >= -1e-6 and u+v <= 1+1e-6:
                    surfaces.append(u*a[2] + v*b[2] + (1-u-v)*c[2])
            assert surfaces, 'Face anchor misses body'
            gap = min(surfaces) - z
            assert .005 < gap < .15, key + ' embedded or floating: ' + str(gap)
    print(f'PASS: {name}: {len(vertices)} vertices, {len(indices)//3} triangles; closed connected mesh, finite UVs, front face anchors, volume {volume:.3f}')


for name in ['Moss', 'Bop']: check(name)
