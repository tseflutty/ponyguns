using Godot;
using System;

//2019 © Даниил Белов
//Создано 14.07.2019

public class AtackListener : Area
{
    [Signal]
    public delegate void AtackListened(int damage, Spatial damager);

    ///Выделитель хитбокса
    private Spatial Viewer;

    ///Отображать ли выделитель
    public static bool ShowViewer = false;

    [Export]
    ///Теги приёмщика атак
    public int[] Tags = new int[0];

    public override void _Ready() {
        Viewer = GetNode("Viewer") as Spatial;
    }

    public override void _PhysicsProcess(float delta) {
        Viewer.Visible = ShowViewer;
    }

    public void GiveDamage(int damage, Spatial damager = null) {
        EmitSignal(nameof(AtackListened), damage, damager);
    }
    
}
