"""Run through Blender MCP. New scenes only; renders are queued on Blender's timer."""
import bpy, math, os
from mathutils import Vector
ROOT = r'C:\Users\Winka\OneDrive\Documents\Arcane-Brigade'
OUT = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'MienCho', 'Journey')
os.makedirs(OUT, exist_ok=True)
original = bpy.context.window.scene
scene = bpy.data.scenes.new('MienCho_ThreeJourneys')
bpy.context.window.scene = scene
scene.render.engine = 'CYCLES'
scene.cycles.samples = 24
scene.render.image_settings.file_format = 'PNG'
scene.render.image_settings.color_mode = 'RGBA'
scene.view_settings.view_transform = 'AgX'
scene.world = bpy.data.worlds.new('The Night Between Rooms')
scene.world.use_nodes = True
scene.world.node_tree.nodes['Background'].inputs[0].default_value = (.025,.04,.09,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value = .3
def mat(name, color, metal=0, emit=0):
    m=bpy.data.materials.new(name); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*color,1)
    p.inputs['Metallic'].default_value=metal; p.inputs['Roughness'].default_value=.42
    p.inputs['Emission Color'].default_value=(*color,1); p.inputs['Emission Strength'].default_value=emit
    return m
stone=mat('Blue Slate',(.055,.075,.105)); wood=mat('Old Walnut',(.19,.065,.025))
brass=mat('Worn Brass',(.55,.32,.07),.65); warm=mat('Chosen Warmth',(.9,.42,.09),0,3)
dark=mat('Velvet Without A Face',(.012,.009,.02)); red=mat('The Unanswered Voice',(.7,.025,.015),0,4)
paper=mat('Unaltered Evidence',(.65,.55,.36)); blue=mat('Other People Light',(.06,.46,.65),0,2)
cloth=mat('One Person Linen',(.34,.27,.22)); objects=[]
def keep(o,material):
    objects.append(o)
    if material: o.data.materials.append(material)
    return o
def box(name,loc,size,material):
    bpy.ops.mesh.primitive_cube_add(size=2,location=loc); o=bpy.context.object; o.name=name; o.scale=size
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    b=o.modifiers.new('Soft Old Corners','BEVEL'); b.width=.04; b.segments=2
    return keep(o,material)
def ball(name,loc,size,material):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=loc)
    o=bpy.context.object; o.name=name; o.scale=size; return keep(o,material)
def text(words,loc,size=.3,material=paper):
    bpy.ops.object.text_add(location=loc,rotation=(math.pi/2,0,0)); o=bpy.context.object
    o.data.body=words; o.data.align_x='CENTER'; o.data.size=size; o.data.extrude=.008; return keep(o,material)
def chair(x,y):
    box('The Chair Can Be Moved',(x,y,.8),(.5,.5,.08),wood)
    for a in (-.4,.4):
        for b in (-.4,.4): box('Chair Leg',(x+a,y+b,.4),(.045,.045,.4),wood)
    for a in (-.42,.42): box('Chair Back',(x+a,y+.4,1.4),(.05,.05,.8),wood)
    for z in (1.2,1.6,1.9): box('Back Slat',(x,y+.4,z),(.45,.045,.035),wood)
def light(name,loc,color,power,size):
    d=bpy.data.lights.new(name,'AREA'); d.energy=power; d.color=color; d.shape='DISK'; d.size=size
    o=bpy.data.objects.new(name,d); scene.collection.objects.link(o); o.location=loc
    o.rotation_euler=(Vector((0,0,.5))-o.location).to_track_quat('-Z','Y').to_euler()
def shell():
    box('Floor',(0,0,-.15),(8,6,.15),stone)
    box('Far Wall',(0,5.6,1.7),(8,.16,1.8),stone)
    for x in range(-7,8,2): box('Wall Pier',(x,5.35,1.6),(.1,.12,1.7),wood)
groups={}
shell()
for y in (-1.8,0,1.8):
    box('Rail',(0,y,.025),(7,.055,.035),brass)
    for x in range(-7,8): box('Sleeper',(x,y,.015),(.08,.45,.025),wood)
for x in (-5.8,5.8):
    box('Platform',(x,0,.18),(.7,4,.18),stone)
    for y in (-2,0,2): chair(x,y)
box('Timetable',(0,5.12,2.3),(3.6,.12,1.1),dark)
text('WHEN THEY RETURN',(0,4.97,2.8),.42)
text('WHEN YOU ARE GOOD ENOUGH',(0,4.97,2.15),.32)
text('NO ARRIVAL TIME',(0,4.97,1.55),.38,red)
groups['Station']=objects[:]; objects.clear()
shell(); box('One Bed',(0,3,.5),(1,1.7,.5),wood); box('Linen',(0,3,1.03),(1,1.7,.08),cloth)
box('Single Pillow',(0,4,1.2),(.8,.45,.16),paper); chair(-4,2)
box('One Cup Table',(4,2,.9),(.7,.7,.1),wood); ball('Only Cup',(4,2,1.15),(.2,.2,.23),paper)
box('Window Refuses To Open',(0,5.35,2),(1.8,.1,1.5),warm)
for x in (-1.7,0,1.7): box('Window Bar',(x,5.16,2),(.06,.06,1.5),wood)
groups['PerfectRoom']=objects[:]; objects.clear()
shell()
for x in (-5,5):
    for y in (-3,0,3):
        box('Door Frame',(x,y,1.4),(.7,.14,1.4),wood)
        box('Sealed Door',(x,y-.16,1.4),(.56,.08,1.25),dark)
for i in range(8):
    ball('Footprint',(math.sin(i)*.25,-3+i*.9,.015),(.13,.23,.008),paper)
    box('Growing Height Mark',(-6.5,4.9,.8+i*.18),(.2,.02,.014),paper)
chair(0,4)
groups['FootstepHall']=objects[:]; objects.clear()
# The entity is a deliberately empty embrace: a coat around an unoccupied chair.
chair(0,0)
body=ball('Coat Of Patient Night',(0,.35,1.8),(1.05,.48,1.65),dark)
head=ball('The Face It Learned',(0,.35,3.55),(.58,.4,.7),dark)
arms=[]
for side in (-1,1):
    a=ball('An Invitation With No Release',(side*1.05,.1,2.2),(.3,.34,1.1),dark); a.rotation_euler[1]=side*.45; arms.append(a)
    ball('Palm',(side*1.5,-.1,1.5),(.35,.25,.17),paper)
for x in (-.18,.18): ball('Red Attention',(x,-.04,3.64),(.07,.04,.035),red)
box('A Door Behind The Coat',(0,1.15,2.0),(1.3,.08,2),wood)
groups['Presence']=objects[:]; objects.clear()
for x in (-3.8,3.8):
    box('Open Gate Pillar',(x,0,1.8),(.35,.35,1.8),brass)
box('Register Of Chosen Names',(0,0,1.4),(2.5,.2,1.4),dark)
text('NO NAME IS PROPERTY',(0,-.24,1.8),.34,blue)
for x in (-1.4,0,1.4): ball('Seal',(x,-.3,.9),(.18,.08,.18),warm)
groups['GateOfNames']=objects[:]; objects.clear()
camera_data=bpy.data.cameras.new('Journey Orthographic'); camera=bpy.data.objects.new('Journey Camera',camera_data)
scene.collection.objects.link(camera); scene.camera=camera; camera_data.type='ORTHO'
light('Warm Witness',(-4,-5,8),(1,.62,.32),1500,6); light('The Far Night',(5,2,7),(.15,.4,1),1800,5)
queue=[]
for name in ('Station','PerfectRoom','FootstepHall'): queue.append((name,name,False,0))
for pose in range(4): queue.append(('Presence','Presence'+str(pose),True,pose))
queue.append(('GateOfNames','GateOfNames',True,0))
def render_next():
    if not queue:
        bpy.context.window.scene=scene
        bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Tools','Blender','MienCho_ThreeJourneys.blend'),copy=True)
        bpy.context.window.scene=original
        open(os.path.join(OUT,'Render_Complete.txt'),'w').write('8 Cycles renders completed via Blender MCP timer')
        return None
    group,name,alpha,pose=queue.pop(0); bpy.context.window.scene=scene
    for key,obs in groups.items():
        for o in obs: o.hide_render=key!=group; o.hide_set(key!=group)
    scene.render.film_transparent=alpha
    scene.render.resolution_x=768 if alpha else 1280; scene.render.resolution_y=768 if alpha else 960
    scene.render.resolution_percentage=100
    camera.location=(0,-10,7) if alpha else (0,-13,17)
    target=Vector((0,0,2)) if alpha else Vector((0,0,0))
    camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler()
    camera_data.ortho_scale=6 if alpha else 18.5
    body.scale.x=1.05*(1+pose*.035); head.rotation_euler[2]=[0,.1,-.1,0][pose]
    for side,a in zip((-1,1),arms): a.rotation_euler[1]=side*(.45+[0,.3,-.15,.12][pose])
    scene.render.filepath=os.path.join(OUT,name+'.png'); bpy.ops.render.render(write_still=True)
    return .2
bpy.context.window.scene=original
bpy.app.timers.register(render_next,first_interval=.5)
print('Eight renders queued in isolated scene; original scene preserved.')
