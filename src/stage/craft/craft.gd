class_name Craft extends Node2D



@export var FlightComponent: Script
@export var PlayerControlsComponent: PackedScene
@export var TractorComponent: PackedScene
@export var TractorTargetComponent: PackedScene
@export var DroneAIComponent: PackedScene



@onready var behavior_component_sets = {
	CraftSetup.Behavior.PLAYER: [
		FlightComponent,
		TractorComponent,
		PlayerControlsComponent
	],
	CraftSetup.Behavior.FIGHTER: [
		FlightComponent,
#		FighterAIComponent
	],
	CraftSetup.Behavior.DRONE: [
		FlightComponent,
		TractorTargetComponent,
		DroneAIComponent
	],
	CraftSetup.Behavior.TURRET: [
		TractorTargetComponent,
	],
	CraftSetup.Behavior.CARRIER: [
#		CarrierAIComponent
	],
	CraftSetup.Behavior.OUTPOST: [
#		OutpostAIComponent
	]
}



@onready var body: CraftBody = %Body



var faction: Faction
var crippled: bool = false
var __components: Dictionary = {}



func _ready() -> void:
	body.craft = self
	body.enable()
	__post_ready.call_deferred()



func set_blueprint(blueprint: CraftBlueprint) -> void:
	body.set_blueprint(blueprint)


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



func __post_ready() -> void:
	pass
