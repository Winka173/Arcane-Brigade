"""Procedural antique street lamps; live Blender MCP, independent scene, editable FBX + Cycles sprite previews."""
import bpy, math, os
from mathutils import Vector
ROOT=r'C:\Users\Winka\OneDrive\Documents\Arcane-Brigade'
OUT=os.path.join(ROOT,'Assets','ArcaneBrigade','Resources','MienCho','Lanterns')
os.makedirs(OUT,exist_ok=True)
original=bpy.data.scenes.get('Scene') or bpy.context.window.scene
scene=bpy.data.scenes.new('Miền Chờ - Antique Lanterns');bpy.context.window.scene=scene
scene.render.engine='CYCLES';scene.cycles.samples=40
scene.render.resolution_x=scene.render.resolution_y=768;scene.render.resolution_percentage=100
scene.render.film_transparent=True;scene.render.image_settings.file_format='PNG';scene.render.image_settings.color_mode='RGBA'
scene.view_settings.view_transform='AgX';scene.world=bpy.data.worlds.new('Midnight Copper');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.02,.035,.07,1);scene.world.node_tree.nodes['Background'].inputs[1].default_value=.5
def mat(name,c,metal=0,emit=0):
 m=bpy.data.materials.new(name);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF')
 p.inputs['Base Color'].default_value=(*c,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=.32
 p.inputs['Emission Color'].default_value=(*c,1);p.inputs['Emission Strength'].default_value=emit
 return m
iron=mat('Midnight Wrought Iron',(.027,.04,.055),.8);brass=mat('Worn Copper Edges',(.32,.135,.045),.7)
patina=mat('Oxidised Copper',(.05,.18,.16),.65);glass=mat('Honey Lantern Glass',(.85,.27,.035),.15,2.4)
soot=mat('Soot Black Glass',(.017,.012,.014),.2);flame=mat('Living Flame', (1,.54,.12),0,8)
pieces=[]
def keep(o,m):
 pieces.append(o);o.data.materials.append(m);return o
def cone(name,z,r1,r2,depth,m):
 bpy.ops.mesh.primitive_cone_add(vertices=8,radius1=r1,radius2=r2,depth=depth,location=(0,0,z));o=bpy.context.object;o.name=name
 bevel=o.modifiers.new('Rounded Metal Edges','BEVEL');bevel.width=.012;bevel.segments=2
 return keep(o,m)
def ball(name,loc,scale,m):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,location=loc);o=bpy.context.object;o.name=name;o.scale=scale;return keep(o,m)
def curve(name,coords,m,width=.027):
 d=bpy.data.curves.new(name,'CURVE');d.dimensions='3D';d.bevel_depth=width;d.bevel_resolution=3
 sp=d.splines.new('BEZIER');sp.bezier_points.add(len(coords)-1)
 for p,co in zip(sp.bezier_points,coords):p.co=co;p.handle_left_type=p.handle_right_type='AUTO'
 o=bpy.data.objects.new(name,d);scene.collection.objects.link(o);return keep(o,m)
cone('Octagonal Street Foot',.1,.34,.29,.2,iron);cone('Copper Foot Rim',.2,.3,.27,.05,brass)
cone('Fluted Column',1.05,.11,.065,1.7,iron)
for z in (.36,.48,1.55,1.7):cone('Cast Collar',z,.14,.13,.06,brass)
for i in range(8):
 a=i*math.pi/4;curve('Column Flute',[(math.cos(a)*.09,math.sin(a)*.09,.5),(math.cos(a)*.066,math.sin(a)*.066,1.55)],brass,.009)
for side in (-1,1):
 curve('Hand-Forged Scroll',[(side*.08,0,1.63),(side*.4,0,1.9),(side*.44,0,2.1),(side*.27,0,2.15),(side*.22,0,2.03)],patina,.028)
cone('Lantern Bowl',1.95,.13,.38,.25,iron);cone('Bowl Copper Lip',2.09,.38,.38,.06,brass)
panes=[]
for i in range(6):
 a=i*math.pi/3
 curve('Lantern Cage Rib',[(math.cos(a)*.33,math.sin(a)*.33,2.1),(math.cos(a)*.29,math.sin(a)*.29,2.77)],brass,.025)
 b=a+math.pi/3
 verts=[(math.cos(a)*.31,math.sin(a)*.31,2.14),(math.cos(b)*.31,math.sin(b)*.31,2.14),(math.cos(b)*.275,math.sin(b)*.275,2.74),(math.cos(a)*.275,math.sin(a)*.275,2.74)]
 mesh=bpy.data.meshes.new('Cut Honey Glass');mesh.from_pydata(verts,[],[(0,1,2,3)]);mesh.update();o=bpy.data.objects.new('Glass Pane '+str(i),mesh);scene.collection.objects.link(o);keep(o,glass);panes.append(o)
cone('Pitched Copper Cap',2.86,.43,.08,.3,patina);cone('Roof Gilt Lip',2.72,.43,.43,.035,brass)
cone('Roof Stem',3.06,.08,.04,.15,iron);ball('Finial',(0,0,3.18),(.07,.07,.1),brass)
fire=ball('Visible Candle Flame',(0,0,2.35),(.07,.07,.18),flame)
for i in range(3):
 a=i*2.1;curve('Old Crack',[(math.cos(a)*.315,math.sin(a)*.315,2.2),(math.cos(a+.04)*.315,math.sin(a+.04)*.315,2.45),(math.cos(a-.04)*.3,math.sin(a-.04)*.3,2.66)],soot,.008)
root=bpy.data.objects.new('Antique Lantern',None);scene.collection.objects.link(root)
for o in pieces:o.parent=root
camera_data=bpy.data.cameras.new('Lantern Portrait');camera=bpy.data.objects.new('Lantern Camera',camera_data);scene.collection.objects.link(camera);scene.camera=camera
camera.location=(3,-8,4.6);camera.rotation_euler=(Vector((0,0,1.6))-camera.location).to_track_quat('-Z','Y').to_euler();camera_data.type='ORTHO';camera_data.ortho_scale=3.8
for loc,col,power in [((-3,-4,6),(1,.66,.35),800),((3,2,5),(.17,.42,1),1100)]:
 d=bpy.data.lights.new('Soft Portrait Light','AREA');d.energy=power;d.color=col;d.size=4;o=bpy.data.objects.new(d.name,d);scene.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector((0,0,1.5))-o.location).to_track_quat('-Z','Y').to_euler()
states=[('Intact',0),('Cracked',1),('Broken',2)]
def next_render():
 if not states:
  bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Tools','Blender','MienCho_AntiqueLanterns.blend'),copy=True)
  bpy.context.window.scene=original;open(os.path.join(OUT,'Render_Complete.txt'),'w').write('3 mesh states and 3 Cycles previews complete');return None
 name,state=states.pop(0);bpy.context.window.scene=scene
 fire.hide_render=state==2;fire.hide_set(state==2)
 for i,p in enumerate(panes):p.hide_render=state>0 and i in ((1,) if state==1 else (0,1,3,4));p.hide_set(p.hide_render)
 for o in pieces:
  if o.name.startswith('Pitched'):o.rotation_euler[1]=0 if state<2 else .23
 scene.render.filepath=os.path.join(OUT,name+'.png');bpy.ops.render.render(write_still=True)
 bpy.ops.object.select_all(action='DESELECT')
 for o in pieces:
  if not o.hide_render:o.hide_set(False);o.select_set(True)
 # Curves are converted in a temporary duplicate collection by the FBX exporter.
 bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,name+'.fbx'),use_selection=True,object_types={'MESH','OTHER'},axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
 return .2
bpy.context.window.scene=original;bpy.app.timers.register(next_render,first_interval=.5)
print('Three ornate antique lantern models and Cycles previews queued; original scene preserved.')
