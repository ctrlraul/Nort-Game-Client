class_name Result


var value = null
var error: String = ""


static func val(_value) -> Result:
	var result = Result.new()
	result.value = _value
	return result

static func err(_error: String) -> Result:
	var result = Result.new()
	result.error = _error
	return result
