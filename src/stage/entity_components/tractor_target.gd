@tool

class_name TractorTarget extends Node2D



@onready var animation_player: AnimationPlayer = %AnimationPlayer



var in_range: bool = false :
	set(value):
		if value:
			animation_player.play("show")
		else:
			animation_player.play("hide")



func _ready() -> void:
	if Engine.is_editor_hint():
		animation_player.play("rotate")


func _on_animation_player_animation_finished(anim_name: StringName) -> void:
	if anim_name == "show":
		animation_player.play("rotate")
