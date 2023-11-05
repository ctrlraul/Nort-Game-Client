

func load_mission(mission: Mission) -> void:

	logger.info("Loading mission: %s" % mission.display_name)

	for entity_setup in mission.entities:
		spawn_entity(entity_setup)


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

	player_flight_comp = player.get_component(FlightComponent)

	camera.position = player.position
	camera.zoom = Vector2.ONE * 0.5

	set_process(true)

	player_spawned.emit(player)

	return player
