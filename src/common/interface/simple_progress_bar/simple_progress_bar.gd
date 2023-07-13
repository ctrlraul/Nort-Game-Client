@tool

class_name SimpleProgressBar extends Control



@onready var bar: ColorRect = %Bar

@export var color: Color :
	set(value):
		if is_inside_tree():
			bar.color = value
		color = value

@export var progress: float = 0.75 :
	set(value):
		if is_inside_tree():
			bar.scale.x = clamp(value, 0, 1)
		progress = value



func _ready() -> void:
	bar.color = color
	bar.scale.x = progress
