using Godot;
using System;

public class DebugConsole : CanvasLayer
{
    public override void _Input(InputEvent e) {
        if (e.IsAction("debugconsole") && !e.IsPressed()) {
            Control n = GetNode("Content") as Control;
            n.Visible = !n.Visible;
        }
    }

    public static DebugConsole Shared;

    public override void _Ready() {
        Shared = this;
    }

    public void Output(string msg) {
        (GetNode("Content/output") as Label).Text = " > " + msg + "\n" + (GetNode("Content/output") as Label).Text;
        GD.Print(msg);
    }

    public void SendBtn_Click() {
        string command = (GetNode("Content/LineEdit") as LineEdit).Text;
        string[] lexemes = command.Split(' ');
        switch (lexemes[0]) {
            case "goto":
                if (lexemes.Length > 1) {
                    PonyLoader.ChangeScene(lexemes[1], GetTree());
                    Output("Success!");
                } else {
                    Output("Use: goto <path to scene>");
                }
                break;
            case "home":
                PonyLoader.ChangeScene("res://Tests/TestMenu.tscn", GetTree());
                Output("Welcome to home!");
                break;
            case "spawn":
                if (lexemes.Length > 1) {
                    if (Arena.GetCurrent() != null) {
                        switch (lexemes[1]) {
                            case "slime":
                                Entity slime = ResourceLoader.Load<PackedScene>("res://Objects/Entity/Slime/Slime.tscn").Instance() as Entity;
                                slime.Translation = new Vector3(0, 4, 0);
                                Arena.GetCurrent().AddChild(slime);
                                Output("Slime spawned");
                                break;
                        }
                    } else {
                        Output("Arena not found");
                    }
                } else {
                    Output("Use: spawn <mob name>");
                }
                break;
            default:
                Output("Unlnown Command");
                break;
        }
    }

    private void ShowHitboxes_Click() {
        AtackArea.ShowViewer = !AtackArea.ShowViewer;
        AtackListener.ShowViewer = !AtackListener.ShowViewer;
    }
}
