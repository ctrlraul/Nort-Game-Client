class_name Craft extends Node2D

signal destroyed()
signal stats_changed(craft: Craft)



@export var FlightComponent: Script
@export var PlayerControlsComponent: PackedScene
@export var TractorComponent: PackedScene
@export var TractorTargetComponent: PackedScene
@export var DroneAIComponent: PackedScene
@export var StatsDisplayComponent: PackedScene



@onready var body: CraftBody = %Body

@onready var behavior_component_sets = {
	CraftSetup.Behavior.PLAYER: [
		FlightComponent,
		TractorComponent,
		PlayerControlsComponent
	],
	CraftSetup.Behavior.FIGHTER: [
		FlightComponent,
#		FighterAIComponent,
		StatsDisplayComponent,
	],
	CraftSetup.Behavior.DRONE: [
		FlightComponent,
		TractorTargetComponent,
		DroneAIComponent,
		StatsDisplayComponent,
	],
	CraftSetup.Behavior.TURRET: [
		TractorTargetComponent,
		StatsDisplayComponent,
	],
	CraftSetup.Behavior.CARRIER: [
#		CarrierAIComponent
		StatsDisplayComponent,
	],
	CraftSetup.Behavior.OUTPOST: [
#		OutpostAIComponent
		StatsDisplayComponent,
	]
}



var __components: Dictionary = {}
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

	stats_changed.emit(self)


func set_faction(value: Faction) -> void:
	faction = value
	body.color = faction.color


func set_behavior(behavior: CraftSetup.Behavior) -> void:
	for Component in behavior_component_sets[behavior]:
		if Component is PackedScene:
			add_child(Component.instantiate())
		else:
			add_child(Component.new())


func get_component(type):
	return __components.get(type, null)


func set_component(type, instance: Node) -> void:
	if __components.has(type):
		push_error("Only one instance of each component allowed")
	else:
		__components[type] = instance


func take_hit(source: CraftPart, part: CraftPart) -> void:

	if source.gimmick is BulletGimmick:

		hull -= source.gimmick.DAMAGE

		if hull < 0:

			if part == body.core:

				core += hull

				if core <= 0:
					__destroy()

			else:
				part.take_damage(hull * -1)

			hull = 0

		stats_changed.emit(self)



func __destroy() -> void:

	hull = 0
	core = 0

	for part in body.get_parts():
		part.destroy()

	queue_free()
	destroyed.emit()

	stats_changed.emit(self)



func _on_body_part_destroyed(_part: CraftPart) -> void:
	pass
