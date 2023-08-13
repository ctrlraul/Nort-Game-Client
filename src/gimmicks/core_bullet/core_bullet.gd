class_name CoreBulletGimmick extends Node2D



const DAMAGE = 10
const __RANGE_RADIUS = 700



@onready var __range_area: Area2D = %RangeArea
@onready var __range_area_collision_shape: CollisionShape2D = %RangeAreaCollisionShape2D
@onready var __cooldown_timer: Timer = %CooldownTimer

var part: CraftPart

var target: CraftPart : set = __set_target



func _ready() -> void:
	__range_area_collision_shape.shape.radius = __RANGE_RADIUS



func __fire() -> void:

	if target == null || __cooldown_timer.time_left > 0:
		return

	if target.is_destroyed:
		__try_to_find_a_target()

	if target == null:
		return

	__cooldown_timer.start()

	print("firr")


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
