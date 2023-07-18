class_name PartsInventoryItem extends Button

signal picked()



@onready var __frame: TextureRect = %Frame
@onready var __craft_part_display: Control = %PartDisplay
@onready var __count_label: Label = %Count



var part_data: PartData
var count: int = 1 : set = __set_count

var color: Color :
	set(value):
		__craft_part_display.color = value
		__frame.self_modulate = value
		color = value



func set_part(source) -> void:

	part_data = null

	if source is Part:
		count = int(INF)
		part_data = PartData.new(source)

	if source is PartData:
		count = int(INF)
		part_data = source

	if source is PartInInventory:
		count = source.count
		part_data = source

	assert(part_data != null, "Invalid source")

	__frame.material = Assets.MATERIAL_SHINY if part_data.shiny else null
	__craft_part_display.set_part_data(part_data)



func __set_count(value: int) -> void:

	if value == 0:
		modulate.a = 0.5
		__count_label.text = ""
	else:
		modulate.a = 1
		__count_label.text = "x%s" % value if value > 1 && value < 10000 else ""

	count = value



func _on_mouse_entered() -> void:
	__craft_part_display.outline = true


func _on_mouse_exited() -> void:
	__craft_part_display.outline = false


func _on_button_down() -> void:
	if count > 0 || Game.dev:
		DragEmitter.drag(self, part_data)
		picked.emit()
