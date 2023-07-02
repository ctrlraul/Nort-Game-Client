class_name GenericPopup
extends CanvasLayer


var cancellable: bool = false



func _ready() -> void:
		$AnimationPlayer.play("appear")



func remove() -> void:
	$AnimationPlayer.play("remove")



func _on_outside_pressed() -> void:
	if cancellable:
		remove()
