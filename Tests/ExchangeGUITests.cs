using Godot;
using System;

public class ExchangeGUITests : Control
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AdvancedInventory inventory1 = 
            new AdvancedInventory("Inventory 1", new int[3] {2, 2, 2}, new string[3] {"Group 1", "Group 2", "Group 3"});
        AdvancedInventory inventory2 = 
            new AdvancedInventory("Inventory 2", new int[3] {1, 1, 2}, new string[3] {"Group 1", "Group 2", "Group 3"});


        PinItemExchangeGUI gui = GetNode("PinItemExchangeGUI") as PinItemExchangeGUI;

        inventory2.GiveAccessToExchange(inventory1);

        gui.Interactor = inventory1;
        gui.Partner = inventory2;

        inventory1.AddItem(Items.Get(1));

        
    }

    public override void _Input(InputEvent e) {
        if (e.IsAction("ui_cancel")) {
            GD.Print("canc");
            PinItemExchangeGUI gui = GetNode("PinItemExchangeGUI") as PinItemExchangeGUI;
            gui.RemoveWithAnimation();
        }
    }
}
