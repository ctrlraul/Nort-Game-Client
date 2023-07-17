class_name EditorPlayerCraft extends EditorCraft



@onready var __label: Label = %Label



func _ready() -> void:
	super()
	await Game.initialize()
	__craft_display.color = Assets.player_faction.color


func _init_explorer_fields() -> Array[ExplorerField]:
	return [
		ExplorerOptionsField.new(self, "blueprint", Assets.get_blueprints, "id"),
	]



func set_setup(setup) -> void:
	assert(setup is PlayerCraftSetup, "Godot is retarded")
	blueprint = setup.test_blueprint


func set_blueprint(value: CraftBlueprint) -> void:
	__label.position.y = Assets.get_blueprint_size(value).y * 0.5 + 20
	super(value)


func get_entity_setup() -> EntitySetup:
	var setup = PlayerCraftSetup.new()
	setup.test_blueprint = blueprint
	return setup
