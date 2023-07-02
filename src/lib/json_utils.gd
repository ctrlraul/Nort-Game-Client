class_name JSONUtils


static func from_file(path: String) -> Result:

	var file = FileAccess.open(path, FileAccess.READ)
	var open_error: int = FileAccess.get_open_error()

	if open_error != OK:
		return Result.err("Failed to read file '%s': %s" % [path, error_string(open_error)])

	var json = JSON.new()
	var parse_error: int = json.parse(file.get_as_text())

	if parse_error != OK:
		return Result.err("Failed to parse json: %s at line %s" % [path, json.get_error_message(), json.get_error_line()])

	return Result.val(json.data)


static func to_file(path: String, data, indent = "") -> Result:

	var file = FileAccess.open(path, FileAccess.WRITE)
	var open_error: int = FileAccess.get_open_error()

	if open_error != OK:
		return Result.err("Failed to write file '%s': %s" % [path, error_string(open_error)])

	file.store_string(JSON.stringify(data, indent))

	return Result.new()
