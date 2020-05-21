using Godot;
using System;

//2019 © Даниил Белов
//Создано 13.05.2019


///<summary>
///<para>Функции для анимированной загрузки</para>
///</summary>
public class PonyLoader : Node
{

    ///<summary>
    ///<para>Сменить сцену на сцену по пути (path). В (tree) указывать GetTree(). Пока сцена будет загружаться, будет проиграна анимация загрузки</para>
    ///</summary>
    public static void ChangeScene(String path, SceneTree tree) {
        Node loadingManager = tree.GetRoot().GetNode("LoadingManager");
        loadingManager.Call("change_scene", path);
    }
}
