extends Node

#2019 © Даниил Белов
#Создано 28.05.2019

#Загрузочный экран
onready var packed_load_screen = preload("res://GUI/LoadingScreen.tscn");

onready var root = get_node("/root")

#Загрузочный экран, отображаемый в данный момент
var current_load_screen : Node

var current_interactive_ldr : ResourceInteractiveLoader

func dummy(i):
	return i

func _physics_process(delta):
	dummy(delta)
	#Если интерактивный загрузчик присутствует
	if (current_interactive_ldr != null):
		#Каждый процесс обработки подгружаются данные
		var err = current_interactive_ldr.poll()
		#Если даннве бвли полностью загружены
		if(err == ERR_FILE_EOF):
			print("load done")
			#Удаляется загрузочный экран
			current_load_screen.queue_free()
			#Загруженнная сцена инсталируется
			var scene = current_interactive_ldr.get_resource().instance()
			root.add_child(scene)
			get_tree().current_scene = scene
			#Удаляется интерактивный загрузчик
			current_interactive_ldr = null

#Запустить процесс загрузки
func change_scene(path):
	#Удаляется текущая сцена если она присутствует
	if (get_tree().current_scene != null):
		get_tree().current_scene.queue_free()
		get_tree().current_scene = null
	print("load start")
	#Инсталируется загрузочный экран
	current_load_screen = packed_load_screen.instance()
	root.add_child(current_load_screen)
	#Создается и устанавливается новый интерактивный загрузчик
	current_interactive_ldr = ResourceLoader.load_interactive(path)
	
