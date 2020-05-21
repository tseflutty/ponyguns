using Godot;
using System;

//2019 © Даниил Белов
//Создано 24.06.2019

public class HeaderPanel : Control
{
    Texture[] Texures = new Texture[2];

    public override void _Ready() {
        Texures[0] = ResourceLoader.Load("res://Images/GUI/HeaderPanelLeft.png") as Texture;
    }

    public override void _Draw() {

    }
}
