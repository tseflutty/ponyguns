using Godot;
using System;

//2019 © Даниил Белов
//Создано 02.08.2019

public class ChestUIController : Node
{
    
    protected PackedScene PackedChestGUI;

    public override void _Ready() {
        PackedChestGUI = ResourceLoader.Load<PackedScene>("res://GUI/Chest/ChestGUI.tscn");

        //Подключение сигналов о создании и удалении интерактивнов объектов
        InteractiveObject.Instances.Connect(nameof(PonyNodeList.ItemAdded), this, nameof(NewInteractiveObject));
        InteractiveObject.Instances.Connect(nameof(PonyNodeList.ItemRemoved), this, nameof(InteractiveObjectRemoved));
        
        //Подключение уже созданных
        foreach (Node obj in InteractiveObject.Instances.GetArr())
            NewInteractiveObject(obj);
    }

    protected void NewInteractiveObject(Node node) {
        if (!(node is InteractiveObject)) return;
        InteractiveObject obj = node as InteractiveObject;

        DebugConsole.Shared.Output("Interactive Connected");
        if (obj is Chest) {
            obj.Connect(nameof(InteractiveObject.StartInteractFrom), this, nameof(EntityStartInteractingWithChest));
            obj.Connect(nameof(InteractiveObject.StopInteractFrom), this, nameof(EntityStopInteractingWithChest));
            obj.Connect(nameof(InteractiveObject.InteractMessageSended), this, nameof(ChestSendInteractMessage));
        }
    }

    protected void InteractiveObjectRemoved(Node node) {
        if (!(node is InteractiveObject)) return;
        InteractiveObject obj = node as InteractiveObject;

        if (obj is Chest) {
            obj.Disconnect(nameof(InteractiveObject.StartInteractFrom), this, nameof(EntityStartInteractingWithChest));
            obj.Disconnect(nameof(InteractiveObject.StopInteractFrom), this, nameof(EntityStopInteractingWithChest));
            obj.Disconnect(nameof(InteractiveObject.InteractMessageSended), this, nameof(ChestSendInteractMessage));
        }
    }

    protected void ChestSendInteractMessage(InteractMessage msg, InteractiveObject obj, Entity user) {
        if (user.InteractingWith != obj) return;
        Chest chest = obj as Chest;
        if (msg == InteractMessage.ReadyToUse) {
            ArenaGUI arenaGUI = ArenaGUI.GetCurrent();
            Arena arena = Arena.GetCurrent();
            if (arenaGUI != null && arena != null && user.IsPlayer) {
                ModalControls mdlControls = arenaGUI.GetModalControls(user.PlayerID);
                if (mdlControls != null) {
                    ChestGUI gui = PackedChestGUI.Instance() as ChestGUI;
                    if (mdlControls.ShowModal(gui, 0))
                        gui.Setup(chest, user);
                    else
                        user.StopInteract();
                } else {
                    OS.Alert("Player not found", "Error");
                }
            }
        }
    }

    protected void EntityStartInteractingWithChest(Entity user, InteractiveObject obj) {
        Chest chest = obj as Chest;
        ArenaGUI arenaGUI = ArenaGUI.GetCurrent();
        Arena arena = Arena.GetCurrent();
        if (arenaGUI != null && arena != null && user.IsPlayer) {
            //ВРЕМЕННО Использование основной камеры
            arena.MainCamera.Pursued = chest;
        }
    }

    protected void EntityStopInteractingWithChest(Entity user, InteractiveObject obj) {
        Chest chest = obj as Chest;
        ArenaGUI arenaGUI = ArenaGUI.GetCurrent();
        Arena arena = Arena.GetCurrent();
        if (arenaGUI != null && arena != null && user.IsPlayer) {
            //ВРЕМЕННО Использование основной камеры
            arena.MainCamera.Pursued = user;
        }
    }

    



}
