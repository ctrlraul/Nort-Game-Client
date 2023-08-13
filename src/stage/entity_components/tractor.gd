class_name TractorComponent extends EntityComponent



@onready var line: Line2D = %Line2D



var __tick_method: Callable = __tick_for_anything

var distance: float = 200
var torque: float = 10
var target: TractorTargetComponent = null : set = set_target



func _ready() -> void:
	super()
	__clear()
	entity.set_component(TractorComponent, self)


func _physics_process(delta: float) -> void:
	delta *= Engine.physics_ticks_per_second # Normalize delta
	__tick_method.call(delta)
	__update_gfx()



func set_target(value: TractorTargetComponent) -> void:

	if value == null:
		__clear()
		return

	if value.entity is DroppedPart:
		__tick_method = __tick_for_dropped_part
	else:
		__tick_method = __tick_for_anything

	target = value

	set_physics_process(true)



func __clear() -> void:
	visible = false
	__tick_method = __tick_for_anything
	set_physics_process(false)


func __tick_for_anything(delta: float) -> void:
	var target_distance = entity.position.distance_to(target.entity.position)
	var force = abs(distance - target_distance) / distance * torque
	var force_delta = 1 if target_distance < distance else -1
	var direction = entity.position.direction_to(target.entity.position)
	target.entity.position += direction * force * force_delta * delta


func __tick_for_dropped_part(delta: float) -> void:
	var direction = entity.position.direction_to(target.entity.position)
	target.entity.position += direction * torque * delta


func __update_gfx() -> void:
	line.points[1] = target.entity.position - entity.position
