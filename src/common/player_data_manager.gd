extends Node



const LOCAL_PLAYERS_DIR: String = "user://local_players"



func new_local_player() -> PlayerData:
	var player = PlayerData.new()
	player.id = __generate_uid()
	player.current_blueprint = Assets.initial_blueprint
	player.blueprints = [player.current_blueprint] as Array[CraftBlueprint]
	return player


func get_local_players() -> Array[PlayerData]:

	if !DirAccess.dir_exists_absolute(LOCAL_PLAYERS_DIR):
		return []

	var players: Array[PlayerData] = []

	FileSystemUtils.map_files(
		LOCAL_PLAYERS_DIR,
		func(file_name):
			var player = __get_local_player(file_name)
			players.append(player)
	)

	return players


func has_local_players() -> bool:

	if !DirAccess.dir_exists_absolute(LOCAL_PLAYERS_DIR):
		return false

	return DirAccess.get_files_at(LOCAL_PLAYERS_DIR).size() > 0


func store_local_player(player_data: PlayerData) -> Result:

	var create_dir_result = __create_local_players_dir()

	if create_dir_result.error != "":
		return Result.err("Failed create local player data directory: %s" % create_dir_result.error)

	var path = LOCAL_PLAYERS_DIR.path_join("%s.json" % player_data.id)
	var store_result = JSONUtils.to_file(path, player_data.to_dictionary(), "\t")

	if store_result.error != "":
		return Result.err("Failed to store local player data: %s" % store_result.error)

	return Result.new()


func delete_local_player(player_id: String) -> void:

	var file_name: String = "%s.json" % player_id
	var file_path: String = LOCAL_PLAYERS_DIR.path_join(file_name)

	if FileAccess.file_exists(file_path):
		DirAccess.remove_absolute(file_path)



func __get_local_player(path: String) -> PlayerData:

	var result = JSONUtils.from_file(path)

	assert(result.error == "", "Failed to load player data at '%s'" % result.error)

	return PlayerData.new(result.value)


func __generate_uid() -> String:

	var random = RandomNumberGenerator.new()

	random.randomize()

	return "%s-%s-%s" % [
		Time.get_unix_time_from_system() * 1000,
		Time.get_ticks_msec(),
		random.randi()
	]


func __create_local_players_dir() -> Result:

	if DirAccess.dir_exists_absolute(LOCAL_PLAYERS_DIR):
		return Result.new()

	var error: int = DirAccess.make_dir_recursive_absolute(LOCAL_PLAYERS_DIR)

	if error != OK:
		return Result.err(error_string(error))

	return Result.new()
