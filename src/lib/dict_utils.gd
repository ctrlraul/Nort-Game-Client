class_name DictUtils

static func get_or_default(dict: Dictionary, key: String, default = null):
	return dict.get(key) if dict.has(key) else default
