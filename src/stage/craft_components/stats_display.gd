class_name CraftComponentStats extends CraftComponent



@onready var hull: Node2D = %Hull
@onready var core: Node2D = %Core



func _ready() -> void:

	super()
	craft.set_component(CraftComponentStats, self)

	var radius = Assets.get_blueprint_radius(craft.blueprint)

	scale.x = radius
	position.y = radius + 20

	hull.modulate = craft.faction.color


func _process(_delta: float) -> void:
	hull.scale.x = clamp(craft.hull / craft.hull_max, 0, 1)
	core.scale.x = clamp(craft.core / craft.core_max, 0, 1)
