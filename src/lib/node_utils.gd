class_name NodeUtils



static func clear(node: Node, remove = true) -> void:
	for child in node.get_children():
		child.queue_free()
		if remove:
			node.remove_child(child)


static func find_nearest(nodes: Array, place: Vector2) -> Node:

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

				var distance = node.position.distance_to(place)

				if distance < shortest_distance:
					shortest_distance = distance
					nearest = node

			return nearest

