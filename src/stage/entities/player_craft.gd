class_name PlayerCraft extends AirCraft



@onready var cursor: CharacterBody2D = %Cursor



func _ready() -> void:
	set_faction(Assets.player_faction)
	if Game.current_player:
		set_blueprint(Game.current_player.current_blueprint)
	else:
		set_blueprint(Assets.initial_blueprint)


func _process(_delta: float) -> void:
	cursor.global_position = get_global_mouse_position()



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
