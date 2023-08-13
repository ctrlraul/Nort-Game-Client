class_name EntitySetup



enum Type {
	CRAFT,
	PLAYER_CRAFT,
	ORPHAN_PART
}



var type: Type
var place: Vector2



func _init(source = null) -> void:

	if source is Dictionary:
		type = Type[source.type]
		place = Vector2(source.x, source.y)
		return

	assert(source == null, "Invalid source")



func to_dictionary() -> Dictionary:
	return {
		"type": Type.find_key(type),
		"x": round(place.x),
		"y": round(place.y)
	}



static func parse(source: Dictionary) -> EntitySetup:

	var setup: EntitySetup

	match Type[source.type]:
		Type.CRAFT: setup = CraftSetup.new(source)
		Type.PLAYER_CRAFT: setup = PlayerCraftSetup.new(source)
		Type.ORPHAN_PART: setup = OrphanPartSetup.new(source)

	assert(setup != null, "Invalid setup")

	return setup