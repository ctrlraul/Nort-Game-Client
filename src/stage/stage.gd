extends Node2D

signal player_spawned(craft: Craft)
signal player_destroyed()



@export var CraftScene: PackedScene
@export var DroppedPartScene: PackedScene

@onready var entities_container: Node2D = %EntitiesContainer
@onready var camera: Camera2D = %Camera2D



var player: Craft = null
var player_flight_comp: CraftComponentFlight = null
var logger: Logger = Logger.new("Stage")



func _ready() -> void:
	clear()


func _process(_delta: float) -> void:

	var vel = max(player_flight_comp.velocity.length(), 0.001)
	camera.zoom = lerp(camera.zoom, Vector2.ONE * clamp(1 / vel * 0.005, 0.4, 0.5), 0.01)
	camera.position = lerp(camera.position, player.position + player_flight_comp.velocity * 120, 0.01)



func load_mission(mission: Mission) -> void:

	logger.info("Loading mission: %s" % mission.display_name)

	for entity_setup in mission.entities:
		spawn_entity(entity_setup)


func spawn_entity(setup: EntitySetup) -> Node2D:

	if setup is CraftSetup:
		return spawn_craft(setup)

	if setup is PlayerCraftSetup:
		return spawn_player_craft(setup)

	if setup is OrphanPartSetup:
		return spawn_dropped_part(setup)

	assert(false, "Not implemented")

	return null


func spawn_craft(setup: CraftSetup) -> Craft:

	var craft = CraftScene.instantiate()

	entities_container.add_child(craft)

	craft.position = setup.place
	craft.set_blueprint(setup.blueprint)
	craft.set_faction(setup.faction)
	craft.set_behavior(setup.behavior)

	return craft


func spawn_player_craft(setup: PlayerCraftSetup = PlayerCraftSetup.new()) -> Craft:

	player = CraftScene.instantiate()

	entities_container.add_child(player)

	player.position = setup.place
	player.set_blueprint(Game.current_player.current_blueprint if Game.current_player else setup.test_blueprint)
	player.set_faction(Assets.player_faction)
	player.set_behavior(CraftSetup.Behavior.PLAYER)

	player.destroyed.connect(_on_player_destroyed)

	player_flight_comp = player.get_component(CraftComponentFlight)

	camera.position = player.position
	camera.zoom = Vector2.ONE * 0.5

	set_process(true)

	player_spawned.emit(player)

	return player


func spawn_dropped_part(setup: OrphanPartSetup) -> DroppedPart:

	var dropped_part: DroppedPart = DroppedPartScene.instantiate()

	entities_container.add_child(dropped_part)

	dropped_part.set_setup(setup)

	return dropped_part


func clear() -> void:
	NodeUtils.clear(entities_container)
	set_process(false)
	camera.zoom = Vector2.ONE * 0.5



func _on_player_destroyed() -> void:
	player = null
	player_destroyed.emit()
	player_flight_comp = null
	set_process(false)
