class_name EditorEntity extends Control

signal pressed()
signal drag_start()
signal drag_stop()



@onready var hitbox: Button = %Hitbox
@onready var __selection_indicator: Panel = %SelectionIndicator

var explorer_fields: Array

var selected: bool = false :
	set(value):
		__selection_indicator.visible = value
		selected = value



func _ready() -> void:
	selected = false
	explorer_fields = _init_explorer_fields()


func _init_explorer_fields() -> Array[ExplorerField]:
	return []



func set_setup(_setup) -> void:
	assert(false, "Not implemented")


func get_entity_setup() -> EntitySetup:
	assert(false, "Not implemented, you should override this method")
	return EntitySetup.new()



func _on_hitbox_pressed() -> void:
	pressed.emit()


func _on_hitbox_button_down() -> void:
	drag_start.emit()


func _on_hitbox_button_up() -> void:
	drag_stop.emit()
