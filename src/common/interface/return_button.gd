extends TextureButton

func _on_pressed() -> void:

	if pressed.get_connections().size() > 1:
		return # Has been connected to do something else

	PagesManager.pop()
