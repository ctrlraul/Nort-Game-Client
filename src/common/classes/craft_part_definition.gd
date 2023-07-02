class_name CraftPartDefinition



enum Type { CORE, HULL }



var id: String
var display_name: String
var texture_name: String
var type: Type



func _init(source) -> void:
	id = source.id
	display_name = source.display_name
	texture_name = source.texture
	type = Type[source.type]
