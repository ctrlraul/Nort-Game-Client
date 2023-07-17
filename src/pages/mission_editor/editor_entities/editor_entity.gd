class_name EditorEntity extends Control

signal selected()



var explorer_fields: Array



func _ready() -> void:
	explorer_fields = _init_explorer_fields()


func _init_explorer_fields() -> Array[ExplorerField]:
	return []



func set_setup(_setup) -> void:
	assert(false, "Not implemented")


func get_entity_setup() -> EntitySetup:
	assert(false, "Not implemented, you should override this method")
	return EntitySetup.new()



func _on_hitbox_pressed() -> void:
	selected.emit()


func _on_hitbox_button_down() -> void:
	set_process_input(true)


func _on_hitbox_button_up() -> void:
	set_process_input(false)
