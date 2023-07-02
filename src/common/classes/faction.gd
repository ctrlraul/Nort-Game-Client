class_name Faction



var id: String
var display_name: String
var color: Color = Color(1, 0, 1)



func _init(source) -> void:
	id = source.id
	display_name = source.display_name
	color = Color(source.color)
