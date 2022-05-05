import bpy
import bmesh
import colorsys
from math import sin, sqrt, pi, tau

# Number of cubes.
count = 16

# Size of grid.
extents = 8.0

# For interpolating from one side
# of the grid to the other.
diff_ext = extents * 2

# Spacing between cubes.
padding = 0.0005

# The height of each cube will be animated,
# so we specify the minimum and maximum scale.
sz_base_cube = (1.75 * (extents / count)) - padding
sz_min = sz_base_cube * 0.25
sz_max = sz_base_cube * extents * 0.5
sz_diff = sz_max - sz_min

# To convert abstract grid position within loop to real-world coordinate.
count_to_prc = 1.0 / (count - 1)

# Center of grid.
x_center = 0.0
y_center = 0.0
z_center = 0.0

# Distances of cube from center.
# The maximum possible distance is used to normalize the distance.
max_dist = sqrt(2 * extents * extents)
inv_max_dist = 1.0 / max_dist

# For color.
gamma = 2.2

# Create an empty list of count x count cubes.
count_sq = count * count
mesh_objs = [None] * count_sq
lin_range_2 = range(0, count_sq, 1)

for n in lin_range_2:
    i = n // count
    j = n % count

    # Convert from row to world coordinates.
    i_prc = i * count_to_prc
    y = -extents + i_prc * diff_ext

    # Convert from column to world coordinates.
    j_prc = j * count_to_prc
    x = -extents + j_prc * diff_ext

    # Calculate rise and run.
    rise = y - y_center
    run = x - x_center

    # Find normalized distance using Pythogorean theorem.
    dst_sq = rise * rise + run * run
    dst = sqrt(dst_sq)
    norm_dist = dst * inv_max_dist

    # Remap the normalized distance to a range -PI .. PI.
    offset = -tau * norm_dist + pi

    bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=sz_base_cube)

    # Translate the mesh's scaling point from its center
    # to its bottom-center.
    bmesh.ops.translate(
        bm,
        verts=bm.verts,
        vec=(0.0, 0.0, sz_base_cube * 0.5))
    meshdata = bpy.data.meshes.new(
        "Mesh ({0:0>2d}, {1:0>2d})".format(j, i))
    bm.to_mesh(meshdata)
    bm.free()

    # Create mesh object, set location.
    mesh_obj = bpy.data.objects.new(meshdata.name, meshdata)
    mesh_obj.location = (x, y, 0.0)
    bpy.context.collection.objects.link(mesh_obj)

    # Set custom properties.
    mesh_obj["row"] = i
    mesh_obj["column"] = j
    mesh_obj["offset"] = offset

    # Append mesh object to the list.
    mesh_objs[n] = mesh_obj

    # Create a material and add it to the current object.
    mat = bpy.data.materials.new(
        name="Material ({0:0>2d}, {1:0>2d})".format(j, i))
    rgb = colorsys.hls_to_rgb(norm_dist * 0.333333, 0.525, 1.0)
    rgba = (rgb[0] ** gamma, rgb[1] ** gamma, rgb[2] * gamma, 1.0)
    mat.diffuse_color = rgba
    meshdata.materials.append(mat)

frame_rate = 30
key_frames = 15
kf_to_prc = 1.0 / (key_frames - 1)

scene = bpy.context.scene
frame_start = scene.frame_start
frame_end = scene.frame_end
scene.render.fps = frame_rate

lin_range_3 = range(0, count_sq * key_frames, 1)
for m in lin_range_3:
    h = m // count_sq
    n = m - h * count_sq

    mesh_obj = mesh_objs[n]
    offset = mesh_obj["offset"]

    # Convert from percent to frame.
    h_prc = h * kf_to_prc
    curr_frame = int((1.0 - h_prc) * frame_start
                     + h_prc * frame_end)
    scene.frame_set(curr_frame)

    # Change the scale.
    # sin returns a value in the range [0.0, 1.0].
    # The values are remapped to the desired scale with min + percent * (max - min).
    angle = tau * h_prc
    total_angle = sin(offset + angle)
    angle_to_fac = total_angle * 0.5 + 0.5
    mesh_obj.scale[2] = sz_min + angle_to_fac * sz_diff

    # Insert the key frame for the scale property.
    mesh_obj.keyframe_insert(data_path="scale", index=2)

for mesh_obj in mesh_objs:
    anim_data = mesh_obj.animation_data
    action = anim_data.action
    fcurves = action.fcurves

    for fcurve in fcurves:
        fcurve.extrapolation = 'LINEAR'

        key_frames = fcurve.keyframe_points
        for key_frame in key_frames:
            key_frame.interpolation = 'BEZIER'