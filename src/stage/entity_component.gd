class_name EntityComponent extends Node2D



@onready var entity: Node2D = get_parent()
@onready var craft: Craft = entity as Craft
@onready var dropped_part: DroppedPart = entity as DroppedPart



func _ready() -> void:
	_post_ready.call_deferred()


func _post_ready() -> void:
	pass
