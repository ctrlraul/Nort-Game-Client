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


#func set_entities(entities: Array[EditorEntity]) -> void:
#
##	clear()
#
#	var entry_separator = "---"
#
#	visible = true
#	var mutual_fields: Dictionary = {}
#
#	for entity in entities:
#		for field in entity.explorer_fields:
#
#			var entry = "%s%s%s" % [field.get_script().resource_path, entry_separator, field.key]
#
#			if mutual_fields.has(entry):
#				mutual_fields[entry] += 1
#			else:
#				mutual_fields[entry] = 1
#
#
#
#	var entities_count = entities.size()
#
#	for entry in mutual_fields:
#
#		if mutual_fields[entry] < entities_count:
#			continue
#
#		var parts: Array = entry.split(entry_separator)
#		var field_script_path = parts[0]
#		var field_key = parts[1]
#
#		var script = load(field_script_path).new()
#		var item = null
#
#		print("%s: %s" % [field_script_path, script is ExplorerOptionsField])
#
#		if field is ExplorerOptionsField:
#			item = ExplorerOptionsFieldItemScene.instantiate()
#
#		assert(item != null)
#
#		fields_list.add_child(item)
#		item.set_field(field)


func clear() -> void:
	visible = false
	entity_label.text = ""
	NodeUtils.clear(fields_list)
