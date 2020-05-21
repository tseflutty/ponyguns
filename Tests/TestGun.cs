using Godot;
using System;

public class TestGun : Gun
{
    

    public TestGun() : base("Пушка-какашка", "res://Tests/TestGun.png", 2, "res://Tests/TestBullet.tscn", 0.1f, new int[2] {(int)ItemTag.HasGrip, (int)ItemTag.Small}) {
        PathToVisualItem = "res://Tests/VisualTestGun.tscn";
    }

}
