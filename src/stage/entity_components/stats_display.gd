class_name StatsComponent extends EntityComponent



@onready var hull: Node2D = %Hull
@onready var core: Node2D = %Core



func _ready() -> void:

	super()
	craft.set_component(StatsComponent, self)

	var half_size = Assets.get_blueprint_size(craft.blueprint) * 0.5

	scale.x = half_size.x
	position.y = half_size.y + 10

	hull.modulate = craft.faction.color


func _process(_delta: float) -> void:
	hull.scale.x = clamp(craft.hull / craft.hull_max, 0, 1)
	core.scale.x = clamp(craft.core / craft.core_max, 0, 1)
