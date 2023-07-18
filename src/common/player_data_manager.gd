extends Node

signal local_player_deleted(player_data: Player)



const LOCAL_PLAYERS_DIR: String = "user://local_players"



func new_local_player() -> Player:

	var player = Player.new()
	var blueprint = Assets.initial_blueprint

	player.id = Assets.generate_uid()
	player.current_blueprint = blueprint
	player.blueprints.append(blueprint)
	player.cores.append(blueprint.core.data)

	for part in blueprint.parts:
		player.parts.append(part.data)

	return player


func get_local_players() -> Array[Player]:

	if !DirAccess.dir_exists_absolute(LOCAL_PLAYERS_DIR):
		return []

	var players: Array[Player] = []

	FileSystemUtils.map_files(
		LOCAL_PLAYERS_DIR,
		func(path):

			var result = __get_local_player(path)

			if result.error != "":
				PopupsManager.error("Failed to load player data from '%s':\n\t%s" % [path, result.error]).width = 800
			else:
				players.append(result.value)
	)

	return players


func has_local_players() -> bool:

	if !DirAccess.dir_exists_absolute(LOCAL_PLAYERS_DIR):
		return false

	return DirAccess.get_files_at(LOCAL_PLAYERS_DIR).size() > 0


func store_local_player(player_data: Player) -> Result:

	var create_dir_result = __create_local_players_dir()

	if create_dir_result.error != "":
		return Result.err("Failed create local player data directory: %s" % create_dir_result.error)

	var path = LOCAL_PLAYERS_DIR.path_join("%s.json" % player_data.id)
	var store_result = JSONUtils.to_file(path, player_data.to_dictionary(), "\t")

	if store_result.error != "":
		return Result.err("Failed to store local player data: %s" % store_result.error)

	return Result.new()


func delete_local_player(player_data: Player) -> Result:

	var file_name: String = "%s.json" % player_data.id
	var file_path: String = LOCAL_PLAYERS_DIR.path_join(file_name)
	var remove_error = DirAccess.remove_absolute(file_path)

	if remove_error != OK:
		return Result.err("Failed to delete local player: %s" % error_string(remove_error))

	local_player_deleted.emit(player_data)

	return Result.new()


func add_part(player_data: Player, part_data: PartData) -> void:
	player_data.parts.append(part_data)



func __get_local_player(path: String) -> Result:

	var result = JSONUtils.from_file(path)

	if result.error != "":
		return result

	return Result.val(Player.new(result.value))


func __create_local_players_dir() -> Result:

	if DirAccess.dir_exists_absolute(LOCAL_PLAYERS_DIR):
		return Result.new()

	var error: int = DirAccess.make_dir_recursive_absolute(LOCAL_PLAYERS_DIR)

	if error != OK:
		return Result.err(error_string(error))

	return Result.new()
