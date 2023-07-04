class_name CraftBlueprintPart



var data: CraftPartData
var place: Vector2
var flipped: bool
var angle: float



func _init(source = null) -> void:

	if source is CraftPartData:
		data = source
		place = Vector2.ZERO
		return

	if source is CraftPartDefinition:
		data = CraftPartData.new(source)
		place = Vector2.ZERO
		return

	if source is Dictionary:
		data = CraftPartData.new(source.data)
		place = Vector2(source.x, source.y)
		flipped = DictUtils.get_or_default(source, "flip", false)
		angle = deg_to_rad(DictUtils.get_or_default(source, "angle", 0))
		return

	assert(source == null, "Invalid source")



func to_dictionary() -> Dictionary:

	var dict = {
		"data": data.to_dictionary(),
		"x": place.x,
		"y": place.y,
#		"flip": flipped,
#		"angle": round(rad_to_deg(angle))
	}

	if flipped:
		dict.flip = flipped

	if angle != 0:
		dict.angle = round(rad_to_deg(angle))

	return dict
