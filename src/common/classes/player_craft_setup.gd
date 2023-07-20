class_name PlayerCraftSetup extends EntitySetup



var test_blueprint: CraftBlueprint



func _init(source = null) -> void:

	super(source)

	type = EntitySetup.Type.PLAYER_CRAFT

	if source == null:
		test_blueprint = Assets.initial_blueprint
		return

	if source is Dictionary:
		test_blueprint = Assets.get_blueprint(source.test_blueprint)
		return

	assert(source == null, "Invalid source")



func to_dictionary() -> Dictionary:
	var dict = super()
	dict.test_blueprint = test_blueprint.id
	return dict
