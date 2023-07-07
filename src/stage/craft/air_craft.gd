class_name AirCraft extends Craft



const DAMP = 0.95



var __torque: float
var __acceleration: float

var __velocity: Vector2 = Vector2.ZERO



func _physics_process(delta: float) -> void:
	delta *= Engine.physics_ticks_per_second # Normalize delta
	__tick_movement(delta)


func _flight_direction() -> Vector2:
	assert(false, "Override this method")
	return Vector2.ZERO


# Overrides
func set_blueprint(blueprint: CraftBlueprint) -> void:

	super(blueprint)

	# TODO: Derivate from blueprint stuff
	__torque = 1
	__acceleration = 1


func __tick_movement(delta: float) -> void:

	var direction = _flight_direction()

	if direction != Vector2.ZERO:
		rotate_towards_angle(direction.angle() + PI * 0.5, __torque * 0.1)
		__velocity += direction * __acceleration * __torque * delta

	position += __velocity * delta

	__velocity *= DAMP
