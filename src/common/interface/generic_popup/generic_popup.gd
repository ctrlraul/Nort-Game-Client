class_name GenericPopup extends CanvasLayer

signal removed()



@onready var window: PanelContainer = %Window
@onready var animation_player: AnimationPlayer = $AnimationPlayer



var cancellable: bool = false

var width: int :
	set(value):
		window.custom_minimum_size.x = value
		width = value



func _ready() -> void:
	visible = false
	animation_player.play("appear")



func remove() -> void:
	animation_player.play("remove")
	removed.emit()


func set_ruby() -> void:
	window.theme_type_variation = "PanelContainerRuby"


func set_amber() -> void:
	window.theme_type_variation = "PanelContainerAmber"



func _on_outside_pressed() -> void:
	if cancellable:
		remove()


func _on_animation_player_animation_finished(anim_name: StringName) -> void:
	if anim_name == "remove":
		queue_free()
