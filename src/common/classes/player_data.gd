class_name PlayerData



const DEFAULT_NICK: String = "Newborn"



var id: String
var nick: String = DEFAULT_NICK
var blueprints: Array[CraftBlueprint]
var current_blueprint: CraftBlueprint
var score: int = 0



func _init(source = null) -> void:

	if source is Dictionary:

		id = source.id
		nick = source.get("nick", DEFAULT_NICK)
		blueprints = []

		for source_blueprint in source.blueprints:
			blueprints.append(CraftBlueprint.new(source_blueprint))

		current_blueprint = blueprints[source.current_blueprint]
		score = source.get("score", 0)

		return

	assert(source == null, "Invalid source")



func to_dictionary() -> Dictionary:
	return {
		"id": id,
		"nick": nick,
		"blueprints": blueprints.map(func(b): return b.to_dictionary()),
		"current_blueprint": blueprints.find(current_blueprint)
	}
