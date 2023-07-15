class_name MissionEditor extends Page



const GRID_SNAP = Vector2.ONE * 16
const ZOOM_STEP = 0.1
const ZOOM_MIN = 0.2
const ZOOM_MAX = 1

const EditorCraftScene = preload("res://pages/mission_editor/editor_objects/editor_craft.tscn")



@onready var container: Control = %Container
@onready var explorer: Explorer = %Explorer
@onready var mission_name_line_edit: LineEdit = %MissionNameLineEdit
@onready var mission_id_label: Label = %MissionIDLabel



var logger: Logger = Logger.new("MissionEditor")
var panning: bool = false



func _mount(_data) -> void:

	await Game.initialize()

	NodeUtils.clear(container)

	mission_id_label.text = Assets.generate_uid()



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


func get_mission() -> Mission:

	var mission = Mission.new()

	mission.id = Assets.generate_uid() if mission_id_label.text == "" else mission_id_label.text
	mission.display_name = mission_name_line_edit.text

	for entity in container.get_children():
		mission.entities.append(entity.get_entity_setup())

	return mission



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

	var craft: EditorCraft = EditorCraftScene.instantiate()

	container.add_child(craft)

	craft.set_blueprint(Assets.initial_blueprint)
	craft.set_faction(Assets.player_faction)
	craft.set_behavior(CraftSetup.Behavior.find_key(CraftSetup.Behavior.FIGHTER))

	explorer.set_object(craft)

	craft.selected.connect(func(): explorer.set_object(craft))


func _on_test_button_pressed() -> void:

	var mission: Mission = get_mission()

	Transition.callback(
		PagesManager.go_to.bind(GameConfig.Routes.MISSION, { "mission": mission })
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
