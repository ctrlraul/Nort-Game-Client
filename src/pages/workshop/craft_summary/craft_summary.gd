class_name CraftSummary extends PanelContainer



@onready var core_label: Label = %CoreLabel
@onready var hull_label: Label = %HullLabel
@onready var mass_label: Label = %MassLabel
@onready var parts_label: Label = %PartsLabel



func set_blueprint(blueprint: CraftBlueprint) -> void:

	var stats = CraftBlueprint.get_stats(blueprint)

	core_label.text = str(stats.core)
	hull_label.text = str(stats.hull)
	mass_label.text = str(stats.mass)

	parts_label.text = "Parts: %s" % (blueprint.parts.size() + 1)
