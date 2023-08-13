class_name Craft extends Entity

signal destroyed()



@export var FlightComponentScene: PackedScene
@export var PlayerControlsComponentScene: PackedScene
@export var TractorComponentScene: PackedScene
@export var TractorTargetComponentScene: PackedScene
@export var DroneAIComponentScene: PackedScene
@export var StatsDisplayComponentScene: PackedScene



@onready var body: CraftBody = %Body

@onready var behavior_component_sets = {
	CraftSetup.Behavior.PLAYER: [
		FlightComponentScene,
		TractorComponentScene,
		PlayerControlsComponentScene
	],
	CraftSetup.Behavior.NONE: [],
	CraftSetup.Behavior.FIGHTER: [
		FlightComponentScene,
#		FighterAIComponentScene,
		StatsDisplayComponentScene,
	],
	CraftSetup.Behavior.DRONE: [
		FlightComponentScene,
		TractorTargetComponentScene,
		DroneAIComponentScene,
		StatsDisplayComponentScene,
	],
	CraftSetup.Behavior.TURRET: [
		TractorTargetComponentScene,
		StatsDisplayComponentScene,
	],
	CraftSetup.Behavior.CARRIER: [
#		CarrierAIComponentScene,
		StatsDisplayComponentScene,
	],
	CraftSetup.Behavior.OUTPOST: [
#		OutpostAIComponentScene,
		StatsDisplayComponentScene,
	]
}



var faction: Faction
var crippled: bool = false
var hull: float = 30
var hull_max: float = 30
var core: float = 50
var core_max: float = 50
var blueprint: CraftBlueprint



func _ready() -> void:
	body.craft = self



func set_blueprint(value: CraftBlueprint) -> void:

	blueprint = value

	hull_max = 0
	core_max = 0

	for part_blueprint in blueprint.parts:
		hull_max += part_blueprint.data.definition.hull
		core_max += part_blueprint.data.definition.core

	hull = hull_max
	core = core_max

	body.set_blueprint(blueprint)


func set_faction(value: Faction) -> void:
	faction = value
	body.color = faction.color


func set_behavior(behavior: CraftSetup.Behavior) -> void:
	for Component in behavior_component_sets[behavior]:
		add_child(Component.instantiate())


func take_hit(source, part: CraftPart) -> void:

	if source is BulletGimmick:

		hull -= source.DAMAGE

		if hull < 0:

			if part == body.core:

				core += hull

				if core <= 0:
					__destroy()

			else:
				part.take_damage(hull * -1)

			hull = 0



func __destroy() -> void:

	hull = 0
	core = 0

	for part in body.get_parts():
		part.destroy()

	queue_free()
	destroyed.emit()



func _on_body_part_destroyed(_part: CraftPart) -> void:
	pass
