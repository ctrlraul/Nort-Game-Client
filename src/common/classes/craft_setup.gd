class_name CraftSetup



enum Behavior {
	PLAYER,
	FIGHTER,
	DRONE,
	TURRET,
	CARRIER,
	OUTPOST
}



var place: Vector2
var blueprint: CraftBlueprint
var faction: Faction
var behavior: Behavior



func _init(source) -> void:

	if source is Dictionary:
		place = Vector2(source.x, source.y)
		blueprint = Assets.get_blueprint(source.blueprint)
		faction = Assets.get_faction(source.faction)
		behavior = Behavior[source.behavior]
		return

	assert(source == null, "Invalid source: '%s'" % source)



func to_dictionary() -> Dictionary:
	return {
		"x": round(place.x),
		"y": round(place.y),
		"blueprint": blueprint.id,
		"faction": faction.id,
		"behavior": Behavior.find_key(behavior)
	}
