using Godot;
using System;

public class EntityPosAndTargetTests : Control
{
    public PonyCamera Cam;
    public Entity Ent;

    [Export]
    public NodePath UsingCamera;

    [Export]
    public NodePath TargetEntity;

    public override void _Ready() {
        if (UsingCamera != null) {
            Node cam = GetNode(UsingCamera);
            if (cam is PonyCamera) Cam = cam as PonyCamera;
        }
        if (TargetEntity != null) {
            Node ent = GetNode(TargetEntity);
            if (ent is Entity) Ent = ent as Entity;
        }
    }

    public override void _Draw() {
        if (Cam != null && Ent != null) {
            DrawCircle(Cam.UnprojectPosition(Ent.Translation), 10, new Color(0, 0, 1, 1));
            DrawCircle(Cam.UnprojectPosition(Ent.GetAtackModeTarget()), 15, new Color(1, 0, 0, 1));
            DrawCircle(new Vector2(50, 50), 45, new Color(0,0,0));
            DrawCircle(new Vector2(50, 50), 6, new Color(0,0,1));
            DrawCircle(
                new Vector2(50, 50)
                + new Vector2(Ent.GetAtackModeTarget().x, Ent.GetAtackModeTarget().z) - new Vector2(Ent.Translation.x, Ent.Translation.z),
                 10, new Color(1,0,0)
                );
        }
    }

    public override void _PhysicsProcess(float delta) {
        if (Cam != null && Ent != null) {
            Update();
        }
    }

}
