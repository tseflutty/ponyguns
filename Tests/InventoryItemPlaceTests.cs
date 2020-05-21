using Godot;
using System;

public class InventoryItemPlaceTests : Control
{
    
    Inventory old;
    
    public override void _Ready()
    {
        InventoryItemPlace iPlace = GetNode("ViewportContainer/Viewport/InventoryItemPlace") as InventoryItemPlace;
        Inventory inv = new Inventory("Пуська", new int[] {2, 6}, new string[] {"Первый нах", "Я большое"});
        InventoryGUI gui = GetNode("InventoryGUI") as InventoryGUI;
        gui.Slave = inv;
        gui.ShowGUI(false);
        iPlace.Used = inv;
        iPlace.Slot = 1;
        iPlace.Group = 0;
        inv.AddItem(Items.Get(2));
    }

    protected void _on_Button_button_up() {
        InventoryItemPlace iPlace = GetNode("ViewportContainer/Viewport/InventoryItemPlace") as InventoryItemPlace;
        Inventory inv = new Inventory("Пуська", new int[] {2, 6}, new string[] {"Первый нах", "Я большое"});
        InventoryGUI gui = GetNode("InventoryGUI") as InventoryGUI;
        old = gui.Slave;
        gui.Slave = inv;
        gui.ShowGUI(false);
        iPlace.Used = inv;
        iPlace.Slot = 1;
        iPlace.Group = 0;
        inv.AddItem(Items.Get(1));
    }

}
