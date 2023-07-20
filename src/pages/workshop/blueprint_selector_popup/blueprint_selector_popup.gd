class_name BlueprintSelectorPopup extends GenericPopup

signal blueprint_selected(blueprint: CraftBlueprint)



@export var BlueprintsListItemScene: PackedScene

@onready var blueprints_list: VBoxContainer = %BlueprintsList



func _ready() -> void:

	super()

	cancelable = true

	NodeUtils.clear(blueprints_list)

	await Game.initialize()

	for blueprint in Assets.get_blueprints():
		var item = BlueprintsListItemScene.instantiate()
		blueprints_list.add_child(item)
		item.set_blueprint(blueprint)
		item.selected.connect(
			func(blueprint):
				blueprint_selected.emit(blueprint)
				remove()
		)
