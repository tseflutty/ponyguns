extends Node

func dummy(i):
	return i

func _physics_process(delta):
	dummy(delta)
	OS.set_window_title("ProjectPonyGuns  –  FPS: " + str(Engine.get_frames_per_second()))
