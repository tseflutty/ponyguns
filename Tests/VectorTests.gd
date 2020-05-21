extends Control

func _physics_process(delta):
	if Input.is_action_pressed("ui_left"):
		$a1point.position = get_local_mouse_position()
	if Input.is_action_pressed("ui_right"):
		$a2point.position = get_local_mouse_position()


func _on_Button_button_up():
	var a = ($a1point.position - $a1center.position).angle()
	var b = ($a2point.position - $a2center.position).angle()
	$sum.Text = str(abs(a - b))
	print(a, ' ', b, ' ', a - b)
