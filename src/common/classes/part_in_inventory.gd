class_name PartInInventory extends PartData



var count: int = 0



func _init(source = null) -> void:

	super(source)

	if source is Dictionary:
		count = source.get("count", 1)
		return

	assert(source == null, "Invalid source")



func to_dictionary() -> Dictionary:

	var dict = super()

	if count > 1:
		dict.count = count

	return dict
