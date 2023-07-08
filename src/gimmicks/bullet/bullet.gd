extends Node2D



const HALF_PI = PI * 0.5



@onready var __area: Area2D = %Area2D
@onready var __gfx: Node2D = %GFX
@onready var __cooldown_timer: Timer = %CooldownTimer
@onready var __particles: GPUParticles2D = %GPUParticles2D

var target: CraftBody
var part: CraftPart



func _ready() -> void:
	assert(__cooldown_timer.wait_time > __particles.lifetime, "ayo bro this aint worky")


func _process(_delta: float) -> void:
	if target:
		__gfx.global_rotation = global_position.angle_to_point(target.global_position) + HALF_PI
	else:
		__gfx.global_rotation = 0


func _notification(what: int) -> void:
	if what == NOTIFICATION_WM_CLOSE_REQUEST:
		__area.area_exited.disconnect(_on_area_2d_area_exited)



func __update_target() -> void:

	var areas = __get_foe_areas_in_range()

	target = null

	match areas.size():
		0: pass
		1: target = areas[0].owner
		_: target = NodeUtils.find_nearest(areas, position).owner

	if __can_fire():
		__fire()


func __get_foe_areas_in_range() -> Array:
	return __area.get_overlapping_areas().filter(
		func(area: Area2D):
			return area.owner.craft.faction != part.craft_body.craft.faction
	)


func __can_fire() -> bool:
	return target != null && __cooldown_timer.time_left == 0


func __fire() -> void:
	__cooldown_timer.start()
	__particles.emitting = true



func _on_area_2d_area_entered(area: Area2D) -> void:
	var craft_body: CraftBody = area.owner
	if craft_body.craft.faction != part.craft_body.craft.faction:
		__update_target()


func _on_area_2d_area_exited(area: Area2D) -> void:
	var craft_body: CraftBody = area.owner
	if craft_body.craft.faction != part.craft_body.craft.faction:
		__update_target()


func _on_cooldown_timer_timeout() -> void:
	if __can_fire():
		__fire()
