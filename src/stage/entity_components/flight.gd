class_name FlightComponent extends EntityComponent



const DAMP = 0.95



var direction: Vector2 = Vector2.ZERO
var velocity: Vector2 = Vector2.ZERO

var __torque: float = 1
var __acceleration: float = 1



func _ready() -> void:
	super()
	craft.set_component(FlightComponent, self)


func _physics_process(delta: float) -> void:

	delta *= Engine.physics_ticks_per_second # Normalize delta

	if direction != Vector2.ZERO:

		__rotate_towards_angle(direction.angle() + PI * 0.5, __torque * 0.08)

		velocity += direction.normalized() * __acceleration * __torque * delta
		direction = Vector2.ZERO

	craft.position += velocity * delta
	velocity *= DAMP



func __rotate_towards_angle(angle: float, amount: float) -> void:

	var delta_angle = func(from: float, to) -> float:
		var difference = fmod(to - from, TAU)
		return fmod(2 * difference, TAU) - difference

	var distance = abs(delta_angle.call(craft.body.rotation, angle));

	if amount > distance:
		craft.body.rotation = angle
		return

	craft.body.rotation = lerp_angle(craft.body.rotation, angle, amount / distance)
