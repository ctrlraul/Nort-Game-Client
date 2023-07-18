class_name MissionEditor extends Page



enum Action {
	NONE,
	MULTI_SELECT,
	DRAG_SELECTION,
	MOUSE_DOWN,
	PANNING,
}

const GRID_SNAP = Vector2.ONE * 32
const ZOOM_STEP = 0.1
const ZOOM_MIN = 0.1
const ZOOM_MAX = 1



@export var MissionSelectorPopupScene: PackedScene
@export var EditorCraftScene: PackedScene
@export var EditorPlayerCraftScene: PackedScene
@export var EditorOrphanPartScene: PackedScene

@onready var sandbox: Control = %Sandbox
@onready var canvas: Control = %Canvas
@onready var entities_container: Control = %EntitiesContainer
@onready var selection_rect: Panel = %SelectionRect
@onready var explorer: Explorer = %Explorer
@onready var mission_name_line_edit: LineEdit = %MissionNameLineEdit
@onready var mission_id_label: Label = %MissionIDLabel



var drag_offsets: Dictionary = {}
var logger: Logger = Logger.new("MissionEditor")
var selection: Array[EditorEntity] = []
var action: Action = Action.NONE



func _ready() -> void:
	update_stage_camera_transform()


func _mount(data) -> void:

	await Game.initialize()

	NodeUtils.clear(entities_container)

	if data != null:
		set_mission(data.mission)
	else:
		mission_id_label.text = Assets.generate_uid()
		add_entity(PlayerCraftSetup.new())

	Stage.clear()


func _input(event: InputEvent) -> void:

	if action == Action.NONE:
		return


	if event is InputEventMouseMotion:

		var mouse = canvas.get_local_mouse_position()

		match action:

			Action.MULTI_SELECT:
				selection_rect.size = abs(mouse - selection_rect.position)
				selection_rect.scale.x = -1 if mouse.x < selection_rect.position.x else 1
				selection_rect.scale.y = -1 if mouse.y < selection_rect.position.y else 1

			Action.DRAG_SELECTION:
				for entity in selection:
					entity.position = snapped(canvas.get_local_mouse_position() - drag_offsets[entity], GRID_SNAP)

			Action.MOUSE_DOWN:
				action = Action.PANNING
				canvas.position += event.relative
				update_stage_camera_transform()

			Action.PANNING:
				canvas.position += event.relative
				update_stage_camera_transform()



func zoom(delta: int) -> void:

	var change = canvas.scale.x + delta * ZOOM_STEP * canvas.scale.x
	var new_zoom = Vector2.ONE * clamp(change, ZOOM_MIN, ZOOM_MAX)

	if new_zoom != canvas.scale:

		var local_mouse = canvas.get_local_mouse_position()

		canvas.scale = new_zoom
		canvas.position -= local_mouse * canvas.scale * ZOOM_STEP * delta

		update_stage_camera_transform()


func update_stage_camera_transform() -> void:
	Stage.camera.zoom = canvas.scale
	Stage.camera.position = -canvas.position / canvas.scale


func sandbox_moused(event: InputEventMouseButton) -> void:

	if event.pressed:

		if Input.is_action_pressed("shift"):
			action = Action.MULTI_SELECT
			selection_rect.position = canvas.get_local_mouse_position()
			selection_rect.size = Vector2.ZERO

		else:
			action = Action.MOUSE_DOWN

	else:

		match action:

			Action.MULTI_SELECT:
				selection_rect.position = Vector2.ZERO
				selection_rect.size = Vector2.ZERO
				# Do select stuff in area

			Action.MOUSE_DOWN:
				clear_selection()
				explorer.clear()

		action = Action.NONE


func set_mission(mission: Mission) -> void:

	clear()

	mission_name_line_edit.text = mission.display_name
	mission_id_label.text = mission.id

	for entity_setup in mission.entities:
		add_entity(entity_setup)


func get_mission() -> Mission:

	var mission = Mission.new()

	mission.id = Assets.generate_uid() if mission_id_label.text == "" else mission_id_label.text
	mission.display_name = mission_name_line_edit.text

	for entity in entities_container.get_children():
		mission.entities.append(entity.get_entity_setup())

	return mission


func clear() -> void:

	mission_name_line_edit.text = ""
	mission_id_label.text = ""

	explorer.clear()
	clear_selection()

	NodeUtils.clear(entities_container)


func add_entity(setup: EntitySetup) -> EditorEntity:

	var Scene: PackedScene

	match setup.type:
		EntitySetup.Type.CRAFT: Scene = EditorCraftScene
		EntitySetup.Type.PLAYER_CRAFT: Scene = EditorPlayerCraftScene
		EntitySetup.Type.ORPHAN_PART: Scene = EditorOrphanPartScene

	assert(Scene != null)

	return add_entity_with_scene(setup, Scene)


func add_entity_with_scene(setup: EntitySetup, Scene: PackedScene) -> EditorEntity:

	var entity = Scene.instantiate() as EditorEntity

	entities_container.add_child(entity)

	entity.set_setup(setup)
	entity.pressed.connect(_on_entity_pressed.bind(entity))
	entity.drag_start.connect(_on_entity_drag_start.bind(entity))
	entity.drag_stop.connect(_on_entity_drag_stop)

	return entity


func clear_selection() -> void:
	for selected_entity in selection:
		selected_entity.selected = false
	selection.clear()


func select(entity: EditorEntity) -> void:

	if selection.has(entity):
		return

	if !Input.is_action_pressed("shift"):
		clear_selection()

	entity.selected = true
	selection.append(entity)


func store_mission(directory: String) -> void:

	var mission: Mission = get_mission()
	var path = directory.path_join(mission.id + ".json")
	var result = JSONUtils.to_file(path, mission.to_dictionary(), "\t")

	if result.error:
		var message = "Failed to export mission: %s" % result.error
		logger.error(message)
		PopupsManager.error(message)

	else:
		logger.info("Exported mission '%s'" % path)
		var popup = PopupsManager.info("Exported to '%s'" % path)
		popup.width = 700
		Assets.add_mission(mission)



func _on_sandbox_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		match event.button_index:
			MOUSE_BUTTON_LEFT: sandbox_moused(event)
			MOUSE_BUTTON_WHEEL_UP: zoom(1)
			MOUSE_BUTTON_WHEEL_DOWN: zoom(-1)


func _on_add_craft_button_pressed() -> void:
	explorer.set_entity(add_entity(CraftSetup.new()))


func _on_entity_pressed(entity: EditorEntity) -> void:
	explorer.set_entity(entity)
	select(entity)


func _on_entity_drag_start(entity: EditorEntity) -> void:

	explorer.clear()
	select(entity)

	for selected_entity in selection:
		drag_offsets[selected_entity] = canvas.get_local_mouse_position() - selected_entity.position

	action = Action.DRAG_SELECTION


func _on_entity_drag_stop() -> void:
	drag_offsets.clear()
	action = Action.NONE


func _on_test_button_pressed() -> void:
	Transition.callback(
		PagesManager.go_to.bind(GameConfig.Routes.MISSION, {
			"mission": get_mission(),
			"from_editor": true
		})
	)


func _on_export_button_pressed() -> void:

	if mission_name_line_edit.text == "":

		var randomize_name = func():
			mission_name_line_edit.text = str(randi())
			_on_export_button_pressed()

		var popup = PopupsManager.info("Your mission needs a name!")

		popup.add_button("Ok", mission_name_line_edit.grab_focus)
		popup.add_button("Randomize", randomize_name)

	elif !OS.has_feature("editor"):

		store_mission(Assets.LOCAL_MISSIONS_DIR)

	else:

		var save_locally = { "": false }
		var popup = PopupsManager.info("Where to save?") as DialogPopup

		popup.add_button("Locally", func(): save_locally[""] = true)
		popup.add_button("In-Game")

		await popup.removed

		if save_locally[""]:
			store_mission(Assets.LOCAL_MISSIONS_DIR)
		else:
			store_mission(GameConfig.CONFIG_PATH.path_join(Assets.MISSIONS_DIR))


func _on_import_button_pressed() -> void:
	var popup = PopupsManager.custom(MissionSelectorPopupScene)
	popup.mission_selected.connect(set_mission)
