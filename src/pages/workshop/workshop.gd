extends Page



const ZOOM_STEP = 0.1
const ZOOM_MIN = 0.8
const ZOOM_MAX = 2
const PAN_MAX = 400



@export var PartsListItem: PackedScene

@onready var hitbox: Control = %Hitbox
@onready var canvas: Control = %Canvas
@onready var craft_display: CraftDisplay = %CraftDisplay
@onready var parts_inventory: PanelContainer = %PartsInventory
@onready var part_drag_preview: Control = %PartDragPreview



var dragged_part = null
var panning: bool = false



func _ready() -> void:
	Stage.clear()


func _mount() -> void:

	if !Game.initialized:
		await Game.initialize()

	if Game.current_player:
		craft_display.set_blueprint(Game.current_player.current_blueprint)
	else:
		craft_display.set_blueprint(Assets.initial_blueprint)

	var hulls: Array[CraftPartDefinition] = []
	var cores: Array[CraftPartDefinition] = []

	craft_display.set_color(Assets.player_faction.color)

	for part in Assets.get_parts():
		match part.type:
			CraftPartDefinition.Type.CORE:
				cores.append(part)
			CraftPartDefinition.Type.HULL:
				hulls.append(part)

	parts_inventory.set_parts(hulls)


func _input(event: InputEvent) -> void:

	if !dragged_part:
		return

	if event is InputEventMouseMotion:
		part_drag_preview.position = hitbox.get_local_mouse_position()
		dragged_part.position = canvas.get_local_mouse_position() * canvas.scale

	elif event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if !event.pressed:
				dragged_part = null
				part_drag_preview.visible = false



func __zoom(delta: int) -> void:

	var change = canvas.scale.x + delta * ZOOM_STEP * canvas.scale.x
	var new_zoom = Vector2.ONE * clamp(change, ZOOM_MIN, ZOOM_MAX)

	if new_zoom != canvas.scale:
		var mouse = canvas.get_local_mouse_position()
		canvas.scale = new_zoom
		canvas.position -= mouse * canvas.scale * ZOOM_STEP * delta



func _on_hitbox_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:

		match event.button_index:

			MOUSE_BUTTON_LEFT:
				panning = event.pressed

			MOUSE_BUTTON_WHEEL_UP:
				__zoom(1)

			MOUSE_BUTTON_WHEEL_DOWN:
				__zoom(-1)

	elif event is InputEventMouseMotion:
		if panning:
			canvas.position += event.relative


func _on_parts_inventory_part_picked(part_data: CraftPartData) -> void:

	var blueprint = CraftBlueprintPart.new(part_data)
	blueprint.place = canvas.get_local_mouse_position()

	dragged_part = craft_display.add_part(blueprint)
	dragged_part.visible = false

	part_drag_preview.visible = true
	part_drag_preview.part_data = part_data
	part_drag_preview.position = hitbox.get_local_mouse_position()


func _on_hitbox_drag_receiver_drag_enter() -> void:

	var blueprint = CraftBlueprintPart.new(DragEmitter.data)
	blueprint.place = canvas.get_local_mouse_position()

	# Adding children to craft display on the same frame as the mouse entered the
	# drag receiver triggered another mouse enter event, which causes a stack
	# overflow, even if craft display's visibility is set to hidden, which is
	# pretty fucking bizarre.
	(
		func():
			dragged_part = craft_display.add_part(blueprint)
	).call_deferred()



func _on_hitbox_drag_receiver_drag_leave() -> void:
	dragged_part.queue_free()
	dragged_part = null


func _on_hitbox_drag_receiver_drag_over() -> void:
	dragged_part.position = canvas.get_local_mouse_position()


func _on_hitbox_drag_receiver_got_data(_source, _data) -> void:
	dragged_part = null
