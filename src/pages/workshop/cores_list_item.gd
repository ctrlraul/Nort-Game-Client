extends Button



@export var ShinyPartMaterial: ShaderMaterial

@onready var craft_part_display: Control = %CraftPartDisplay
@onready var core_light: TextureRect = %CoreLight



var color: Color :
	set(value): craft_part_display.color = value
	get: return craft_part_display.color

var part_data: CraftPartData :
	set(value):

		craft_part_display.definition = value.definition
		craft_part_display.shiny = value.shiny

		core_light.visible = Assets.is_core(value)

		part_data = value



func _on_mouse_entered() -> void:
	craft_part_display.outline = true


func _on_mouse_exited() -> void:
	craft_part_display.outline = false
