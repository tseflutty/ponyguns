extends Spatial

# Declare member variables here. Examples:
# var a = 2
# var b = "text"

# Called when the node enters the scene tree for the first time.
func _ready():
	$Animation.play('Dead')

func _on_Animation_animation_finished(anim_name):
	queue_free()
