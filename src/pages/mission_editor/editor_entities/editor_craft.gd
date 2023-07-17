class_name EditorCraft extends EditorEntity



var blueprint: CraftBlueprint : set = set_blueprint
var faction: Faction : set = set_faction
var behavior: String : set = set_behavior



@onready var __craft_display: CraftDisplay = %CraftDisplay



func _ready() -> void:
	super()
	set_process_input(false)


func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		position += event.relative / get_global_transform().get_scale()


func _init_explorer_fields() -> Array[ExplorerField]:

	var get_behaviors = func():
		return CraftSetup.Behavior.keys()

	return [
		ExplorerOptionsField.new(self, "blueprint", Assets.get_blueprints, "id"),
		ExplorerOptionsField.new(self, "faction", Assets.get_factions, "display_name"),
		ExplorerOptionsField.new(self, "behavior", get_behaviors, "")
	]



func get_entity_setup() -> EntitySetup:

	var setup = CraftSetup.new()

	setup.place = position
	setup.faction = faction
	setup.blueprint = blueprint
	setup.behavior = CraftSetup.Behavior[behavior]

	return setup


func set_setup(setup: CraftSetup) -> void:
	position = setup.place
	blueprint = setup.blueprint
	faction = setup.faction
	behavior = CraftSetup.Behavior.find_key(setup.behavior)


func set_blueprint(value: CraftBlueprint) -> void:
	__craft_display.set_blueprint(value)
	blueprint = value


func set_faction(value: Faction) -> void:
	__craft_display.color = value.color
	faction = value


func set_behavior(value: String) -> void:
	behavior = value
