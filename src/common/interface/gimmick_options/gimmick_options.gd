extends OptionButton

signal gimmick_selected(gimmick: Gimmick)



var gimmicks: Array[Gimmick] = [null]



func _ready() -> void:

	clear()

	await Game.initialize()

	add_item("Passive")

	for gimmick in Assets.gimmicks.values():
		gimmicks.append(gimmick)
		add_icon_item(Assets.get_gimmick_texture(gimmick), " " + gimmick.display_name)



func set_gimmick(gimmick: Gimmick) -> void:
	selected = gimmicks.find(gimmick)



func _on_item_selected(index: int) -> void:
	gimmick_selected.emit(gimmicks[index])
