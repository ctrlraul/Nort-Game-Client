extends CanvasLayer

signal unpause()
signal forfeit()
signal quit()



@onready var tree = get_tree()



func _ready() -> void:
	tree.paused = true


func _input(_event: InputEvent) -> void:
	if Input.is_action_just_pressed("escape"):
		__unpause()



func __unpause() -> void:
	tree.paused = false
	queue_free()
	unpause.emit()



func _on_tree_exiting() -> void:
	tree.paused = false


func _on_continue_button_pressed() -> void:
	__unpause()


func _on_forfeit_button_pressed() -> void:
	forfeit.emit()


func _on_quit_button_pressed() -> void:
	quit.emit()
