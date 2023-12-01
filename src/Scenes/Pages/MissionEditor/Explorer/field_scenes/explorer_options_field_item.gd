extends HBoxContainer



@onready var __label: Label = %Label
@onready var __option_button: OptionButton = %OptionButton

var __field: ExplorerOptionsField
var __options: Array = []



func _ready() -> void:
	__option_button.clear()



func set_field(field: ExplorerOptionsField) -> void:

	__field = field
	__options = field.get_options_method.call()

	__label.text = field.key.capitalize()

	for option in __options:
		var label = option[field.option_label_key] if field.option_label_key != "" else str(option)
		__option_button.add_item(label.capitalize())

	__option_button.selected = __options.find(field.entity[field.key])



func _on_option_button_item_selected(index: int) -> void:
	__field.entity[__field.key] = __options[index]
