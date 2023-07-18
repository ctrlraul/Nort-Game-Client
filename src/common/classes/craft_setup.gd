class_name CraftSetup extends EntitySetup



enum Behavior {
	NONE,
	PLAYER,
	FIGHTER,
	DRONE,
	TURRET,
	CARRIER,
	OUTPOST
}



var blueprint: CraftBlueprint
var faction: Faction
var behavior: Behavior



func _init(source = null) -> void:

	super(source)

	type = EntitySetup.Type.CRAFT

	if source == null:
		blueprint = Assets.initial_blueprint
		faction = Assets.enemy_faction_1
		behavior = Behavior.NONE
		return

	if source is Dictionary:
		blueprint = Assets.get_blueprint(source.blueprint)
		faction = Assets.get_faction(source.faction)
		behavior = Behavior[source.behavior]
		return

	assert(source == null, "Invalid source")



func to_dictionary() -> Dictionary:

	var dict = super()

	dict.blueprint = blueprint.id
	dict.faction = faction.id
	dict.behavior = Behavior.find_key(behavior)

	return dict
