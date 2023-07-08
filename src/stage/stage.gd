extends Node2D



@export var CraftScene: PackedScene
@export var PlayerCraftScene: PackedScene
@export var TurretCraftScene: PackedScene

@onready var entities_container: Node2D = %EntitiesContainer
@onready var camera: Camera2D = $Camera2D



var logger = Logger.new("Stage")



func load_mission(mission: Mission) -> void:

	logger.info("Loading mission: %s" % mission.display_name)

	for craft_setup in mission.crafts:

		var craft = CraftScene.instantiate()

		entities_container.add_child(craft)

		craft.position = craft_setup.place
		craft.set_blueprint(craft_setup.blueprint)
		craft.set_faction(craft_setup.faction)



func spawn_player() -> PlayerCraft:
	var craft = PlayerCraftScene.instantiate()
	entities_container.add_child(craft)
	return craft


func spawn_turret() -> TurretCraft:
	var craft = TurretCraftScene.instantiate()
	entities_container.add_child(craft)
	return craft



func clear() -> void:
	camera.position = Vector2.ZERO
	camera.zoom = Vector2.ONE * 0.5
	NodeUtils.clear(entities_container)
