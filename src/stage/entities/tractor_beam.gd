extends Node2D



@onready var line: Line2D = %Line2D



var distance: float = 200
var torque: float = 10

var target = null :
	set(value):
		visible = value != null
		set_physics_process(visible)
		target = value



func _ready() -> void:
	visible = false
	set_physics_process(false)


func _physics_process(delta: float) -> void:

	delta *= Engine.physics_ticks_per_second # Normalize delta

	var target_distance = owner.position.distance_to(target.position)
	var force = abs(distance - target_distance) / distance * torque
	var force_delta = 1 if target_distance < distance else -1
	var direction = owner.position.direction_to(target.position)

	target.position += direction * force * force_delta * delta

	line.points[1] = target.position - owner.position
	line.width = min(1, 0.5 + distance / target_distance * 0.5) * torque * 3

#	line.points[2] = target.position - owner.position
#	line.points[1] = line.points[2] * 0.5
#	line.width_curve.set_point_value(1, min(1, distance / target_distance))
