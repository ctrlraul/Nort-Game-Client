class_name CraftPart extends Node2D

signal destroyed()



@export var ShinyPartMaterial: Material

@onready var __hitbox_collision_shape: CollisionShape2D = %CollisionShape2D
@onready var __sprite: Sprite2D = %Sprite2D

var gimmick: Node2D
var body: CraftBody
var hull: float = 10
var hull_max: float = 10
var is_destroyed: bool = false
var blueprint: CraftBlueprintPart

var color: Color = GameConfig.FACTIONLESS_COLOR :
	set(value):
		__sprite.self_modulate = lerp(GameConfig.FACTIONLESS_COLOR, value, max(0, hull) / hull_max)
		__hitbox_collision_shape.debug_color = value * Color(1, 1, 1, 0.2)
		color = value



func set_blueprint(value: CraftBlueprintPart) -> void:

	blueprint = value

	position = blueprint.place
	rotation = blueprint.angle

	hull_max = blueprint.data.definition.hull
	hull = hull_max

	__sprite.texture = Assets.get_part_texture(blueprint)
	__sprite.flip_h = blueprint.flipped
	__sprite.material = ShinyPartMaterial if blueprint.data.shiny else null

	var shape = RectangleShape2D.new()
	shape.size = __sprite.texture.get_size()
	__hitbox_collision_shape.shape = shape

	if Assets.is_core(blueprint):
		var core_light = Sprite2D.new()
		core_light.texture = Assets.CORE_LIGHT
		add_child(core_light)

	if blueprint.data.gimmick != null:
		gimmick = blueprint.data.gimmick.scene.instantiate()
		gimmick.position = position
		gimmick.part = self


func take_damage(damage: float) -> void:

	hull -= damage
	color = color

	if hull <= 0:

		if randf() <= __get_drop_rate():
			__drop()

		destroy()

		NodeUtils.remove_self(self)

		if gimmick != null:
			NodeUtils.remove_self(gimmick)


func destroy() -> void:
	is_destroyed = true
	destroyed.emit()



func __get_drop_rate() -> float:
	if Assets.is_core(blueprint):
		return GameConfig.DROP_RATE_CORE
	return GameConfig.DROP_RATE_HULL


func __drop() -> void:

	var setup = OrphanPartSetup.new()

	setup.place = global_position
	setup.definition = blueprint.data.definition
	setup.flipped = blueprint.flipped
	setup.gimmick = blueprint.data.gimmick
	setup.shiny = blueprint.data.shiny || randf() > GameConfig.DROP_RATE_SHINY

	Stage.spawn_dropped_part(setup)
