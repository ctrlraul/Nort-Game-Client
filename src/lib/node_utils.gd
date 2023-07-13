class_name NodeUtils



static func clear(node: Node, remove: bool = true) -> void:
	for child in node.get_children():
		child.queue_free()
		if remove:
			node.remove_child(child)


static func find_nearest(nodes: Array, place: Vector2, use_global_position: bool = false) -> Node:

	var position_prop = "global_position" if use_global_position else "position"

	match nodes.size():

		0:
			return null

		1:
			return nodes[0]

		_:
			var shortest_distance: float = INF
			var nearest = null

			for node in nodes:

				if node.is_queued_for_deletion():
					continue

				var distance = node[position_prop].distance_to(place)

				if distance < shortest_distance:
					shortest_distance = distance
					nearest = node

			return nearest


static func remove_self(node: Node, queue_free: bool = true) -> void:
	node.get_parent().remove_child(node)
	if queue_free:
		node.queue_free()
