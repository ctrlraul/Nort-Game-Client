extends Node2D



@onready var animation_player: AnimationPlayer = %AnimationPlayer



func _on_area_2d_body_entered(_body: Node2D) -> void:
	animation_player.play("show")


func _on_area_2d_body_exited(_body: Node2D) -> void:
	animation_player.play("hide")


func _on_animation_player_animation_finished(anim_name: StringName) -> void:
	if anim_name == "show":
		animation_player.play("rotate")
