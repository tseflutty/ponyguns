using Godot;
using System;


public class TestArena : Arena
{
    
    public override void _Ready() {
        base._Ready();
        // //Проверка панелей объектов
        // Label l = new Label();
        // l.Text = "TseFlutty";
        // ArenaGUI.GetCurrent().GetObjectPanels(1).ShowObjectPanel(GetPlayer(1), new Vector2(-30, -110), l);

        // Label l2 = new Label();
        // l2.Text = "SomeSlime";
        // ArenaGUI.GetCurrent().GetObjectPanels(1).ShowObjectPanel(GetNode("Slime2") as Spatial, new Vector2(-10, -70), l2);
        // ArenaGUI.GetCurrent().GetModalControls(1).ShowModal(ResourceLoader.Load<PackedScene>("res://Tests/ModalTests2.tscn").Instance() as Control, 1);
    }

}
