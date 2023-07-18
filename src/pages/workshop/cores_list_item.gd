extends Button



@export var ShinyPartMaterial: ShaderMaterial

@onready var part_display: PartDisplay = %PartDisplay



var color: Color :
	set(value):
		part_display.color = value
		color = value

var part_data: PartData :
	set(value):
		part_display.set_part_data(value)
		part_data = value



func _on_mouse_entered() -> void:
	part_display.outline = true


func _on_mouse_exited() -> void:
	part_display.outline = false
