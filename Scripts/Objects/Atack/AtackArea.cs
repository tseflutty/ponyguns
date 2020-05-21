using Godot;
using System;

//2019 © Даниил Белов
//Создано 14.07.2019

public class AtackArea : Area
{
    [Signal]
    public delegate void Atacked();

    public enum AtackTypes {
        Always,
        Colising
    }
    
    [Export]
    public int Damage = 1;

    [Export]
    public AtackTypes AtackType = AtackTypes.Colising; 

    [Export]
    public float AtackDelay = 1;

    [Export]
    public int[] TagExceptions = new int[0];


    protected AtackListener[] WhiteList;

    protected float AtackDelayWait = 0;

    ///Выделитель хитбокса
    private Spatial Viewer;

    ///Отображать ли выделитель
    public static bool ShowViewer = false;

    ///Хозяин области атаки
    public Spatial AreaOwner;

    public override void _Ready() {
        Viewer = GetNode("Viewer") as Spatial;
    }

    public override void _PhysicsProcess(float delta) {
        Viewer.Visible = ShowViewer;
        
        if (AtackDelayWait > 0)
            AtackDelayWait -= delta;
            
        if (AtackDelayWait < 0)
            AtackDelay = 0;
    }

    public void AreaEntered(Node area) {
        if (area is AtackListener) {
            AtackListener listener = area as AtackListener;
            if (WhiteList != null)
                if (Array.IndexOf(WhiteList, listener) >= 0) return;
            foreach (int tag in TagExceptions)
                if (Array.IndexOf(listener.Tags, tag) >= 0) return;
            listener.GiveDamage(Damage, AreaOwner);
            EmitSignal(nameof(Atacked));
        }
    }


}
