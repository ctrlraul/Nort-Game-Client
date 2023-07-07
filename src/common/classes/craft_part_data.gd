class_name CraftPartData



var definition: CraftPartDefinition
var level: int
var shiny: bool
var gimmick: Gimmick



func _init(source = null) -> void:

	if source is CraftPartDefinition:
		definition = source
		level = 1
		shiny = false
		gimmick = null
		return

	if source is Dictionary:
		definition = Assets.get_part(source.definition)
		level = source.level
		shiny = source.get("shiny", false)
		gimmick = Assets.get_gimmick(source.gimmick) if source.has("gimmick") else null
		return

	assert(source == null, "Invalid source")



func to_dictionary() -> Dictionary:

	var dict = {
		"definition": definition.id,
		"level": level,
#		"shiny": ,
#		"gimmick": ,
	}

	if shiny:
		dict.shiny = true

	if gimmick:
		dict.gimmick = gimmick.id

	return dict
