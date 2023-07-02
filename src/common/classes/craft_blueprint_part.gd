class_name CraftBlueprintPart



var data: CraftPartData
var place: Vector2



func _init(source = null) -> void:

	if source is CraftPartData:
		data = source
		place = Vector2.ZERO

	elif source is CraftPartDefinition:
		data = CraftPartData.new(source)
		place = Vector2.ZERO

	elif source is Dictionary:
		data = CraftPartData.new(source.data)
		place = Vector2(source.x, source.y)

	assert(source != null, "Invalid source")



func to_dictionary() -> Dictionary:
	return {
		"data": data.to_dictionary(),
		"x": place.x,
		"y": place.y
	}
