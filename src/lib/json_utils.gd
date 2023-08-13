class_name JSONUtils


static func from_file(path: String) -> Result:

	var file = FileAccess.open(path, FileAccess.READ)
	var open_error: int = FileAccess.get_open_error()

	if open_error != OK:
		return Result.with_error("Failed to read file '%s': %s" % [path, error_string(open_error)])

	var json = JSON.new()
	var parse_error: int = json.parse(file.get_as_text())

	if parse_error != OK:
		return Result.with_error("Failed to parse json: %s at line %s" % [json.get_error_message(), json.get_error_line()])

	return Result.with_value(json.data)


static func to_file(path: String, data, indent = "") -> Result:

	var create_dir_error = DirAccess.make_dir_recursive_absolute(path.get_base_dir())

	if create_dir_error != OK:
		return Result.with_error("Failed to write file '%s': %s" % [path, error_string(create_dir_error)])

	var file = FileAccess.open(path, FileAccess.WRITE)
	var open_error: int = FileAccess.get_open_error()

	if open_error != OK:
		return Result.with_error("Failed to write file '%s': %s" % [path, error_string(open_error)])

	file.store_string(JSON.stringify(data, indent))

	return Result.new()
