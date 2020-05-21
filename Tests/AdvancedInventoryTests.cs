using Godot;
using System;

public class AdvancedInventoryTests : Control
{

    public AdvancedInventory inventory1 = new AdvancedInventory("Инвентарь 1", new int[2] {3, 5}, new string[2] {"Группа 1", "Группа 2"});
    public AdvancedInventory inventory2 = new AdvancedInventory("Инвентарь 2", new int[2] {4, 2}, new string[2] {"Группа 1", "Группа 2"});
    
    public ExchangeInventoryGUI gui1;
    public ExchangeInventoryGUI gui2;

    public Button ButtonAcces;
    public Button ButtonFirstTO1;
    public Button ButtonFirstTO2;
    public Button ButtonFirstFROM1;
    public Button ButtonFirstFROM2;

    public override void _Ready()
    {
        gui1 = GetNode("ExchangeInventoryGUI1") as ExchangeInventoryGUI;
        gui2 = GetNode("ExchangeInventoryGUI2") as ExchangeInventoryGUI;

        gui1.Slave = inventory1;                        gui2.Slave = inventory1;
                                                        gui2.SlavePartner = inventory2;
        gui1.Init(new ExchangeInventoryGUI[1] {gui2});  gui2.Init(new ExchangeInventoryGUI[1] {gui1});
        gui1.ShowGUI(false);                            gui2.ShowGUI(false);

        ButtonAcces = GetNode("ButtonAcces") as Button;

        ButtonFirstTO1 = GetNode("ButtonFirstTO1") as Button;
        ButtonFirstTO2 = GetNode("ButtonFirstTO2") as Button;
        ButtonFirstFROM1 = GetNode("ButtonFirstFROM1") as Button;
        ButtonFirstFROM2 = GetNode("ButtonFirstFROM2") as Button;

        (GetNode("AdvRD") as AdvancedInventoryRequestsDebugger).inventory = inventory2;

        AddChild(inventory1);  AddChild(inventory2);
    }

    protected void _on_ButtonAcces_button_up() {
        inventory2.GiveAccessToExchange(inventory1);
        GD.Print("OK");
    }

    protected void _on_ButtonAcces2_button_up() {
        inventory2.CancelAccessToExchange(inventory1);
        GD.Print("OK");
    }

    protected void _on_ButtonFirstTO1_button_up() {
        inventory1.ExchangeGiveItem(inventory2, 0, 0);
    }
    protected void _on_ButtonFirstTO2_button_up() {
        inventory1.ExchangeGiveItem(inventory2, 0, 0, 0, 0);
    }

    protected void _on_ButtonFirstFROM1_button_up() {
        inventory1.ExchangeGetItem(inventory2, 0, 0);
    }
    protected void _on_ButtonFirstFROM2_button_up() {
        inventory1.ExchangeGetItem(inventory2, 0, 0, 0, 0);
    }


    protected void _on_ButtonGive_button_up() {
        inventory1.AddItem(Items.Get(1));
        inventory1.AddItem(Items.Get(2));
        inventory2.AddItem(Items.Get(3));
    }


    protected void _on_ButtonRGet1_button_up() {
        GD.Print("--1");
        inventory1.SendExchangeRequest(AdvancedInventory.ExchangeRequestType.Get, 0, 0, inventory2);
        GD.Print("-> > ", inventory2.GetExchangeRequests().Length);
    }

    protected void _on_ButtonRGive_button_up() {
        GD.Print("--1");
        inventory1.SendExchangeRequest(AdvancedInventory.ExchangeRequestType.Give, 0, 1, inventory2);
        GD.Print("-> > ", inventory2.GetExchangeRequests().Length);
    }


    protected void _on_ButtonRGet2_button_up() {
        GD.Print("--2");
        GD.Print("-> > ", inventory2.GetExchangeRequests().Length);
        GD.Print(inventory2.AcceptExchangeRequest(0));
        GD.Print("-> > ", inventory2.GetExchangeRequests().Length);
    }

    protected void _on_remove1_button_up() {
        inventory1.RemoveItemByUID(1);
    }

}
