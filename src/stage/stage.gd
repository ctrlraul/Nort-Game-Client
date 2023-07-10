extends Node2D



@export var CraftScene: PackedScene

@onready var entities_container: Node2D = %EntitiesContainer



var logger = Logger.new("Stage")



func load_mission(mission: Mission) -> void:

	logger.info("Loading mission: %s" % mission.display_name)

	for craft_setup in mission.crafts:

		var craft = CraftScene.instantiate()

		entities_container.add_child(craft)

		craft.position = craft_setup.place
		craft.set_blueprint(craft_setup.blueprint)
		craft.set_faction(craft_setup.faction)
		craft.set_behavior(craft_setup.behavior)


func spawn_player() -> Craft:

	var craft = CraftScene.instantiate()

	entities_container.add_child(craft)

	craft.set_blueprint(
		Game.current_player.current_blueprint
		if Game.current_player
		else Assets.initial_blueprint
	)

	craft.set_faction(Assets.player_faction)
	craft.set_behavior(CraftSetup.Behavior.PLAYER)

	return craft


#func spawn_turret() -> TurretCraft:
#	var craft = TurretCraftScene.instantiate()
#	entities_container.add_child(craft)
#	return craft


func clear() -> void:
	NodeUtils.clear(entities_container)
