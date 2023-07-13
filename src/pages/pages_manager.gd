extends Node
signal page_changed()
signal page_mounted()



@onready var tree: SceneTree = get_tree()


var current_page: Page :
	get:
		return tree.current_scene if tree.current_scene is Page else null



func _ready() -> void:
	if tree.current_scene is Page:
		await tree.current_scene._mount(null)
		page_mounted.emit()



func go_to(scene_path: String, data = null) -> Signal:
	tree.node_added.connect(_on_tree_node_added.bind(scene_path, data))
	tree.change_scene_to_file(scene_path)
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
