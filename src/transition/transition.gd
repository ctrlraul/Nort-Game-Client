extends CanvasLayer



@onready var animation_player: AnimationPlayer = %AnimationPlayer



func callback(callable: Callable) -> void:
	cover()
	await animation_player.animation_finished
	await callable.call()
	uncover()


func cover() -> void:
	animation_player.play("cover")


func uncover() -> void:
	animation_player.play("uncover")


func cover_instantly() -> void:
	animation_player.play("cover_instantly")


func uncover_instantly() -> void:
	animation_player.play("uncover_instantly")
