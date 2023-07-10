class_name CraftComponent extends Node2D



@onready var craft: Craft = get_parent()



func _ready() -> void:
	_post_ready.call_deferred()


func _post_ready() -> void:
	pass
