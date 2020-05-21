using Godot;
using System;

public class EntityActionsTests : Spatial
{
    Entity ent {
        get {
            return GetNode("TestingEntity") as Entity;
        }
    }
    private void _on_a1btn_button_up() {
        ent.RunAction(0);
    }

    private void _on_a1btn2_button_up() {
        ent.RunAction(1);
    }
}
