extends Node2D

signal player_spawned(craft: Craft)
signal player_destroyed()



@export var CraftScene: PackedScene
@export var DroppedPartScene: PackedScene

@onready var entities_container: Node2D = %EntitiesContainer



var player: Craft = null
var logger: Logger = Logger.new("Stage")



func load_mission(mission: Mission) -> void:

	logger.info("Loading mission: %s" % mission.display_name)

	for craft_setup in mission.crafts:
		spawn_craft(craft_setup)


func spawn_craft(craft_setup: CraftSetup) -> void:

	var craft = CraftScene.instantiate()

	entities_container.add_child(craft)

	craft.position = craft_setup.place
	craft.set_blueprint(craft_setup.blueprint)
	craft.set_faction(craft_setup.faction)
	craft.set_behavior(craft_setup.behavior)


func spawn_player() -> Craft:

	player = CraftScene.instantiate()

	var blueprint: CraftBlueprint = (
		Game.current_player.current_blueprint
		if Game.current_player
		else Assets.initial_blueprint
	)

	entities_container.add_child(player)

	player.set_blueprint(blueprint)
	player.set_faction(Assets.player_faction)
	player.set_behavior(CraftSetup.Behavior.PLAYER)

	player.destroyed.connect(_on_player_destroyed)

	player_spawned.emit(player)

	return player


func spawn_dropped_part(setup: DroppedPartSetup) -> DroppedPart:

	var dropped_part: DroppedPart = DroppedPartScene.instantiate()

	entities_container.add_child(dropped_part)

	dropped_part.setup(setup)

	return dropped_part


func clear() -> void:
	NodeUtils.clear(entities_container)



func _on_player_destroyed() -> void:
	player = null
	player_destroyed.emit()
