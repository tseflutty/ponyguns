extends Control

# Declare member variables here. Examples:
# var a = 2
# var b = "text"

func _input(event):
	if (event is InputEventMouseButton):
		print("Click in ", event.position)
	if (event is InputEventMouseMotion):
		print("Move to ", event.position, " ", get_viewport().get_mouse_position())
