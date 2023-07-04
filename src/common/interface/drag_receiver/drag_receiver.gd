class_name DragReceiver extends Control

signal got_data(source: Control, data)
signal drag_enter()
signal drag_leave()
signal drag_over()



@onready var color_rect: ColorRect = %ColorRect



var __mouse_over: bool = false



func _ready() -> void:
	visible = false
	set_process_input(false)
	DragEmitter.drag_start.connect(_on_drag_start)
	DragEmitter.drag_stop.connect(_on_drag_stop)

	if !GameConfig.DEBUG:
		modulate.a = 0



func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		drag_over.emit()



func _on_drag_start(_source, _data) -> void:
	visible = true
	# When a Control node is made visible and the mouse cursor is already on top
	# of it it doesn't get a mouse_enter signal until the cursor is moved, this
	# is a hack to solve it.
	var input: InputEventMouseMotion = InputEventMouseMotion.new()
	input.position = get_global_mouse_position()
	get_viewport().push_input(input)


func _on_drag_stop(source, data) -> void:
	visible = false
	if __mouse_over:
		__mouse_over = false
#		normal.visible = true
#		mouse_over.visible = false
		set_process_input(false)
		got_data.emit(source, data)


func _on_mouse_entered() -> void:
#	normal.visible = false
#	mouse_over.visible = true
	__mouse_over = true
	drag_enter.emit()
	set_process_input(true)


func _on_mouse_exited() -> void:
#	normal.visible = true
#	mouse_over.visible = false
	__mouse_over = false
	drag_leave.emit()
	set_process_input(false)
