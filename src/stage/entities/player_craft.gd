class_name PlayerCraft extends AirCraft



@onready var __tractor: Node2D = %TractorBeam
@onready var __cursor_area: Area2D = %CursorArea

var __hovered_interactable = null



func _ready() -> void:

	super()

	set_process_unhandled_input(false)

	set_faction(Assets.player_faction)

	if Game.current_player:
		set_blueprint(Game.current_player.current_blueprint)
	else:
		set_blueprint(Assets.initial_blueprint)


func _process(_delta: float) -> void:
	__cursor_area.global_position = get_global_mouse_position()


func _unhandled_input(_event: InputEvent) -> void:
	if Input.is_action_just_pressed("select"):
		__interact()


# Overrides
func _flight_direction() -> Vector2:
	return Vector2.ZERO if crippled else __get_keyboard_motion_input()



func __get_keyboard_motion_input() -> Vector2:

	var direction = Vector2()

	if Input.is_action_pressed("move_left"):
		direction.x -= 1
	if Input.is_action_pressed("move_right"):
		direction.x += 1
	if Input.is_action_pressed("move_up"):
		direction.y -= 1
	if Input.is_action_pressed("move_down"):
		direction.y += 1

	return direction.normalized()


func __update_hovered_interactable() -> void:

	var areas = __cursor_area.get_overlapping_areas()

	if __hovered_interactable != null:
		__hovered_interactable.in_range = false

	match areas.size():
		0: __hovered_interactable = null
		1: __hovered_interactable = areas[0].owner
		_: __hovered_interactable = NodeUtils.find_nearest(areas, __cursor_area.position).owner

	if __hovered_interactable is TractorTarget:
		__hovered_interactable.in_range = true

	set_process_unhandled_input(__hovered_interactable != null)


func __interact() -> void:
	if __hovered_interactable is TractorTarget:
		__tractor.target = __hovered_interactable.owner



func _on_cursor_area_area_entered(_area: Area2D) -> void:
	__update_hovered_interactable()


func _on_cursor_area_area_exited(_area: Area2D) -> void:
	__update_hovered_interactable()
