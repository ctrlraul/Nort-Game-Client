class_name DroppedPartSetup



var definition: CraftPartDefinition
var place: Vector2
var angle: float
var flipped: bool
var gimmick: Gimmick
var shiny: bool



func _init(source = null) -> void:

	if source is Dictionary:
		definition = source.definition
		place = Vector2(source.x, source.y)
		angle = deg_to_rad(source.angle) if source.has("angle") else 0.0
		flipped = source.get("flipped", false)
		gimmick = Assets.get_gimmick(source.gimmick) if source.has("gimmick") else null
		shiny = source.get("shiny", false)

	assert(source == null, "Invalid source")



func to_dictionary() -> Dictionary:

	var dict = {
		"definition": definition.id,
		"x": round(place.x),
		"y": round(place.y),
#		"angle": ,
#		"flipped": ,
#		"gimmick": ,
#		"shiny": ,
	}

	if angle != 0:
		dict.angle = round(rad_to_deg(angle))

	if flipped:
		dict.flipped = true

	if gimmick != null:
		dict.gimmick = gimmick.id

	if shiny:
		dict.shiny = true

	return dict
