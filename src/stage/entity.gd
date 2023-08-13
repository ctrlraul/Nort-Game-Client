class_name Entity extends Node2D



var __components: Dictionary = {}



func get_component(type):
	return __components.get(type, null)


func set_component(type, instance: Node) -> void:
	if __components.has(type):
		push_error("Only one instance of each component allowed")
	else:
		__components[type] = instance
