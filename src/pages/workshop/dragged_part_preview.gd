extends Control



@onready var craft_part_display: Control = %PartDisplay



func _ready() -> void:
	set_process_input(false)


func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		position = event.global_position



func set_part_data(part_data: PartData) -> void:
	visible = true
	position = get_global_mouse_position()
	set_process_input(true)
	craft_part_display.set_part_data(part_data)


func set_color(color: Color) -> void:
	craft_part_display.color = color


func clear() -> void:
	visible = false
	set_process_input(false)
