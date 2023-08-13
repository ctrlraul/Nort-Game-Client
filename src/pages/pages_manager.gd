extends Node

signal __page_mounted()
signal page_changed()
signal page_change_error()



@onready var tree: SceneTree = get_tree()



var default_scene_path: String
var history: Array[String] = []



func _ready() -> void:

	assert(default_scene_path != "", "Set default scene path asap")

	var current_page: Page = get_current_page()

	if current_page:
		await current_page._mount(null)
		__page_mounted.emit()



func get_current_page() -> Page:
	if tree.current_scene is Page:
		return tree.current_scene
	return null


func go_to(scene_path: String, data: Dictionary = {}) -> Signal:

	if __change_page(scene_path, data).has_error():
		return TimeUtils.instant()

	history.append(scene_path)

	return __page_mounted


func pop(data: Dictionary = {}) -> Signal:

	history.pop_back()

	var scene_path = default_scene_path if history.is_empty() else history.back()

	if __change_page(scene_path, data).has_error():
		return TimeUtils.instant()

	return __page_mounted



func __change_page(scene_path: String, data = null) -> Result:

	var issue = tree.change_scene_to_file(scene_path)

	if issue != OK:
		var message = "Failed to go to page '%s': %s" % [scene_path, error_string(issue)]
		page_change_error.emit(message)
		return Result.with_error(message)

	tree.node_added.connect(_on_tree_node_added.bind(scene_path, data))

	return Result.new()



func _on_tree_node_added(node: Node, scene_path: String, data) -> void:

	if node.scene_file_path != scene_path:
		return

	tree.node_added.disconnect(_on_tree_node_added)

	page_changed.emit()

	assert(node is Page, "All pages should extend the Page class")

	await node.ready
	await node._mount(data)

	__page_mounted.emit()
