using Godot;
using System;

public class TestMenu : Control
{
    private void TestArenaButton_Click() {
        PonyLoader.ChangeScene("res://Tests/TestArena.tscn", GetTree());
    }

    private void EntityTestsButton_Click() {
        PonyLoader.ChangeScene("res://Tests/EntityTests.tscn", GetTree());
    }

    private void _on_LabelTestsButton2_button_up() {
        PonyLoader.ChangeScene("res://Tests/PonyLabelTests.tscn", GetTree());
    }
}
