class_name MissionSelectorPopup extends GenericPopup

signal mission_selected(mission: CraftBlueprint)



@export var MissionsListItemScene: PackedScene

@onready var missions_list: VBoxContainer = %MissionsList



func _ready() -> void:

	super()

	cancellable = true

	NodeUtils.clear(missions_list)

	await Game.initialize()

	for mission in Assets.get_missions():
		var item = MissionsListItemScene.instantiate()
		missions_list.add_child(item)
		item.set_mission(mission)
		item.selected.connect(
			func(mission):
				mission_selected.emit(mission)
				remove()
		)
