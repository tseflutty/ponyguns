using Godot;
using System;

public class ModalTests2 : Control
{
   protected void _more() {
       ArenaGUI.GetCurrent().GetModalControls(1).ShowModal(ResourceLoader.Load<PackedScene>("res://Tests/ModalTests3.tscn").Instance() as Control, 4);
   }
   
   protected void _hide() {
       QueueFree();
   }
}
