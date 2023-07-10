class_name Logger



const __FORMAT = "%s [%s] %s - %s"



var __label: String



func _init(label: String) -> void:
	__label = label



func info(message: String) -> void:
	print(__FORMAT % [Time.get_ticks_msec() / 1000.0, "i", __label, message])


func error(message: String) -> void:
	printerr(__FORMAT % [Time.get_ticks_msec() / 1000.0, "E", __label, message])


static func error_static(label: String, message: String) -> void:
	printerr(__FORMAT % [Time.get_ticks_msec() / 1000.0, "E", label, message])
