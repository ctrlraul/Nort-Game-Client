class_name CraftDisplay
extends Control



@export var PartScene: PackedScene

@onready var __parts_container: Control = %PartsContainer
@onready var core: CraftDisplayPart = %Core



var color: Color = GameConfig.FACTIONLESS_COLOR :
	set(value):
		core.color = value
		for part in parts:
			part.color = value
		color = value

var parts: Array :
	get:
		return __parts_container.get_children()



func _ready() -> void:
	NodeUtils.clear(__parts_container)



func set_blueprint(blueprint: CraftBlueprint) -> void:
	for part_blueprint in blueprint.parts:
		add_part(part_blueprint)
	set_core_blueprint(blueprint.core)


func set_core_blueprint(blueprint: CraftBlueprintPart) -> CraftDisplayPart:
	core.set_blueprint(blueprint)
	return core


func add_part(part_blueprint: CraftBlueprintPart) -> CraftDisplayPart:

	var part = PartScene.instantiate()

	__parts_container.add_child(part)

	part.set_blueprint(part_blueprint)
	part.color = color

	return part


func clear() -> void:
	NodeUtils.clear(__parts_container)


func to_blueprint() -> CraftBlueprint:

	var blueprint = CraftBlueprint.new()

	blueprint.id = Assets.generate_uid()
	blueprint.core = core.to_blueprint()

	for part in parts:

		if part == core:
			continue

		blueprint.parts.append(part.to_blueprint())

	return blueprint
