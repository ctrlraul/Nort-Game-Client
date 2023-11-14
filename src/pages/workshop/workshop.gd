var color: Color : set = set_color



func _ready() -> void:
	part_controls.clear()
	dragged_part_preview.clear()
	Stage.clear()
	NodeUtils.clear(cores_list)
	canvas.scale = Vector2.ONE * 0.5
	update_camera()


func _mount(data) -> void:

	await Game.initialize()

	editor_mode = Game.dev || data.get("editor_mode", false)

	repopulate_inventory()

	if editor_mode:
		blueprint_id_input.visible = true
		blueprint_buttons.visible = true
		set_blueprint(Assets.initial_blueprint)
		blueprint_id_input.text = ""
		color = Assets.enemy_faction_1.color
	else:
		blueprint_id_input.visible = false
		blueprint_buttons.visible = false
		set_blueprint(Game.current_player.current_blueprint)
		color = Assets.player_faction.color



func set_color(value: Color) -> void:

	craft_display.color = value
	parts_inventory.color = value
	part_inspector.color = value

	dragged_part_preview.set_color(value)

	for item in cores_list.get_children():
		item.color = value

	color = value


func add_core_button(part_data: PartData) -> void:

	var item = CoresListItemScene.instantiate()
	var blueprint = CraftBlueprintPart.new(part_data)

	cores_list.add_child(item)

	item.part_data = part_data
	item.color = color
	item.pressed.connect(set_core_blueprint.bind(blueprint))
	item.mouse_entered.connect(part_inspector.set_part_data.bind(part_data))


func set_core_blueprint(blueprint: CraftBlueprintPart) -> void:
	craft_display.set_core_blueprint(blueprint)
	if hovered_part == craft_display.core:
		set_hovered_part_outline(blueprint)
	if part_controls.part == craft_display.core:
		part_controls.part = craft_display.core
	craft_summary.set_blueprint(craft_display.to_blueprint())


func set_blueprint(blueprint: CraftBlueprint) -> void:

	clear()

	blueprint_id_input.text = blueprint.id

	for part in blueprint.parts:
		add_part(part)

	if Game.current_player:
		craft_display.set_core_blueprint(blueprint.core) # this way it's not editable
	else:
		add_part(blueprint.core, true)

	part_inspector.set_part(craft_display.core)
	parts_inventory.set_blueprint(blueprint)

	craft_summary.set_blueprint(craft_display.to_blueprint())


func set_hovered_part_outline(blueprint: CraftBlueprintPart) -> void:

	var texture = Assets.get_part_texture(blueprint)
	var texture_size = texture.get_size()

	hovered_part_outline_sprite.texture = texture
	hovered_part_outline_sprite.size = texture_size
	hovered_part_outline_sprite.pivot_offset = texture_size * 0.5
	hovered_part_outline_sprite.position = -texture_size * 0.5
	hovered_part_outline_sprite.flip_h = blueprint.flipped


func clear() -> void:

#	blueprint_id_input.text = ""

	area_for_part.clear()
	part_for_area.clear()
	part_controls.clear()
	craft_display.clear()
	part_inspector.clear()

	hovered_part_outline.visible = false

	craft_summary.set_blueprint(craft_display.to_blueprint())

	repopulate_inventory()

	NodeUtils.clear(part_areas)


func repopulate_inventory() -> void:

	var hulls: Array[PartData]
	var cores: Array[PartData]

	if editor_mode:
		hulls = []
		cores = []
		for part in Assets.get_hulls():
			hulls.append(PartData.new(part))
		for part in Assets.get_cores():
			cores.append(PartData.new(part))

	else:
		hulls = Game.current_player.parts
		cores = Game.current_player.cores

	parts_inventory.clear()
	parts_inventory.set_parts(hulls)

	NodeUtils.clear(cores_list)

	for core in cores:
		add_core_button(core)

	cores_list_container.visible = cores.size() > 1


func zoom(delta: int) -> void:

	var change = canvas.scale.x + delta * ZOOM_STEP * canvas.scale.x
	var new_zoom = Vector2.ONE * clamp(change, ZOOM_MIN, ZOOM_MAX)

	if new_zoom == canvas.scale:
		return

	var local_mouse = canvas.get_local_mouse_position()

	canvas.scale = new_zoom
	canvas.position -= local_mouse * canvas.scale * ZOOM_STEP * delta

	update_camera()

	part_controls.update_transform(canvas)


func update_camera() -> void:
	camera.zoom = canvas.scale
	camera.position = -canvas.position / canvas.scale


func add_part(blueprint: CraftBlueprintPart, core = false) -> CraftDisplayPart:

	var part: CraftDisplayPart

	if core:
		part = craft_display.set_core_blueprint(blueprint)
	else:
		part = craft_display.add_part(blueprint)

	var area = Area2D.new()
	var shape = CollisionShape2D.new()

	area.position = blueprint.place
	area.collision_layer = 256
	area.rotation = blueprint.angle
	shape.shape = RectangleShape2D.new()
	shape.shape.size = Assets.get_part_texture(blueprint.data.definition.id).get_size()

	area.add_child(shape)
	part_areas.add_child(area)

	area_for_part[part.get_instance_id()] = area
	part_for_area[area.get_instance_id()] = part

	return part


func add_and_drag_part(blueprint: CraftBlueprintPart) -> void:
	dragged_part = add_part(blueprint)
	hovered_part_outline.visible = false
	craft_summary.set_blueprint(craft_display.to_blueprint())


func remove_part(part: CraftDisplayPart) -> void:

	if part == null:
		return

	var area: Node2D = get_area_for_part(part)

	part_for_area.erase(area)
	area_for_part.erase(part)

	area.queue_free()
	part.queue_free()

	craft_summary.set_blueprint(craft_display.to_blueprint())


func update_hovered_part() -> void:

	var areas = mouse_area.get_overlapping_areas()

	match areas.size():
		0: hovered_part = null
		1: hovered_part = get_part_for_area(areas[0])
		_: hovered_part = get_part_for_area(NodeUtils.find_nearest(areas, mouse_area.position))

	if hovered_part:
		hovered_part_outline.visible = true
		hovered_part_outline.position = hovered_part.position
		hovered_part_outline.rotation = hovered_part.angle
		set_hovered_part_outline(hovered_part.to_blueprint())
		part_inspector.set_part(hovered_part)
	else:
		hovered_part_outline.visible = false


func get_area_for_part(part: CraftDisplayPart) -> Area2D:
	return area_for_part.get(part.get_instance_id())


func get_part_for_area(area: Area2D) -> CraftDisplayPart:
	return part_for_area.get(area.get_instance_id())


func select(event: InputEventMouseButton) -> void:

	if !event.pressed:
		panning = false
		return

	if hovered_part == null:
		panning = true
		part_controls.clear()
		hovered_part_outline.visible = false
		return

	if hovered_part == craft_display.core:
		part_inspector.set_part(hovered_part)
		part_controls.part = hovered_part
		part_controls.update_transform(canvas)

	else:
		part_controls.clear()
		drag_offset = event.position - hovered_part.global_position
		DragEmitter.drag(self, hovered_part.to_blueprint())
		remove_part(hovered_part)
		hovered_part = null



func _on_hitbox_gui_input(event: InputEvent) -> void:

	if event is InputEventMouseButton:

		match event.button_index:

			MOUSE_BUTTON_LEFT:
				select(event)

			MOUSE_BUTTON_WHEEL_UP:
				zoom(1)

			MOUSE_BUTTON_WHEEL_DOWN:
				zoom(-1)

	elif event is InputEventMouseMotion:

		if panning:
			canvas.position += event.relative
			update_camera()
			part_controls.update_transform(canvas)

		elif dragged_part == null:
			mouse_area.global_position = mouse_area.get_global_mouse_position()
			update_hovered_part()


func _on_parts_inventory_part_picked(part_data: PartData) -> void:
	part_controls.clear()
	dragged_part_preview.set_part_data(part_data)


func _on_parts_inventory_part_stored() -> void:
	dragged_part_preview.clear()


func _on_parts_inventory_part_hovered(part_data: PartData) -> void:
	part_inspector.set_part_data(part_data)


func _on_hitbox_drag_receiver_drag_enter() -> void:

	dragged_part_preview.clear()

	var blueprint: CraftBlueprintPart

	if DragEmitter.data is CraftBlueprintPart:
		blueprint = DragEmitter.data

	elif DragEmitter.data is PartData:
		blueprint = CraftBlueprintPart.new(DragEmitter.data)

	blueprint.place = snapped(canvas.get_local_mouse_position() - drag_offset, GRID_SNAP)

	# Adding children to the tree on the same frame as the mouse entered the
	# drag receiver triggers another mouse enter event, which causes a stack
	# overflow, pretty fucking bizarre.
	add_and_drag_part.call_deferred(blueprint)


func _on_hitbox_drag_receiver_drag_leave() -> void:

	dragged_part_preview.set_part_data(dragged_part.part_data)

	remove_part(dragged_part)

	dragged_part = null
	drag_offset = Vector2.ZERO


func _on_hitbox_drag_receiver_drag_over() -> void:
	if dragged_part: # Might be null because we defer setting it
		var place = canvas.get_local_mouse_position() - drag_offset
		dragged_part.position = snapped(place, GRID_SNAP)
		get_area_for_part(dragged_part).position = dragged_part.position


func _on_hitbox_drag_receiver_got_data(_source, _data) -> void:
	part_controls.part = dragged_part
	part_controls.update_transform(canvas)
	dragged_part = null


func _on_part_controls_rotated(angle) -> void:
	get_area_for_part(part_controls.part).rotation = angle


func _on_export_button_pressed() -> void:

	var blueprint = craft_display.to_blueprint()

	if blueprint_id_input.text != "":
		blueprint.id = blueprint_id_input.text

	var path = GameConfig.CONFIG_PATH.path_join(Assets.BLUEPRINTS_DIR).path_join(blueprint.id + ".json")
	var result = JSONUtils.to_file(path, blueprint.to_dictionary(), "\t")

	if result.error:
		var message = "Failed to export blueprint: %s" % result.error
		logger.error(message)
		PopupsManager.error(message)

	else:
		logger.info("Exported blueprint '%s'" % path)
		var popup = PopupsManager.info("Exported to '%s'" % path)
		popup.width = 700
		Assets.add_blueprint(blueprint)


func _on_import_button_pressed() -> void:
	var popup = PopupsManager.custom(BlueprintSelectorPopupScene)
	popup.blueprint_selected.connect(set_blueprint)


func _on_cores_drag_receiver_got_data(_source, data) -> void:

	if data is PartData:
		add_core_button(data)
	elif data is CraftBlueprintPart:
		add_core_button(data.data)

	dragged_part_preview.clear()


func _on_theme_resized() -> void:
	%SubViewport.size = get_viewport().size


func _on_build_button_pressed() -> void:
	if Game.current_player:
		Game.current_player.current_blueprint = craft_display.to_blueprint()
		Game.current_player.blueprints[0] = Game.current_player.current_blueprint
		PlayerManager.store_local_player(Game.current_player)
	PagesManager.go_to(GameConfig.Routes.LOBBY)


func _on_clear_button_pressed() -> void:
	clear()
