class_name CraftComponentPlayerControls extends CraftComponent



@onready var __cursor_area: Area2D = %CursorArea

var __interactable: Area2D
var __tractor_comp: CraftComponentTractor
var __flight_comp: CraftComponentFlight



func _ready() -> void:
	super()
	craft.set_component(CraftComponentPlayerControls, self)
	set_process_unhandled_input(false)


func _process(_delta: float) -> void:
	__cursor_area.global_position = get_global_mouse_position()
	__update_interactable()


func _physics_process(_delta: float) -> void:
	__flight_comp.direction = __get_keyboard_motion_direction()


func _unhandled_input(_event: InputEvent) -> void:
	if Input.is_action_just_pressed("select"):
		__interact()


func _post_ready() -> void:
	__tractor_comp = craft.get_component(CraftComponentTractor)
	__flight_comp = craft.get_component(CraftComponentFlight)



func __get_keyboard_motion_direction() -> Vector2:

	var direction = Vector2()

	if Input.is_action_pressed("move_left"):
		direction.x -= 1
	if Input.is_action_pressed("move_right"):
		direction.x += 1
	if Input.is_action_pressed("move_up"):
		direction.y -= 1
	if Input.is_action_pressed("move_down"):
		direction.y += 1

	return direction


func __interact() -> void:

	if __interactable == null:
		Logger.error_static("Player Controls", "Interaction with '%s' is not implemented" % __interactable)

	if __interactable.owner is CraftComponentTractorTarget:
		__tractor_comp.target = __interactable.owner


func __update_interactable() -> void:

	var areas = __cursor_area.get_overlapping_areas()
	var nearest_area = NodeUtils.find_nearest(areas, __cursor_area.position)

	if nearest_area != __interactable:

		if __interactable != null && __interactable.owner is CraftComponentTractorTarget:
			__interactable.owner.in_range = false

		__interactable = nearest_area

		if nearest_area == null:
			set_process_unhandled_input(false)

		else:
			set_process_unhandled_input(true)
			if __interactable.owner is CraftComponentTractorTarget:
				__interactable.owner.in_range = true



func _on_cursor_area_area_entered(_area: Area2D) -> void:
	__update_interactable()


func _on_cursor_area_area_exited(_area: Area2D) -> void:
	__update_interactable()
