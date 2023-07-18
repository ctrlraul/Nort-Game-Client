class_name PartData



var definition: Part
var shiny: bool
var gimmick: Gimmick



func _init(source = null) -> void:

	if source is Part:
		definition = source
		shiny = false
		gimmick = null
		return

	if source is Dictionary:
		definition = Assets.get_part(source.definition)
		shiny = source.get("shiny", false)
		gimmick = Assets.get_gimmick(source.gimmick) if source.has("gimmick") else null
		return

	assert(source == null, "Invalid source")



func same_kind(part: PartData) -> bool:

	if definition != part.definition:
		return false

	if shiny != part.shiny:
		return false

	if gimmick != part.gimmick:
		return false

	return true



func to_dictionary() -> Dictionary:

	var dict = {
		"definition": definition.id,
#		"shiny": ,
#		"gimmick": ,
	}

	if shiny:
		dict.shiny = true

	if gimmick:
		dict.gimmick = gimmick.id

	return dict
