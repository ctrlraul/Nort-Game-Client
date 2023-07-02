class_name DragData

var source: Control
var value = null

@warning_ignore("shadowed_variable")
func _init(source: Control, value = null) -> void:
	self.source = source
	self.value = value
