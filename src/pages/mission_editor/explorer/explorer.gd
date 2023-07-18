class_name Explorer extends PanelContainer



const ExplorerOptionsFieldItemScene = preload("res://pages/mission_editor/explorer/field_scenes/explorer_options_field_item.tscn")



@onready var entity_label: Label = %EntityLabel
@onready var fields_list: VBoxContainer = %FieldsList



func _ready() -> void:
	clear()



func set_entity(entity: EditorEntity) -> void:

	clear()

	visible = true
	entity_label.text = EntitySetup.Type.find_key(entity.get_entity_setup().type).capitalize()

	for field in entity.explorer_fields:

		var item = null

		if field is ExplorerOptionsField:
			item = ExplorerOptionsFieldItemScene.instantiate()

		assert(item != null)

		fields_list.add_child(item)
		item.set_field(field)



func clear() -> void:
	visible = false
	entity_label.text = ""
	NodeUtils.clear(fields_list)
