extends PanelContainer



@onready var object_label: Label = %ObjectLabel
@onready var fields_list: VBoxContainer = %FieldsList



func _ready() -> void:
	NodeUtils.clear(fields_list)



func set_object(object: EditorObject) -> void:

	object_label.text = object.name

	for field_key in object.explorer_fields:
