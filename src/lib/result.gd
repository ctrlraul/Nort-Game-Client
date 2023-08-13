class_name Result


var value = null
var error: String = ""



func has_error() -> bool:
	return error != ""



static func with_value(val) -> Result:
	var result = Result.new()
	result.value = val
	return result


static func with_error(message: String) -> Result:
	var result = Result.new()
	result.error = message
	return result
