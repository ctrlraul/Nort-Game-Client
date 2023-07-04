extends Control



@onready var __sprite: TextureRect = %Sprite



func _ready() -> void:
	clear()
	await Game.initialize()
	__sprite.self_modulate = Assets.player_faction.color


func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		position = get_global_mouse_position()



func set_part_data(part_data: CraftPartData) -> void:
	__sprite.texture = Assets.get_part_texture(part_data.definition.id)

	position = get_global_mouse_position()
	visible = true

	set_process_input(true)


func clear() -> void:
	visible = false
	set_process_input(false)
