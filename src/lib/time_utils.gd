extends Node



func seconds(amount: float) -> Signal:
	return get_tree().create_timer(amount).timeout


func milliseconds(amount: float) -> Signal:
	return get_tree().create_timer(amount / 1000).timeout


func instant() -> Signal:
	return milliseconds(1)
