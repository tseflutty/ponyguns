using Godot;
using System;

public class AdvancedInventoryRequestsDebugger : Label
{
    public AdvancedInventory inventory;

    public override void _PhysicsProcess(float delta) {
        if (inventory == null) return;

        string cont = "";

        foreach (AdvancedInventory.ExchangeRequest req in inventory.GetExchangeRequests()) {
            cont += "Request from "+req.From.InventoryName+":\n";
            cont += "  Item: "+req.ItemOfRequest.ItemName+"\n";
            cont += "  Type: "+req.Type.ToString()+"\n";
            cont += "  CancelWait: "+req.CancelWait+"\n";
            cont += " \n";
        }

        Text = cont;
    }
}
