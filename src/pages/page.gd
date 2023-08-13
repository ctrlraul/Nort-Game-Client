class_name Page extends CanvasLayer



func _mount(_data) -> void:
	await TimeUtils.instant() # Just so the engine knows it can be a coroutine
