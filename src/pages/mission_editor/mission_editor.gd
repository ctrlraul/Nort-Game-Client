class_name MissionEditor extends Page



const GRID_SNAP = Vector2.ONE * 16
const ZOOM_STEP = 0.1
const ZOOM_MIN = 0.1
const ZOOM_MAX = 1



@onready var container: Control = %Container



var panning: bool = false



func _mount(_data) -> void:

	await Game.initialize()



func zoom(delta: int) -> void:

	var change = container.scale.x + delta * ZOOM_STEP * container.scale.x
	var new_zoom = Vector2.ONE * clamp(change, ZOOM_MIN, ZOOM_MAX)

	if new_zoom != container.scale:

		var local_mouse = container.get_local_mouse_position()

		container.scale = new_zoom
		container.position -= local_mouse * container.scale * ZOOM_STEP * delta


func select(event: InputEventMouseButton) -> void:

	if !event.pressed:
		panning = false
		return

	panning = true



func _on_sandbox_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:

		match event.button_index:
			MOUSE_BUTTON_LEFT: select(event)
			MOUSE_BUTTON_WHEEL_UP: zoom(1)
			MOUSE_BUTTON_WHEEL_DOWN: zoom(-1)

	elif event is InputEventMouseMotion:
		if panning:
			container.position += event.relative
