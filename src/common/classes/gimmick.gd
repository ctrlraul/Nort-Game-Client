class_name Gimmick



const __BASE_SCENE_PATH = "res://gimmicks/"



var id: String
var display_name: String
var scene: PackedScene
var mass: int



func _init(source) -> void:
	id = source.id
	display_name = source.display_name
	scene = load(__BASE_SCENE_PATH.path_join(source.scene))
	mass = source.mass
