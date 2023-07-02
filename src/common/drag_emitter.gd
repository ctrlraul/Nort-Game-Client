extends Node

signal drag_start(source, data)
signal drag_stop(source, data)


var source: Control
var data



func _ready() -> void:
	set_process_input(false)


@warning_ignore("shadowed_variable")
func drag(source, data = null) -> void:

	self.source = source
	self.data = data

	drag_start.emit(source, data)

	set_process_input(true)



func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if !event.pressed:
				set_process_input(false)
				drag_stop.emit(source, data)
				source = null
				data = null
