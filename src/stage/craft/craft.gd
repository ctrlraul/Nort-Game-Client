class_name Craft extends Node2D



@onready var body: CraftBody = %Body



var faction: Faction
var crippled: bool = false



func _ready() -> void:
	body.craft = self
	body.enable()



func set_blueprint(blueprint: CraftBlueprint) -> void:
	body.set_blueprint(blueprint)


func set_faction(value: Faction) -> void:
	faction = value
	body.color = faction.color


func rotate_towards_angle(angle: float, amount: float) -> void:

	var delta_angle = func(from: float, to) -> float:
		var difference = fmod(to - from, TAU)
		return fmod(2 * difference, TAU) - difference

	var distance = abs(delta_angle.call(body.rotation, angle));

	if amount > distance:
		body.rotation = angle
		return

	body.rotation = lerp_angle(body.rotation, angle, amount / distance)
