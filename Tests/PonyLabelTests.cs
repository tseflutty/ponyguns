using Godot;
using System;

public class PonyLabelTests : Control
{
    private void settextBtn_Click() {
        PonyLabel label = GetNode("PonyLabel") as PonyLabel;
        LineEdit edit = GetNode("LineEdit") as LineEdit;
        label.Text = edit.Text;
        label.ShowText(true);
    }


    private void _on_settextBtn2_button_up() {
        PonyLabel label = GetNode("PonyLabel") as PonyLabel;
        LineEdit edit = GetNode("LineEdit") as LineEdit;
        label.Text = edit.Text;
        label.ShowText(false);
    }
}
