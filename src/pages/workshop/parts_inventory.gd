extends PanelContainer

signal part_picked(part_data: CraftPartData)



@export var PartsInventoryItemScene: PackedScene

@onready var parts_container: Control = %PartsContainer



func _ready() -> void:
	NodeUtils.clear(parts_container)



func set_parts(parts: Array[CraftPartDefinition]) -> void:
	for part in parts:
		var item = PartsInventoryItemScene.instantiate()
		parts_container.add_child(item)
		item.set_part(part)
		item.color = Assets.player_faction.color
		item.picked.connect(_on_item_picked.bind(item))


func _on_item_picked(item: Control) -> void:
	part_picked.emit(item.part_data)
