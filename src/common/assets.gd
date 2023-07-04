extends Node



const MISSING_PART_IMAGE = preload("res://assets/images/part_example.png")

const __PARTS_DIR = "craft_parts"
const __BLUEPRINTS_DIR = "craft_blueprints"
const __FACTIONS_DIR = "factions"
const __PART_SPRITES_DIR = "craft_part_sprites"



var __logger = Logger.new("Assets")

var parts: Dictionary = {}
var cores: Dictionary = {}
var __blueprints: Dictionary = {}
var __factions: Dictionary = {}
var __part_textures: Dictionary = {}

var initial_blueprint: CraftBlueprint :
	get: return __blueprints[GameConfig.INITIAL_BLUEPRINT]

var player_faction: Faction :
	get: return __factions[GameConfig.PLAYER_FACTION]



func import_assets(base_path: String) -> void:

	FileSystemUtils.map_files(base_path.path_join(__PARTS_DIR), __import_part)
	FileSystemUtils.map_files(base_path.path_join(__BLUEPRINTS_DIR), __import_blueprint)
	FileSystemUtils.map_files(base_path.path_join(__FACTIONS_DIR), __import_faction)

	assert(GameConfig.INITIAL_BLUEPRINT in __blueprints, "Initial blueprint '%s' not found" % GameConfig.INITIAL_BLUEPRINT)
	assert(GameConfig.PLAYER_FACTION in __factions, "Player faction '%s' not found" % GameConfig.PLAYER_FACTION)

	__import_part_textures(base_path)

	__logger.info("Items: %s" % (parts.size() + cores.size()))
	__logger.info("Blueprints: %s" % __blueprints.size())
	__logger.info("Factions: %s" % __factions.size())



func get_part_texture(reference) -> Texture2D:

	var id: String

	if reference is String:
		id = reference
	elif reference is CraftPartDefinition:
		id = reference.id
	elif reference is CraftPartData:
		id = reference.definition.id
	elif reference is CraftBlueprintPart:
		id = reference.data.definition.id
	else:
		__logger.error("Texture for '%s' is not found" % id)

	return __part_textures.get(id, MISSING_PART_IMAGE)


func get_part(id: String) -> CraftPartDefinition:
	return parts[id] if id in parts else cores[id]


func get_blueprint(id: String) -> CraftBlueprint:
	return __blueprints[id]


func generate_uid() -> String:

	var random = RandomNumberGenerator.new()

	random.randomize()

	return "%s-%s-%s" % [
		Time.get_unix_time_from_system() * 1000,
		Time.get_ticks_msec(),
		random.randi()
	]


func is_core(object) -> bool:

	if object is CraftPartDefinition:
		return object.type == CraftPartDefinition.Type.CORE
	if object is CraftPartData:
		return object.definition.type == CraftPartDefinition.Type.CORE
	if object is CraftBlueprintPart:
		return object.data.definition.type == CraftPartDefinition.Type.CORE

	assert(false, "Not implemented")

	return false


func __import_part(path: String) -> void:

	var result = JSONUtils.from_file(path)

	if result.error != "":
		push_error("Failed to import craft part '%s': %s" % [path, result.error])
	else:
		var part = CraftPartDefinition.new(result.value)
		match part.type:
			CraftPartDefinition.Type.CORE: cores[part.id] = part
			CraftPartDefinition.Type.HULL: parts[part.id] = part


func __import_blueprint(path: String) -> void:

	var result = JSONUtils.from_file(path)

	if result.error != "":
		push_error("Failed to import craft blueprint '%s': %s" % [path, result.error])
	else:
		var blueprint = CraftBlueprint.new(result.value)
		__blueprints[blueprint.id] = blueprint


func __import_faction(path: String) -> void:

	var result = JSONUtils.from_file(path)

	if result.error != "":
		push_error("Failed to import faction '%s': %s" % [path, result.error])
	else:
		var faction = Faction.new(result.value)
		__factions[faction.id] = faction


func __import_part_textures(base_path: String) -> void:

	assert(parts.size() + cores.size() > 0, "No parts imported")

	var dir_path = base_path.path_join(__PART_SPRITES_DIR)

	for part in parts.values() + cores.values():

		var texture_path = dir_path.path_join(part.texture_name)
		var texture = load(texture_path)

		if texture:
			__part_textures[part.id] = __with_inset_margin(texture, 1)
		else:
			__part_textures[part.id] = MISSING_PART_IMAGE
			__logger.error("Failed to import texture for part '%s': %s" % [part.id, texture_path])


func __with_inset_margin(texture: Texture2D, margin: int) -> Texture2D:

	var image: Image = texture.get_image()
	var size = texture.get_size()
	var new_image = Image.create(size.x, size.y, false, image.get_format())

	image.resize(
		size.x - margin * 2,
		size.y - margin * 2,
		Image.INTERPOLATE_BILINEAR
	)

	new_image.blit_rect(
		image,
		Rect2i(Vector2.ZERO, image.get_size()),
		Vector2.ONE * margin
	)

	return ImageTexture.create_from_image(new_image)
