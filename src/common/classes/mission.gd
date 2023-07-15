class_name Mission



var id: String
var display_name: String
var entities: Array[EntitySetup]



func _init(source = null) -> void:

	if source is Dictionary:

		id = source.id
		display_name = source.display_name
		entities = []

		for entity_setup in source.entities:
			entities.append(EntitySetup.parse(entity_setup))

		return

	assert(source == null, "Invalid source")



func to_dictionary() -> Dictionary:
	return {
		"id": id,
		"display_name": display_name,
		"entities": entities.map(func(setup): return setup.to_dictionary())
	}
