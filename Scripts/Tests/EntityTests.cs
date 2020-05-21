using Godot;
using System;

public class EntityTests : Node
{

    Label MobInfoLabel;
    Entity OurEntity;

    PackedScene TestBullet;
    
    Arena TestArena;

    ShotUserControl shc;

    public override void _Ready() {
        MobInfoLabel = GetNode("GUI/MobInfoLabel") as Label;
        OurEntity = GetNode("Scene/Entity2_") as Entity;
        TestArena = GetNode("Arena") as Arena;

        TestBullet = ResourceLoader.Load("res://Objects/Weapons/Bullets/Bullet.tscn") as PackedScene;
        OurEntity.CurrentBullet = ResourceLoader.Load("res://Objects/Weapons/Bullets/Bullet.tscn") as PackedScene;
        (GetNode("Scene/Entity2_") as Entity).CurrentBullet = ResourceLoader.Load("res://Objects/Weapons/Bullets/Bullet.tscn") as PackedScene;

        shc = GetNode("GUI/ShotUserControl") as ShotUserControl;
        shc.CurrentCamera = GetNode("Scene/FollowingCamera") as PonyCamera;
        shc.Slave = GetNode("Scene/Entity2_") as Entity;

        (GetNode("Scene/FollowingCamera") as PonyCamera).Pursued = GetNode("Scene/Entity2_") as Entity;

        (GetNode("GUI/InventoryGUI") as InventoryGUI).Slave = (GetNode("Scene/Entity2_") as Entity).EntityInventory;

        (GetNode("Scene/Entity2_") as Entity).EntityInventory.AddItem(new Gun("test", "res://Tests/TestGun.png", 0, "res://Tests/TestBullet.tscn"));
        (GetNode("Scene/Entity2_") as Entity).EntityInventory.AddItem(new Item("test", "res://Tests/testitem.png", 0));
    }
    
    public override void _PhysicsProcess(float delta) {
        OS.SetWindowTitle("FPS: " + Engine.GetFramesPerSecond().ToString());

        if (Input.IsActionPressed("ui_up")) {
            OurEntity.ChangeFlyHeight(1);
        }
        if (Input.IsActionPressed("ui_down")) {
            OurEntity.ChangeFlyHeight(-1);
        }

        // MobInfoLabel.Text = "Health = " + OurEntity.Health + " CanFly = " + OurEntity.CanFly +
        //                     " OnFloor = " + OurEntity.IsOnFloor() + " MovementDirection = " + OurEntity.GetMovementDirection() +
        //                     " Current Usable Item  = " + OurEntity.CurrentUsableItemGroup + ", " + OurEntity.CurrentUsableItemSlot;
    }

    public void JumpButton_Click() {
        OurEntity.Jump();
    }

    public void StartMovementButton_Click() {
        TextEdit MovXInput = GetNode("GUI/MovXInput") as TextEdit;
        TextEdit MovYInput = GetNode("GUI/MovYInput") as TextEdit;
        float x, y;

        if (float.TryParse(MovXInput.Text, out x) && float.TryParse(MovYInput.Text, out y))
            OurEntity.StartMovement(new Vector2(x, y));
    }

    public void OnOffFlightButton_Click() {
        OurEntity.IsFly = (OurEntity.IsFly) ? false : true;
    }

    public void StopMovementButton_Click() {
        OurEntity.StopMovement();
    }

    public void TestShot_Click() {
        //TestArena.SpawnShot(TestBullet.Instance() as Bullet, new Vector3(1, 0, 1), new Vector3(0, 2, 0));
    }

    public void TestEntityShot_Click() {
        //OurEntity.ShotOffset = new Vector3(0, -1, 0);
        OurEntity.Shot(new Vector3(1, 0, 1));
    }
}