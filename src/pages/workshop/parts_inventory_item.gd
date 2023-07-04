extends Button

signal picked()



@export var PartDragPreviewScene: PackedScene

@onready var __sprite: TextureRect = %Sprite
@onready var __outline_sprite: TextureRect = %OutlineSprite
@onready var __count_label: Label = %Count



var part_data: CraftPartData

var color: Color :
	set(value):
		__sprite.self_modulate = value
		color = value



func set_part(part: CraftPartDefinition) -> void:

	part_data = CraftPartData.new(part)

	__sprite.texture = Assets.get_part_texture(part.id)
	__outline_sprite.texture = __sprite.texture
	__count_label.text = ""



func _on_mouse_entered() -> void:
	__outline_sprite.visible = true


func _on_mouse_exited() -> void:
	__outline_sprite.visible = false


func _on_button_down() -> void:
	DragEmitter.drag(self, part_data)
	picked.emit()
