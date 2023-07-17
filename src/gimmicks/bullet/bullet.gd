class_name BulletGimmick extends Node2D



const DAMAGE = 10
const __HALF_PI = PI * 0.5
const __RANGE_RADIUS = 700



@onready var __range_area: Area2D = %RangeArea
@onready var __range_area_collision_shape: CollisionShape2D = %RangeAreaCollisionShape2D
@onready var __ray: Area2D = %Ray
@onready var __ray_collision_shape: CollisionShape2D = %RayCollisionShape2D
@onready var __cooldown_timer: Timer = %CooldownTimer
@onready var __particles: GPUParticles2D = %GPUParticles2D

var part: CraftPart

var target: CraftPart : set = __set_target



func _ready() -> void:

	assert(__cooldown_timer.wait_time > __particles.lifetime, "ayo bro this aint worky")

	__range_area_collision_shape.shape.radius = __RANGE_RADIUS
	__ray_collision_shape.shape.b.y = -__RANGE_RADIUS

	$Sprite2D.texture = Assets.get_gimmick_texture("bullet")

	set_physics_process(false)


func _physics_process(_delta: float) -> void:
	global_rotation = global_position.angle_to_point(target.global_position) + __HALF_PI



func __fire() -> void:

	if target == null || __cooldown_timer.time_left > 0:
		return

	if target.is_destroyed:
		__try_to_find_a_target()

	if target == null:
		return

	__cooldown_timer.start()
	__particles.emitting = true

	var foe_parts_in_ray = __get_foe_parts_in_area(__ray)
	var part_hit = NodeUtils.find_nearest(foe_parts_in_ray, part.global_position, true)

	if part_hit == null:
		return

	target.body.craft.take_hit(part, part_hit)


func __try_to_find_a_target() -> void:
	var foe_parts_in_range = __get_foe_parts_in_area(__range_area)
	target = foe_parts_in_range.pick_random() if foe_parts_in_range.size() > 0 else null


func __is_foe_part_area(area: Area2D) -> bool:
	return area.owner.body.craft.faction != part.body.craft.faction


func __get_foe_parts_in_area(area: Area2D) -> Array:
	return (
		area
			.get_overlapping_areas()
			.filter(__is_foe_part_area)
			.map(func(part_area): return part_area.owner)
			.filter(func(foe_part): return !foe_part.is_destroyed)
	)


func __set_target(value: CraftPart) -> void:

	if value == target:
		return

	if target != null && !target.is_destroyed:
		target.destroyed.disconnect(_on_target_destroyed)

	target = value

	if target != null:
		target.destroyed.connect(_on_target_destroyed)
		__fire()
		set_physics_process(true)
	else:
		rotation = 0
		set_physics_process(false)



func _on_cooldown_timer_timeout() -> void:
	__fire()


func _on_range_area_area_entered(area: Area2D) -> void:
	if target == null && __is_foe_part_area(area):
		target = area.owner


func _on_range_area_area_exited(area: Area2D) -> void:
	if target != null && area.owner == target:
		__try_to_find_a_target()


func _on_target_destroyed() -> void:
	__try_to_find_a_target()
