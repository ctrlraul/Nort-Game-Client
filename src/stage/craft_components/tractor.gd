class_name CraftComponentTractor extends CraftComponent



@onready var line: Line2D = %Line2D



var distance: float = 200
var torque: float = 10

var target = null :
	set(value):
		visible = value != null
		set_physics_process(visible)
		target = value



func _ready() -> void:
	super()
	visible = false
	set_physics_process(false)
	craft.set_component(CraftComponentTractor, self)


func _physics_process(delta: float) -> void:

	delta *= Engine.physics_ticks_per_second # Normalize delta

	var target_distance = craft.position.distance_to(target.craft.position)
	var force = abs(distance - target_distance) / distance * torque
	var force_delta = 1 if target_distance < distance else -1
	var direction = craft.position.direction_to(target.craft.position)

	target.craft.position += direction * force * force_delta * delta

	line.points[1] = target.craft.position - craft.position
	line.width = min(1, 0.5 + distance / target_distance * 0.5) * torque * 3
