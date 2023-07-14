class_name EditorCraft extends EditorObject



var blueprint: CraftBlueprint : set = set_blueprint
var faction: Faction : set = set_faction
var behavior: String : set = set_behavior



func _ready() -> void:

	var get_behaviors = func(): return CraftSetup.Behavior.keys()

	explorer_fields.append_array([
		ExplorerOptionsField.new(self, "blueprint", Assets.get_blueprints, "id"),
		ExplorerOptionsField.new(self, "faction", Assets.get_factions, "display_name"),
		ExplorerOptionsField.new(self, "behavior", get_behaviors, "")
	])



func set_blueprint(blueprint: CraftBlueprint) -> void:
	print(blueprint.id)


func set_faction(faction: Faction) -> void:
	print(faction.id)


func set_behavior(behavior: String) -> void:
	print(CraftSetup.Behavior[behavior])
