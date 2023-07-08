extends Node



const DialogPopupScene: PackedScene = preload("res://common/interface/dialog_popup/dialog_popup.tscn")
const PartBuilderPopupScene: PackedScene = preload("res://common/interface/part_builder_popup/part_builder_popup.tscn")



func error(message: String, title = "Error") -> DialogPopup:

	var popup = DialogPopupScene.instantiate()

	add_child(popup)

	popup.title = title
	popup.message = message
	popup.cancellable = true

	popup.set_ruby()
	popup.add_button("Ok")

	return popup


func warn(message: String, title = "Warning") -> DialogPopup:

	var popup = DialogPopupScene.instantiate()

	add_child(popup)

	popup.title = title
	popup.message = message

	popup.set_amber()

	return popup


func part_builder() -> PartBuilderPopup:
	var popup = PartBuilderPopupScene.instantiate()
	add_child(popup)
	return popup
