class_name TractorTargetComponent extends EntityComponent



@onready var area: Area2D = %Area2D
@onready var animation_player: AnimationPlayer = %AnimationPlayer



var in_range: bool = false :
	set(value):
		if value:
			animation_player.play("show")
		else:
			animation_player.play("hide")



func _ready() -> void:
	super()
	animation_player.play("hide")
	entity.set_component(TractorTargetComponent, self)



func targeted(_by: Craft) -> void:
	pass


func released(_from: Craft) -> void:
	pass



func _on_animation_player_animation_finished(anim_name: StringName) -> void:
	if anim_name == "show":
		animation_player.play("rotate")
