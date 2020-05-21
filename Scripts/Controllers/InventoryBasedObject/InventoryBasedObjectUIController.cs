using Godot;
using System;

//2019 © Даниил Белов
//Создано 27.08.2019

public class InventoryBasedObjectUIController : Node
{
    
    protected PackedScene PackedItemExchangeGUI;

    public override void _Ready() {
        PackedItemExchangeGUI = ResourceLoader.Load<PackedScene>("res://GUI/Inventory/PinItemExchangeGUI.tscn");

        //Подключение сигналов о создании и удалении интерактивнов объектов
        InteractiveObject.Instances.Connect(nameof(PonyNodeList.ItemAdded), this, nameof(NewInteractiveObject));
        InteractiveObject.Instances.Connect(nameof(PonyNodeList.ItemRemoved), this, nameof(InteractiveObjectRemoved));
        
        //Подключение уже созданных
        foreach (Node obj in InteractiveObject.Instances.GetArr())
            NewInteractiveObject(obj);
    }

    protected void NewInteractiveObject(Node node) {
        if (!(node is InventoryBasedObject)) return;
        InventoryBasedObject obj = node as InventoryBasedObject;

        DebugConsole.Shared.Output("InventoryBasedObject Connected");
        obj.Connect(nameof(InteractiveObject.StartedInteractFrom), this, nameof(EntityStartInteractingWithObject));
        obj.Connect(nameof(InteractiveObject.StopInteractFrom), this, nameof(EntityStopInteractingWithObject));
    }

    protected void InteractiveObjectRemoved(Node node) {
        if (!(node is InventoryBasedObject)) return;
        InventoryBasedObject obj = node as InventoryBasedObject;

        DebugConsole.Shared.Output("InventoryBasedObject Connected");
        obj.Disconnect(nameof(InteractiveObject.StartedInteractFrom), this, nameof(EntityStartInteractingWithObject));
        obj.Disconnect(nameof(InteractiveObject.StopInteractFrom), this, nameof(EntityStopInteractingWithObject));
    }

    protected void EntityStartInteractingWithObject(Entity user, InteractiveObject iobj) {
        GD.Print("[InventoryBasedObjectUIController] START");
        if (!user.IsPlayer) return;

        InventoryBasedObject obj = iobj as InventoryBasedObject;
        PinItemExchangeGUI gui = PackedItemExchangeGUI.Instance() as PinItemExchangeGUI;

        gui.FirstObject = user;
        gui.SecondObject = iobj;

        if (!(user.EntityInventory is AdvancedInventory) || obj.Content == null) {
            gui.QueueFree();
            return;
        }
        gui.Interactor = user.EntityInventory as AdvancedInventory;
        gui.Partner = obj.Content;

        ArenaGUI arenaGUI = ArenaGUI.GetCurrent();
        Arena arena = Arena.GetCurrent();
        if (arenaGUI != null && arena != null && user.IsPlayer) {
            ModalControls mdlControls = arenaGUI.GetModalControls(user.PlayerID);
            if (mdlControls == null) {
                gui.QueueFree();
                obj.StopInteract(user);
                return;
            }
            if (!mdlControls.ShowModal(gui, 0)) {
                gui.QueueFree();
                obj.StopInteract(user);
                return;
            }
        } else {
            gui.QueueFree();
            obj.StopInteract(user);
            return;
        }

        
    }

    protected void EntityStopInteractingWithObject(Entity user, InteractiveObject obj) {
        if (!user.IsPlayer) return;
        
        ArenaGUI arenaGUI = ArenaGUI.GetCurrent();
        Arena arena = Arena.GetCurrent();
        if (arenaGUI != null && arena != null && user.IsPlayer) {
            ModalControls mdlControls = arenaGUI.GetModalControls(user.PlayerID);
            Control curr = mdlControls.GetControlOf(0);
            if (curr is PinItemExchangeGUI) {
                PinItemExchangeGUI gui = curr as PinItemExchangeGUI;
                gui.RemoveWithAnimation();
            }
        }
    }

}
