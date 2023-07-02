extends Control


@onready var header: Control = %Header


# Called when the node enters the scene tree for the first time.
func _ready() -> void:

	visible = false
	header.modulate.a = 1

	var atween = create_tween()
	atween.tween_interval(1)

	await atween.finished

	visible = true

	var tween = create_tween()
	tween.set_trans(Tween.TRANS_QUAD)
	tween.set_ease(Tween.EASE_OUT)
	tween.set_parallel(true)
	tween.tween_property(%Sizer, "size:y", %Sizer.size.y, 0.3)
	tween.tween_property(header, "size:y", header.size.y, 0.3)
	%Sizer.size.y = 0


func _on_close_button_pressed() -> void:
	queue_free()
