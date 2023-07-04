extends Button

signal picked()



@export var PartDragPreviewScene: PackedScene

@onready var __frame: TextureRect = %Frame
@onready var __craft_part_display: Control = %CraftPartDisplay
@onready var __count_label: Label = %Count



var part_data: CraftPartData

var color: Color :
	set(value):
		__craft_part_display.color = value
		color = value



func set_part(value: CraftPartData) -> void:
	part_data = value
	__craft_part_display.definition = part_data.definition
	__craft_part_display.shiny = part_data.shiny
	__frame.self_modulate = Color.WHITE if part_data.shiny else Color.BLACK
	__count_label.text = ""



func _on_mouse_entered() -> void:
	__craft_part_display.outline = true


func _on_mouse_exited() -> void:
	__craft_part_display.outline = false


func _on_button_down() -> void:
	DragEmitter.drag(self, part_data)
	picked.emit()
