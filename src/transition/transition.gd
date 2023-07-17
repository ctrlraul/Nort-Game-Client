extends CanvasLayer



@onready var animation_player: AnimationPlayer = %AnimationPlayer



var is_covered: bool = false



func callback(callable: Callable) -> void:
	cover()
	await animation_player.animation_finished
	await callable.call()
	uncover()


func cover() -> void:
	animation_player.play("cover")
	is_covered = true


func uncover() -> void:
	animation_player.play("uncover")
	is_covered = false


func cover_instantly() -> void:
	animation_player.play("cover_instantly")
	is_covered = true


func uncover_instantly() -> void:
	animation_player.play("uncover_instantly")
	is_covered = false
