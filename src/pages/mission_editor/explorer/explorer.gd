class_name Explorer extends PanelContainer

const ExplorerOptionsFieldItemScene = preload("res://pages/mission_editor/explorer/field_scenes/explorer_options_field_item.tscn")



@onready var object_label: Label = %ObjectLabel
@onready var fields_list: VBoxContainer = %FieldsList



func _ready() -> void:
	clear()



func set_object(object: EditorObject) -> void:

	clear()

	visible = true
	object_label.text = object.name

	for field in object.explorer_fields:

		var item = null

		if field is ExplorerOptionsField:
			item = ExplorerOptionsFieldItemScene.instantiate()

		assert(item != null)

		fields_list.add_child(item)
		item.set_field(field)



func clear() -> void:
	visible = false
	object_label.text = ""
	NodeUtils.clear(fields_list)
