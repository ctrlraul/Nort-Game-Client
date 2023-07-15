class_name DroneGimmick extends Node2D



@onready var __cooldown_timer: Timer = %CooldownTimer
@onready var __particles: GPUParticles2D = %GPUParticles2D

var part: CraftPart



func _ready() -> void:
	assert(__cooldown_timer.wait_time > __particles.lifetime, "ayo bro this aint worky")
	set_physics_process(false)



func __fire() -> void:

	if __cooldown_timer.time_left > 0:
		return

	__cooldown_timer.start()
	__particles.emitting = true



func _on_cooldown_timer_timeout() -> void:
	__fire()
