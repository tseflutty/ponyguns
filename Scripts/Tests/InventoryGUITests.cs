using Godot;
using System;
using System.Collections.Generic;

public class InventoryGUITests : Control
{
    public override void _Ready()
    {
        List<InventorySlotGroup> groups = new List<InventorySlotGroup>();

        groups.Add(new InventorySlotGroup("Storage", 4));
        groups.Add(new InventorySlotGroup("In mouth", 4));
        groups.Add(new InventorySlotGroup("Test3", 1));
         groups.Add(new InventorySlotGroup("Test4", 1));

        Inventory testInventory = new Inventory(groups);

        testInventory.AddItem(new Item("test", "res://icon.png", 0));
        
        (GetNode("InventoryGUI") as InventoryGUI).Slave = testInventory;
        (GetNode("InventoryGUI") as InventoryGUI).ShowGUI(true);
    }

    private void addItemButton_Click() {
        bool result = (GetNode("InventoryGUI") as InventoryGUI).Slave.AddItem(new Item("test", "res://icon.png", 0));
        GD.Print(result ? "[Inventory tests] OK" : "[Inventory tests] NO SPACE");
    }
}
