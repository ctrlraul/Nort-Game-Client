class_name CraftBlueprint



class BlueprintStats:
	var core: int = 0
	var hull: int = 0
	var mass: int = 0
	var torque: int = 0
	var acceleration: int = 0



var id: String
var core: CraftBlueprintPart
var parts: Array[CraftBlueprintPart] = []


static func get_hull(blueprint: CraftBlueprint) -> int:

	var result = 0

	result += blueprint.core.data.definition.hull

	for part in blueprint.parts:
		result += part.data.definition.hull

	return result


static func get_mass(blueprint: CraftBlueprint) -> int:

	var result = 0

	result += get_part_mass(blueprint.core)

	for part in blueprint.parts:
		result += get_part_mass(part)

	return result


static func get_stats(blueprint: CraftBlueprint) -> BlueprintStats:

	var stats = BlueprintStats.new()

	# TODO
	stats.torque = 1
	stats.acceleration = 1

	if blueprint.core != null && blueprint.core.data != null:
		stats.core += blueprint.core.data.definition.core
		stats.hull += blueprint.core.data.definition.hull
		stats.mass += get_part_mass(blueprint.core)

	for part in blueprint.parts:
		stats.core += part.data.definition.core
		stats.hull += part.data.definition.hull
		stats.mass += get_part_mass(part)

	return stats


static func get_part_mass(part: CraftBlueprintPart) -> int:

	var mass = 0

	mass += part.data.definition.mass

	if part.data.gimmick:
		mass += part.data.gimmick.mass

	if part.data.shiny:
		mass = round(mass * 0.9)

	return mass
