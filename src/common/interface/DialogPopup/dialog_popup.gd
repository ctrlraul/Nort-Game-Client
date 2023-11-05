class_name DialogPopup extends GenericPopup



@onready var title_label: Label = %TitleLabel
@onready var message_container: PanelContainer = %MessageContainer
@onready var message_label: RichTextLabel = %MessageLabel
@onready var buttons_container: HBoxContainer = %ButtonsContainer



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
	title_label.visible = false
	buttons_container.visible = false
	message_container.visible = false
	NodeUtils.clear(buttons_container)
	super()


func add_button(text: String, on_pressed: Callable = remove) -> void:

	buttons_container.visible = true

	if buttons_container.get_child_count() > 0:
		buttons_container.add_spacer(false)

	var button = Button.new()

	if on_pressed != remove:
		on_pressed = func():
			on_pressed.call()
			remove()

	buttons_container.add_child(button)

	button.text = text
	button.pressed.connect(on_pressed)
	button.custom_minimum_size.x = 100
