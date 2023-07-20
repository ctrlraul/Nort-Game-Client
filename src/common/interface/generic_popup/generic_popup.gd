class_name GenericPopup extends CanvasLayer

signal removed()



@onready var window: PanelContainer = %Window
@onready var animation_player: AnimationPlayer = $AnimationPlayer




var canceled: bool = false
var cancelable: bool = false :
	set(value):
		set_process_unhandled_input(value)
		cancelable = value

var width: int :
	set(value):
		window.custom_minimum_size.x = value
		width = value



func _ready() -> void:
	visible = false
	animation_player.play("appear")


func _unhandled_input(_event: InputEvent) -> void:
	if Input.is_action_just_pressed("escape"):
		cancel()


func _notification(what: int) -> void:
	if what == NOTIFICATION_WM_GO_BACK_REQUEST:
		cancel()



func remove() -> void:
	animation_player.play("remove")
	removed.emit()


func cancel() -> void:
	if cancelable:
		canceled = true
		remove()


func set_ruby() -> void:
	window.theme_type_variation = "PanelContainerRuby"


func set_amber() -> void:
	window.theme_type_variation = "PanelContainerAmber"



func _on_outside_pressed() -> void:
	cancel()


func _on_animation_player_animation_finished(anim_name: StringName) -> void:
	if anim_name == "remove":
		queue_free()
