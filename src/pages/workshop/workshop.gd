extends Page



const GRID_SNAP = Vector2.ONE * 16
const ZOOM_STEP = 0.1
const ZOOM_MIN = 0.6
const ZOOM_MAX = 1.3
const PAN_MAX = 400



@export var PartsListItem: PackedScene

@onready var mouse_area: Area2D = %MouseArea
@onready var core_area: Area2D = %CoreArea
@onready var part_areas: Node2D = %PartAreas
@onready var camera: Camera2D = %Camera2D

@onready var hitbox: Control = %Hitbox
@onready var canvas: Control = %Canvas
@onready var hovered_part_outline: Control = %HoveredPartOutline
@onready var hovered_part_outline_sprite: TextureRect = %HoveredPartOutlineSprite
@onready var part_controls: Control = %PartControls
@onready var craft_display: CraftDisplay = %CraftDisplay
@onready var parts_inventory: Control = %PartsInventory
@onready var picked_part_preview: Control = %PartDragPreview



var drag_offset: Vector2 = Vector2.ZERO
var dragged_part: CraftDisplayPart = null
var hovered_part: CraftDisplayPart = null
var panning: bool = false

var area_for_part: Dictionary = {}
var part_for_area: Dictionary = {}



func _ready() -> void:
	picked_part_preview.clear()
	part_controls.clear()
	Stage.clear()
	%SubViewport.size = hitbox.size


func _mount() -> void:

	await Game.initialize()

	if Game.current_player:
		set_blueprint(Game.current_player.current_blueprint)
	else:
		set_blueprint(Assets.initial_blueprint)

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



func set_blueprint(blueprint: CraftBlueprint) -> void:
	for part in blueprint.parts:
		add_part(part)
	craft_display.set_core_blueprint(blueprint.core)


func zoom(delta: int) -> void:

	var change = canvas.scale.x + delta * ZOOM_STEP * canvas.scale.x
	var new_zoom = Vector2.ONE * clamp(change, ZOOM_MIN, ZOOM_MAX)

	if new_zoom != canvas.scale:

		var local_mouse = canvas.get_local_mouse_position()

		canvas.scale = new_zoom
		canvas.position -= local_mouse * canvas.scale * ZOOM_STEP * delta

		update_camera()

		part_controls.update_transform(canvas)


func update_camera() -> void:
	camera.zoom = canvas.scale
	camera.position = -canvas.position / canvas.scale


func add_part(blueprint: CraftBlueprintPart) -> void:

	var part = craft_display.add_part(blueprint)
	var area = Area2D.new()
	var shape = CollisionShape2D.new()

	area.position = blueprint.place
	area.collision_layer = 256
	shape.shape = RectangleShape2D.new()
	shape.shape.size = Assets.get_part_texture(blueprint.data.definition.id).get_size()

	area.add_child(shape)
	part_areas.add_child(area)

	area_for_part[part.get_instance_id()] = area
	part_for_area[area.get_instance_id()] = part

	dragged_part = part
	hovered_part_outline.visible = false


func remove_part(part: CraftDisplayPart) -> void:

	var area: Node2D = get_area_for_part(part)

	part_for_area.erase(area)
	area_for_part.erase(part)

	area.queue_free()
	part.queue_free()


func get_nearest_part() -> CraftDisplayPart:

	var relative_mouse = craft_display.get_local_mouse_position()
	var shortest_distance: float = INF
	var closest: CraftDisplayPart = null

	for part in craft_display.parts:

		if part == craft_display.core:
			continue

		if !part.is_mouse_over():
			continue

		var distance = part.position.distance_to(relative_mouse)

		if distance < shortest_distance:
			shortest_distance = distance
			closest = part

	return closest


func find_nearest(nodes: Array, place: Vector2) -> Node:

	var shortest_distance: float = INF
	var nearest = null

	for node in nodes:

		if node.is_queued_for_deletion():
			continue

		var distance = node.position.distance_to(place)

		if distance < shortest_distance:
			shortest_distance = distance
			nearest = node

	return nearest


func update_hovered_part() -> void:

	var areas = mouse_area.get_overlapping_areas()

	match areas.size():
		0: hovered_part = null
		1: hovered_part = get_part_for_area(areas[0])
		_: hovered_part = get_part_for_area(find_nearest(areas, mouse_area.position))

	if hovered_part:
		hovered_part_outline.visible = true
		hovered_part_outline.position = hovered_part.position
		hovered_part_outline.rotation = hovered_part.angle
		hovered_part_outline_sprite.texture = hovered_part.sprite.texture
	else:
		hovered_part_outline.visible = false


func get_area_for_part(part: CraftDisplayPart) -> Area2D:
	return area_for_part.get(part.get_instance_id())


func get_part_for_area(area: Area2D) -> CraftDisplayPart:
	return part_for_area.get(area.get_instance_id())



func _on_hitbox_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:

		match event.button_index:

			MOUSE_BUTTON_LEFT:
				if event.pressed:
					if hovered_part != null:
						part_controls.clear()
						drag_offset = event.position - hovered_part.global_position
						DragEmitter.drag(self, hovered_part.to_blueprint())
						remove_part(hovered_part)
						hovered_part = null
					else:
						panning = true
						part_controls.clear()
				else:
					panning = false


			MOUSE_BUTTON_WHEEL_UP:
				zoom(1)

			MOUSE_BUTTON_WHEEL_DOWN:
				zoom(-1)

	elif event is InputEventMouseMotion:

		if panning:
			canvas.position += event.relative
			update_camera()
			part_controls.update_transform(canvas)

		elif !dragged_part:
			mouse_area.global_position = mouse_area.get_global_mouse_position()
			update_hovered_part()


func _on_parts_inventory_part_picked(part_data: CraftPartData) -> void:
	part_controls.clear()
	picked_part_preview.set_part_data(part_data)


func _on_parts_inventory_part_stored() -> void:
	picked_part_preview.clear()


func _on_hitbox_drag_receiver_drag_enter() -> void:

	picked_part_preview.clear()

	var blueprint: CraftBlueprintPart

	if DragEmitter.data is CraftBlueprintPart:
		blueprint = DragEmitter.data

	elif DragEmitter.data is CraftPartData:
		blueprint = CraftBlueprintPart.new(DragEmitter.data)

	blueprint.place = canvas.get_local_mouse_position() - drag_offset

	# Adding children to the tree on the same frame as the mouse entered the
	# drag receiver triggers another mouse enter event, which causes a stack
	# overflow, pretty fucking bizarre.
	add_part.bind(blueprint).call_deferred()


func _on_hitbox_drag_receiver_drag_leave() -> void:

	picked_part_preview.set_part_data(dragged_part.part_data)

	remove_part(dragged_part)

	dragged_part = null
	drag_offset = Vector2.ZERO


func _on_hitbox_drag_receiver_drag_over() -> void:
	if dragged_part: # Might be null because we defer setting it
		var place = canvas.get_local_mouse_position() - drag_offset
		dragged_part.position = snapped(place, GRID_SNAP)
		get_area_for_part(dragged_part).position = dragged_part.position


func _on_hitbox_drag_receiver_got_data(_source, _data) -> void:
	part_controls.set_part(dragged_part)
	part_controls.update_transform(canvas)
	dragged_part = null


func _on_part_controls_rotated(angle) -> void:
	get_area_for_part(part_controls.part).rotation = angle


func _on_save_button_pressed() -> void:
	if Game.current_player:
		Game.current_player.current_blueprint = craft_display.to_blueprint()
		Game.current_player.blueprints[0] = Game.current_player.current_blueprint
		PagesManager.go_to(GameConfig.Routes.LOBBY)
