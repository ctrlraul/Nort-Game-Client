extends Node



const GenericPopupScene: PackedScene = preload("res://common/interface/generic_popup/generic_popup.tscn")



func error(message: String) -> GenericPopup:

	var popup = GenericPopupScene.instantiate()

	add_child(popup)

	popup.title = "Error"
	popup.message = message
	popup.cancellable = true

	popup.set_ruby()
	popup.add_button("Ok")

	return popup



func warn(message: String) -> GenericPopup:

	var popup = GenericPopupScene.instantiate()

	add_child(popup)

	popup.title = "Warning"
	popup.message = message

	popup.set_amber()

	return popup
