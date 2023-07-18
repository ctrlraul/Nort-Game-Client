class_name PartsInventory extends MarginContainer

signal part_picked(part_data: PartData)
signal part_stored()
signal part_hovered(part_data: PartData)



@export var PartBuilderPopupScene: PackedScene
@export var PartsInventoryItemScene: PackedScene

@onready var __parts_container: Control = %PartsContainer
@onready var __empty_text_label: Label = %EmptyTextLabel
@onready var __add_part_button: Button = %AddPartButton



var color: Color :
	set(value):
		for item in __parts_container.get_children():
			item.color = value
		color = value



func _ready() -> void:
	clear()
	__add_part_button.visible = Game.dev


func set_parts(parts: Array[PartData]) -> void:

	__empty_text_label.visible = parts.size() == 0

	for part in parts:
		add_part_data(part)


func add_part_data(part_data: PartData) -> void:

	var item = PartsInventoryItemScene.instantiate()

	__parts_container.add_child(item)

	item.set_part(part_data)
	item.color = color
	item.picked.connect(_on_item_picked.bind(item))
	item.mouse_entered.connect(_on_item_mouse_entered.bind(item))


func set_blueprint(blueprint: CraftBlueprint) -> void:
	for part_blueprint in blueprint.parts:
		for item in __parts_container.get_children():
			if item.part_data.same_kind(part_blueprint.data):
				item.count -= 1


func clear() -> void:
	NodeUtils.clear(__parts_container)



func _on_item_picked(item: PartsInventoryItem) -> void:
	item.count -= 1
	part_picked.emit(item.part_data)


func _on_item_mouse_entered(item: Control) -> void:
	part_hovered.emit(item.part_data)


func _on_drag_receiver_got_data(_source, part) -> void:

	var part_data: PartData

	if part is CraftBlueprintPart:
		part_data = part.data
	elif part is PartData:
		part_data = part

	assert(part_data != null)

	var stored = false

	for item in __parts_container.get_children():
		if item.part_data.same_kind(part_data):
			item.count += 1
			stored = true

	if !stored && Game.dev:
		add_part_data(part_data)

	part_stored.emit()


func _on_add_part_button_pressed() -> void:
	var popup: PartBuilderPopup = PopupsManager.custom(PartBuilderPopupScene)
	popup.part_built.connect(_on_part_built)


func _on_part_built(part_data: PartData) -> void:
	add_part_data(part_data)
