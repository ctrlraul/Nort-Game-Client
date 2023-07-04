extends Panel

signal part_picked(part_data: CraftPartData)
signal part_stored()



@export var PartsInventoryItemScene: PackedScene

@onready var parts_container: Control = %PartsContainer



var items: Dictionary



func _ready() -> void:
	items.clear()
	NodeUtils.clear(parts_container)



func set_parts(parts: Array[CraftPartData]) -> void:
	for part in parts:
		var item = PartsInventoryItemScene.instantiate()

		parts_container.add_child(item)

		item.set_part(part)
		item.color = Assets.player_faction.color
		item.picked.connect(_on_item_picked.bind(item))

		items[part.definition.id] = item



func _on_item_picked(item: Control) -> void:
	part_picked.emit(item.part_data)


func _on_drag_receiver_got_data(_source, data) -> void:

	if data is CraftPartDefinition:
		if data.id in items:
			items[data.id].count += 1

	part_stored.emit()
