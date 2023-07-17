class_name MissionEditor extends Page



const GRID_SNAP = Vector2.ONE * 16
const ZOOM_STEP = 0.1
const ZOOM_MIN = 0.1
const ZOOM_MAX = 1



@export var MissionSelectorPopupScene: PackedScene
@export var EditorCraftScene: PackedScene
@export var EditorPlayerCraftScene: PackedScene
@export var EditorOrphanPartScene: PackedScene

@onready var container: Control = %Container
@onready var explorer: Explorer = %Explorer
@onready var mission_name_line_edit: LineEdit = %MissionNameLineEdit
@onready var mission_id_label: Label = %MissionIDLabel



var logger: Logger = Logger.new("MissionEditor")
var panning: bool = false



func _mount(data) -> void:

	await Game.initialize()

	NodeUtils.clear(container)

	if data != null:
		set_mission(data.mission)
	else:
		mission_id_label.text = Assets.generate_uid()
		add_entity(PlayerCraftSetup.new())

	Stage.clear()



func zoom(delta: int) -> void:

	var change = container.scale.x + delta * ZOOM_STEP * container.scale.x
	var new_zoom = Vector2.ONE * clamp(change, ZOOM_MIN, ZOOM_MAX)

	if new_zoom != container.scale:

		var local_mouse = container.get_local_mouse_position()

		container.scale = new_zoom
		container.position -= local_mouse * container.scale * ZOOM_STEP * delta


func select(event: InputEventMouseButton) -> void:

	if !event.pressed:
		panning = false
		return

	panning = true


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

	for entity in container.get_children():
		mission.entities.append(entity.get_entity_setup())

	return mission


func clear() -> void:
	mission_name_line_edit.text = ""
	mission_id_label.text = ""
	explorer.clear()
	NodeUtils.clear(container)


func add_entity(setup: EntitySetup) -> EditorEntity:

	var Scene: PackedScene

	match setup.type:
		EntitySetup.Type.CRAFT: Scene = EditorCraftScene
		EntitySetup.Type.PLAYER_CRAFT: Scene = EditorPlayerCraftScene
		EntitySetup.Type.ORPHAN_PART: Scene = EditorOrphanPartScene

	assert(Scene != null)

	return add_entity_with_scene(setup, Scene)


func add_entity_with_scene(setup: EntitySetup, Scene: PackedScene) -> EditorEntity:

	var entity = Scene.instantiate()

	container.add_child(entity)

	entity.set_setup(setup)
	entity.selected.connect(func(): explorer.set_entity(entity))

	return entity



func _on_sandbox_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:

		match event.button_index:
			MOUSE_BUTTON_LEFT: select(event)
			MOUSE_BUTTON_WHEEL_UP: zoom(1)
			MOUSE_BUTTON_WHEEL_DOWN: zoom(-1)

	elif event is InputEventMouseMotion:
		if panning:
			container.position += event.relative


func _on_add_craft_button_pressed() -> void:
	explorer.set_entity(add_entity(CraftSetup.new()))



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

		return


	var mission: Mission = get_mission()

	var path = GameConfig.CONFIG_PATH.path_join(Assets.MISSIONS_DIR).path_join(mission.id + ".json")
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


func _on_import_button_pressed() -> void:
	var popup = PopupsManager.custom(MissionSelectorPopupScene)
	popup.mission_selected.connect(set_mission)
