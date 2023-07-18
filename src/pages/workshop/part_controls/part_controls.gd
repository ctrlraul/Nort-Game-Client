extends Control

signal flip(flipped: bool)
signal rotated(angle: int)



const ANGLES = 16



@onready var outline_sprite_container: Control = %OutlineSpriteContainer
@onready var outline_sprite: TextureRect = %OutlineSprite
@onready var buttons_margin: Control = %ButtonsMargin
@onready var rotate_icon_centerer: Control = %RotateIconCenterer
@onready var rotate_icon_container: Control = %RotateIconContainer
@onready var line: Line2D = %Line2D
@onready var flip_icon: TextureRect = %FlipIcon


var part: CraftDisplayPart = null : set = __set_part
var __rotation_start_angle: float = 0
var __last_mouse_wheel_spin: float = 0

var flipped: bool :
	set(value):
		flipped = value
		flip_icon.flip_h = value
		outline_sprite.flip_h = value

var angle: float :
	set(value):
		angle = value
		rotate_icon_container.rotation = value
		outline_sprite_container.rotation = value



func _ready() -> void:
	set_process_input(false)
	line.visible = false


func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:

		var mouse = get_local_mouse_position()
		var mouse_angle = mouse.angle()
		var new_rotation = rad_to_deg(mouse_angle - __rotation_start_angle)
		var snap: float = 360.0 / ANGLES

		angle = deg_to_rad(floor(new_rotation / snap) * snap)

		if part:
			part.angle = angle

		line.rotation = mouse_angle
		line.points[1].x = mouse.length()

		rotated.emit(angle)



func __set_part(value: CraftDisplayPart) -> void:

	if value != null:

		var texture = Assets.get_part_texture(value.part_data.definition.id)
		var texture_size = texture.get_size()

		outline_sprite.texture = texture
		outline_sprite.size = texture_size
		outline_sprite.pivot_offset = texture_size * 0.5
		outline_sprite.position = -texture_size * 0.5

		angle = value.angle
		flipped = value.flipped

		visible = true

	else:
		outline_sprite.texture = null
		visible = false

	part = value


func clear() -> void:
	part = null
	visible = false



func update_transform(control: Control) -> void:

	if part == null:
		return

	position = control.position + part.position * control.scale
	outline_sprite_container.scale = control.scale
	buttons_margin.position.y = outline_sprite.texture.get_height() * 0.5 * control.scale.y + 20



func _on_rotate_button_button_down() -> void:

	var mouse = get_local_mouse_position()
	var mouse_angle = mouse.angle()

	__rotation_start_angle = mouse_angle - angle

	line.visible = true
	line.rotation = mouse_angle
	line.points[1].x = mouse.length()

	set_process_input(true)


func _on_rotate_button_button_up() -> void:
	set_process_input(false)
	line.visible = false


func _on_flip_button_pressed() -> void:
	flipped = !flipped
	part.flipped = flipped
	flip.emit(flipped)


func _on_rotate_button_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:

		if (event.button_index != MOUSE_BUTTON_WHEEL_UP &&
				event.button_index != MOUSE_BUTTON_WHEEL_DOWN):
			return

		# Time stuff to mitigate some mouses sending multiple scroll events at once

		var now = Time.get_ticks_msec()

		if now - __last_mouse_wheel_spin < 10:
			return

		var delta = 1 if event.button_index == MOUSE_BUTTON_WHEEL_UP else -1

		angle += deg_to_rad(360.0 / ANGLES * delta)

		part.angle = angle
		__last_mouse_wheel_spin = now
