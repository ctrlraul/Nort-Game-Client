extends Panel

signal part_picked(part_data: CraftPartData)
signal part_stored()
signal part_hovered(part_data: CraftPartData)



@export var PartsInventoryItemScene: PackedScene

@onready var parts_container: Control = %PartsContainer
@onready var empty_text_label: Label = %EmptyTextLabel



var items: Dictionary

var color: Color :
	set(value):
		for item in items.values():
			item.color = value
		color = value



func _ready() -> void:
	items.clear()
	NodeUtils.clear(parts_container)



func set_parts(parts: Array[CraftPartData]) -> void:

	empty_text_label.visible = parts.size() == 0

	for part in parts:
		var item = PartsInventoryItemScene.instantiate()

		parts_container.add_child(item)

		item.set_part(part)
		item.color = Assets.player_faction.color
		item.picked.connect(_on_item_picked.bind(item))
		item.mouse_entered.connect(_on_item_mouse_entered.bind(item))

		items[part.definition.id] = item



func _on_item_picked(item: Control) -> void:
	part_picked.emit(item.part_data)


func _on_item_mouse_entered(item: Control) -> void:
	part_hovered.emit(item.part_data)


func _on_drag_receiver_got_data(_source, data) -> void:

	if data is CraftPartDefinition:
		if data.id in items:
			items[data.id].count += 1

	part_stored.emit()
