"""Run in the live Blender via MCP. Adds an isolated scene; preserves the live scene."""
import bpy
import math
import os
from mathutils import Vector

ROOT = r'C:\Users\Winka\OneDrive\Documents\Arcane-Brigade'
OUTPUT = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'MienCho', 'Encounters')
os.makedirs(OUTPUT, exist_ok=True)
original = bpy.context.window.scene
scene = bpy.data.scenes.new('MienCho_EncounterProps')
bpy.context.window.scene = scene
scene.render.engine = 'CYCLES'
scene.cycles.samples = 32
scene.render.resolution_x = scene.render.resolution_y = 768
scene.render.resolution_percentage = 100
scene.render.film_transparent = True
scene.render.image_settings.file_format = 'PNG'
scene.render.image_settings.color_mode = 'RGBA'
scene.view_settings.view_transform = 'AgX'
scene.world = bpy.data.worlds.new('Encounter_Dusk')
scene.world.use_nodes = True
background = next(n for n in scene.world.node_tree.nodes if n.type == 'BACKGROUND')
background.inputs[0].default_value = (.13, .19, .3, 1)
background.inputs[1].default_value = .35

def material(name, color, metal=0, rough=.4):
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    shader = next(n for n in m.node_tree.nodes if n.type == 'BSDF_PRINCIPLED')
    shader.inputs['Base Color'].default_value = (*color, 1)
    shader.inputs['Metallic'].default_value = metal
    shader.inputs['Roughness'].default_value = rough
    return m

wood = material('Dark_Grained_Walnut', (.15, .055, .025), 0, .4)
nodes = wood.node_tree.nodes
noise = nodes.new('ShaderNodeTexNoise')
noise.inputs['Scale'].default_value = 7
noise.inputs['Detail'].default_value = 3
bump = nodes.new('ShaderNodeBump')
bump.inputs['Strength'].default_value = .16
bump.inputs['Distance'].default_value = .045
wood.node_tree.links.new(noise.outputs['Fac'], bump.inputs['Height'])
wood.node_tree.links.new(bump.outputs['Normal'], next(n for n in nodes if n.type == 'BSDF_PRINCIPLED').inputs['Normal'])
brass = material('Old_Honey_Brass', (.5, .26, .065), .7, .32)
ivory = material('Warm_Ceramic', (.72, .65, .46), .08, .24)
black = material('Bakelite', (.024, .035, .043), .18, .24)
glass = material('Mirror_Night_Glass', (.02, .09, .13), .55, .21)
wine = material('Burgundy_Cloth', (.21, .025, .045), 0, .8)
paper = material('Unclaimed_Name', (.65, .5, .29), 0, .7)
groups = {}
current = []

def keep(o, name, mat):
    o.name = name
    if mat:
        o.data.materials.append(mat)
    current.append(o)
    return o

def box(name, loc, size, mat):
    bpy.ops.mesh.primitive_cube_add(size=2, location=loc)
    o = bpy.context.object
    o.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel = o.modifiers.new('Rounded_Worn_Edges', 'BEVEL')
    bevel.width = .035
    bevel.segments = 3
    o.modifiers.new('Weighted_Corner_Normals', 'WEIGHTED_NORMAL')
    return keep(o, name, mat)

def sphere(name, loc, size, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=16, location=loc)
    o = bpy.context.object
    o.scale = size
    for polygon in o.data.polygons:
        polygon.use_smooth = True
    return keep(o, name, mat)

def cylinder(name, loc, radius, depth, mat):
    bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=radius, depth=depth, location=loc)
    o = bpy.context.object
    bevel = o.modifiers.new('Rounded_Rim', 'BEVEL')
    bevel.width = .018
    bevel.segments = 3
    return keep(o, name, mat)

def line(name, points, thickness, mat):
    curve = bpy.data.curves.new(name, 'CURVE')
    curve.dimensions = '3D'
    curve.bevel_depth = thickness
    curve.bevel_resolution = 4
    spline = curve.splines.new('POLY')
    spline.points.add(len(points) - 1)
    for vertex, p in zip(spline.points, points):
        vertex.co = (*p, 1)
    o = bpy.data.objects.new(name, curve)
    scene.collection.objects.link(o)
    return keep(o, name, mat)

def chair(x, y, angle=0):
    start = len(current)
    for dx in [-.3, .3]:
        for dy in [-.27, .27]:
            box('Dining_Chair_Leg', (dx, dy, .35), (.04, .04, .35), wood)
    box('Dining_Chair_Seat', (0, 0, .72), (.39, .36, .065), wood)
    for dx in [-.3, .3]:
        box('Dining_Chair_Post', (dx, .28, 1.12), (.045, .045, .48), wood)
    for z in [.98, 1.22, 1.48]:
        box('Dining_Chair_Slat', (0, .28, z), (.3, .035, .045), wood)
    for o in current[start:]:
        p = o.location.copy()
        o.location = (x + p.x * math.cos(angle) - p.y * math.sin(angle), y + p.x * math.sin(angle) + p.y * math.cos(angle), p.z)
        o.rotation_euler.z += angle

try:
    current = []
    box('Table_Top', (0, 0, 1.15), (1.1, .62, .09), wood)
    box('Table_Cloth', (0, -.01, 1.248), (.3, .64, .008), wine)
    for x in [-.88, .88]:
        for y in [-.45, .45]:
            box('Table_Leg', (x, y, .55), (.055, .055, .55), wood)
    for x in [-.65, .65]:
        cylinder('Cup_Saucer', (x, 0, 1.265), .19, .025, ivory)
        cylinder('Cup', (x, 0, 1.36), .105, .17, ivory)
        cylinder('Tea_Surface', (x, 0, 1.45), .088, .005, black)
        line('Cup_Handle', [(x+.1+math.sin(i/24*math.pi)*.08, 0, 1.36+math.cos(i/24*math.pi)*.065) for i in range(25)], .018, brass)
    chair(-1.47, .05, math.pi/2)
    chair(1.47, .05, -math.pi/2)
    chair(0, .97)
    box('Empty_Name_Card', (0, .4, 1.39), (.2, .012, .12), paper)
    bpy.ops.object.text_add(location=(0, .382, 1.355), rotation=(math.pi / 2, 0, 0))
    name_card = bpy.context.object
    name_card.data.body = 'MARA'
    name_card.data.align_x = 'CENTER'
    name_card.data.size = .095
    name_card.data.extrude = .001
    keep(name_card, 'A_Name_Before_An_Arrival', black)
    groups['ReservedTable'] = current[:]

    current = []
    box('Telephone_Cabinet', (0, 0, .48), (.53, .36, .48), wood)
    box('Telephone_Cabinet_Top', (0, 0, .99), (.6, .41, .04), wood)
    sphere('Telephone_Base', (0, 0, 1.11), (.47, .3, .12), black)
    cylinder('Rotary_Dial', (0, -.08, 1.23), .2, .025, brass)
    cylinder('Dial_Centre', (0, -.08, 1.25), .085, .015, black)
    for i in range(10):
        a=i*math.pi*2/10
        cylinder('Dial_Hole', (math.cos(a)*.146, -.08+math.sin(a)*.146, 1.25), .026, .015, black)
    for x in [-.31, .31]:
        box('Receiver_Cradle', (x, .1, 1.32), (.035, .045, .18), brass)
        sphere('Receiver_Earpiece', (x, .1, 1.55), (.15, .13, .085), black)
    line('Telephone_Receiver', [(-.31+i*.62/30, .1, 1.56+math.sin(i/30*math.pi)*.085) for i in range(31)], .065, black)
    line('Telephone_Coil', [(.47+.045*math.cos(i*.55), .1+.045*math.sin(i*.55), 1.45-i*.0037) for i in range(220)], .012, black)
    groups['BreathingPhone'] = current[:]

    current = []
    box('Mirror_Stand', (0, .1, .08), (.57, .35, .065), wood)
    for x in [-.43, .43]:
        box('Mirror_Upright', (x, .12, 1.02), (.045, .07, .92), wood)
        sphere('Mirror_Pivot', (x, .03, 1.35), (.07, .07, .07), brass)
    sphere('Mirror_Glass', (0, 0, 1.4), (.46, .035, .77), glass)
    line('Oval_Mirror_Frame', [(math.cos(i/128*math.pi*2)*.48, -.025, 1.4+math.sin(i/128*math.pi*2)*.8) for i in range(129)], .045, brass)
    for sign in [-1, 1]:
        line('Mirror_Filigree', [(sign*(.48+.06*math.sin(i/40*math.pi*3)), -.01, .75+i/40*1.25) for i in range(41)], .016, brass)
    groups['LateReflection'] = current[:]

    def area(name, position, energy, color, size):
        light = bpy.data.lights.new(name, 'AREA')
        light.energy = energy; light.color = color; light.shape = 'DISK'; light.size = size
        obj = bpy.data.objects.new(name, light); scene.collection.objects.link(obj)
        obj.location = position
        obj.rotation_euler = (Vector((0,0,1))-obj.location).to_track_quat('-Z','Y').to_euler()
    area('Warm_Shared_Key', (-3,-4,5), 650, (1,.68,.4), 4)
    area('Cold_Corridor_Rim', (2,2,4), 850, (.35,.65,1), 3)
    camera_data = bpy.data.cameras.new('Encounter_Orthographic')
    camera = bpy.data.objects.new('Encounter_Orthographic', camera_data)
    scene.collection.objects.link(camera); scene.camera = camera
    camera_data.type = 'ORTHO'
    for name, objects in groups.items():
        for group, members in groups.items():
            for o in members:
                o.hide_render = group != name
        camera.location = (3,-6,4) if name != 'LateReflection' else (0,-7,3.1)
        target = Vector((0,0,1.05 if name != 'LateReflection' else 1.15))
        camera.rotation_euler = (target-camera.location).to_track_quat('-Z','Y').to_euler()
        camera_data.ortho_scale = 4.6 if name == 'ReservedTable' else 2.7
        scene.render.filepath = os.path.join(OUTPUT, name+'.png')
        bpy.ops.render.render(write_still=True, scene=scene.name)
    scene['provenance'] = 'Procedural Blender meshes, own materials and lights; no external source assets.'
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT, 'Tools','Blender','MienCho_EncounterProps.blend'), copy=True)
    print('Three transparent encounter sprites rendered; isolated editable scene saved.')
finally:
    bpy.context.window.scene = original
