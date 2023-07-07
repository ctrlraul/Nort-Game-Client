class_name GenericPopup
extends CanvasLayer



@onready var panel_container: PanelContainer = %PanelContainer
@onready var title_label: Label = %TitleLabel
@onready var message_container: PanelContainer = %MessageContainer
@onready var message_label: RichTextLabel = %MessageLabel
@onready var buttons_container: HBoxContainer = %ButtonsContainer
@onready var animation_player: AnimationPlayer = $AnimationPlayer



var cancellable: bool = false

var title: String = "" :
	set(value):
		if !is_inside_tree():
			await ready
		title_label.text = value
		title_label.visible = value != ""
		title = value

var message: String = "" :
	set(value):
		message_label.text = value
		message_container.visible = value != ""
		message = value



func _ready() -> void:
	visible = false
	title_label.visible = false
	buttons_container.visible = false
	message_container.visible = false
	animation_player.play("appear")
	NodeUtils.clear(buttons_container)



func remove() -> void:
	animation_player.play("remove")


func add_button(text: String, callable: Callable = remove) -> void:

	buttons_container.visible = true

	if buttons_container.get_child_count() > 0:
		buttons_container.add_spacer(false)

	var button = Button.new()

	buttons_container.add_child(button)

	button.theme_type_variation = "PanelButton"
	button.text = text
	button.pressed.connect(callable)
	button.custom_minimum_size.x = 100


func set_ruby() -> void:
	panel_container.theme_type_variation = "PanelContainerRuby"


func set_amber() -> void:
	panel_container.theme_type_variation = "PanelContainerAmber"


func _on_outside_pressed() -> void:
	if cancellable:
		remove()
