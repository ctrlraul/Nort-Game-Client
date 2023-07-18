class_name Part



enum Type { CORE, HULL }



var id: String
var display_name: String
var texture_name: String
var type: Type
var core: int
var hull: int
var mass: int



func _init(source) -> void:
	id = source.id
	display_name = source.display_name
	texture_name = source.texture
	type = Type[source.type]
	core = source.core
	hull = source.hull
	mass = source.mass
