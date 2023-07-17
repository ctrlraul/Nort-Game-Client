class_name ExplorerOptionsField extends ExplorerField



var get_options_method: Callable
var option_label_key: String



func _init(entity_: EditorEntity, key_: String, get_options_method_: Callable, option_label_key_: String) -> void:
	super(entity_, key_)
	get_options_method = get_options_method_
	option_label_key = option_label_key_
