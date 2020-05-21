using Godot;
using System;

public class TestVisualInventoryObject  : VisualInventoryObject
{
    
    public override void _Ready()
    {
        base._Ready();

        Content = new AdvancedInventory("Какой-то стол", new int[] {3, 1}, new string[] {"На столе", "На полу"});

        Content = new AdvancedInventory("Какая-то хуйня", new int[] {3, 8}, new string[] {"Стол", "Какая-то хуйня"});

        InventoryItemPlace[][] places = new InventoryItemPlace[][] {

            new InventoryItemPlace[] {
                GetNode("Top1") as InventoryItemPlace,
                null,
                GetNode("Top3") as InventoryItemPlace,
            },

            new InventoryItemPlace[] {
                GetNode("Floor1") as InventoryItemPlace,
            }
        
        };

        SetItemPlaces(places);
    }

    protected void _on_TestVisualInventoryObject__StopInteractFromTEST(Entity _, InteractiveObject __) {
        var a = new AdvancedInventory("Какой-то стол", new int[] {3, 1}, new string[] {"На столе", "На полу"});
        Content = a;
        // GD.Print(a);
        
    }

}
