extends Node
signal page_changed()
signal page_mounted()
signal page_change_error()



@onready var tree: SceneTree = get_tree()



var default_scene_path: String
var history: Array[String] = []

var current_page: Page :
	get:
		return tree.current_scene if tree.current_scene is Page else null



func _ready() -> void:

	assert(default_scene_path != "", "Set default scene path asap")

	if tree.current_scene is Page:
		await tree.current_scene._mount(null)
		page_mounted.emit()



func go_to(scene_path: String, data = null) -> Signal:

	tree.node_added.connect(_on_tree_node_added.bind(scene_path, data))

	var err = tree.change_scene_to_file(scene_path)

	if err != OK:
		page_change_error.emit("Failed to go to page '%s': %s" % [scene_path, error_string(err)])
		tree.node_added.disconnect(_on_tree_node_added)
		return tree.create_timer(0.0001).timeout

	else:
		history.append(scene_path)

	return page_mounted


func pop(data = null) -> Signal:

	var scene_path = history.pop_front()

	if scene_path == null:
		scene_path = default_scene_path

	return __change_scene(scene_path, data)



func __change_scene(scene_path: String, data = null) -> Signal:

	tree.node_added.connect(_on_tree_node_added.bind(scene_path, data))

	var err = tree.change_scene_to_file(scene_path)

	if err != OK:
		page_change_error.emit("Failed to go to page '%s': %s" % [scene_path, error_string(err)])

	return page_mounted



func _on_tree_node_added(node: Node, scene_path: String, data) -> void:

	if node.scene_file_path != scene_path:
		return

	tree.node_added.disconnect(_on_tree_node_added)

	page_changed.emit()

	assert(node is Page, "All pages should extend the Page class")

	await node.ready
	await node._mount(data)

	page_mounted.emit()
