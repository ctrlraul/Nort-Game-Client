extends HBoxContainer



@onready var __label: Label = %Label
@onready var __line_edit: LineEdit = %LineEdit



var __source: Control = null
var __key: String = ""



func bind_property(source: Control, key: String) -> void:
	__source = source
	__key = key
	__label.text = key.capitalize()
	__line_edit.text = __source.get(key)



func _on_line_edit_text_changed(new_text: String) -> void:
	if __source != null:
		__source.set(__key, new_text)
