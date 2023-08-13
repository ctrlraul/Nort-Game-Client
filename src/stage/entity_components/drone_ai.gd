class_name DroneAIComponent extends EntityComponent



const __FRAMES_BETWEEN_CHECKS = 5
const __PATH_POINT_DISTANCE_TOLERANCE = 200



var __flight_comp: FlightComponent
var __check_time_offset = randi() % __FRAMES_BETWEEN_CHECKS
var path: Array[Vector2] = []



func _ready() -> void:
	super()
	craft.set_component(DroneAIComponent, self)


func _post_ready() -> void:
	__flight_comp = craft.get_component(FlightComponent)


func _physics_process(_delta: float) -> void:

	if path.size() == 0:
		return

	__flight_comp.direction = craft.position.direction_to(path[0])

	if Engine.get_frames_drawn() % __FRAMES_BETWEEN_CHECKS == __check_time_offset:
		if craft.position.distance_to(path[0]) < __PATH_POINT_DISTANCE_TOLERANCE:
			path.pop_front()



func _on_area_2d_area_entered(area: Area2D) -> void:
	var craft_part: CraftPart = area.owner
	if craft_part.body.craft.faction != craft.faction:
		path.push_front(lerp(craft.position, craft_part.body.craft.position, 0.5))


func _on_area_2d_area_exited(area: Area2D) -> void:
	var craft_part: CraftPart = area.owner
	if craft_part.body.craft.faction != craft.faction:
		path.push_front(craft_part.body.craft.position)
