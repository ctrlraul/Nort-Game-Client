class_name CraftBlueprint



var id: String
var core: CraftBlueprintPart
var parts: Array[CraftBlueprintPart] = []



func _init(source = null) -> void:

	if source == null:
		core = CraftBlueprintPart.new(Assets.initial_core)

	elif source is Dictionary:
		id = source.id
		core = CraftBlueprintPart.new(source.core)

		for source_part in source.parts:
			parts.append(CraftBlueprintPart.new(source_part))

		return

	assert(source == null, "Invalid source")



func to_dictionary() -> Dictionary:
	return {
		"id": id,
		"core": core.to_dictionary(),
		"parts": parts.map(func(p): return p.to_dictionary())
	}
