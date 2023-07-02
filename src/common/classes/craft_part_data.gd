class_name CraftPartData



var definition: CraftPartDefinition
var level: int
var shiny: bool



func _init(source = null) -> void:

	if source is CraftPartDefinition:
		definition = source
		level = 1
		shiny = false

	if source is Dictionary:
		definition = Assets.get_part(source.definition)
		level = source.level
		shiny = source.shiny

	assert(source != null, "Invalid source")



func to_dictionary() -> Dictionary:
	return {
		"definition": definition.id,
		"level": level,
		"shiny": shiny
	}
