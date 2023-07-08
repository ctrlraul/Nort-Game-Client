class_name Mission



var id: String
var display_name: String
var crafts: Array[CraftSetup]



func _init(source) -> void:

	id = source.id
	display_name = source.display_name
	crafts = []

	for craft_setup in source.crafts:
		crafts.append(CraftSetup.new(craft_setup))
