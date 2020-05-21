using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 29.07.2019

public class InteractiveObjectsUIController : Node
{
    ///Подсказка нажатия кнопки взаимодействия
    protected PackedScene PackedButtonHint;

    // ///Список всех ожидающих интерактивных объектов для каждого игрока
    // protected List<InteractiveObject>[] WaitingInteractiveObjects = new List<InteractiveObject>[4];

    ///Приоритетный ожидающий интерактивный объект для каждого игрока
    protected InteractiveObject[] WaitersOfPlayers = null;
    
    // protected void NewInteractiveObject(InteractiveObject obj) {
    //     DebugConsole.Shared.Output("Interactive Connected");
    //     obj.Connect(nameof(InteractiveObject.StartWaitInteractFrom), this, nameof(StartWaitInteractFrom));
    //     obj.Connect(nameof(InteractiveObject.StopWaitInteractFrom), this, nameof(StopWaitInteractFrom));
    // }

    // protected void InteractiveObjectRemoved(InteractiveObject obj) {
    //     obj.Disconnect(nameof(InteractiveObject.StartWaitInteractFrom), this, nameof(StartWaitInteractFrom));
    //     obj.Disconnect(nameof(InteractiveObject.StopWaitInteractFrom), this, nameof(StopWaitInteractFrom));
    // }

    public override void _Ready() {
        PackedButtonHint = ResourceLoader.Load<PackedScene>("res://GUI/ObjectPanels/ButtonHint.tscn");

        // //Подключение сигналов о создании и удалении интерактивнов объектов
        // InteractiveObject.Instances.Connect(nameof(PonyList<InteractiveObject>.ItemAdded), this, nameof(NewInteractiveObject));
        // InteractiveObject.Instances.Connect(nameof(PonyList<InteractiveObject>.ItemRemoved), this, nameof(InteractiveObjectRemoved));
        
        // //Создание списков с ожидающими
        // for (int i = 0; i < WaitingInteractiveObjects.Length; ++i)
        //     WaitingInteractiveObjects[i] = new List<InteractiveObject>();

        // //Подключение уже созданных
        // foreach (InteractiveObject obj in InteractiveObject.Instances.GetArr())
        //     NewInteractiveObject(obj);
    }

    //Обновить список игроков
    //Подключает сигналы и настраивает список приоритетных объектов
    protected void UpdatePlayers(Entity[] players) {
        WaitersOfPlayers = new InteractiveObject[players.Length];
        for (int i = 0; i < players.Length; ++i) {
            if (players[i] != null) {
                Godot.Collections.Array bindsForPlayerSignal = new Godot.Collections.Array();
                bindsForPlayerSignal.Add((byte)i);
                players[i].Connect(nameof(Entity.WaitingInteractForChanged), this, nameof(PlayerWaiterChanged), bindsForPlayerSignal);
                players[i].Connect("tree_exited", this, nameof(PlayerRemoved), bindsForPlayerSignal);
            }
        }
    }

    public override void _PhysicsProcess(float delta) {
        //Настройка списка и подключения сигналов приоритетных ожидающих при первом обнаружении арены
        if (WaitersOfPlayers == null) {
            Arena arena = Arena.GetCurrent();
            if (arena != null) {
                Entity[] players = arena.GetPlayers();
                UpdatePlayers(players);
            }
        }
    }

    ///Вызывается когда у игрока изменяется приоритетный ожидаемый. Отсчёт номера игрока (player) здесь начинается с 0
    protected void PlayerWaiterChanged(InteractiveObject to, byte player) {
        ArenaGUI gui = ArenaGUI.GetCurrent();
        if (gui == null) return;

        if (WaitersOfPlayers.Length <= player) {
            OS.Alert("PlayerID not found", "Error");
            return;
        }

        byte playerID = player;
        playerID++;

        //Удаление старой подсказки
        if (WaitersOfPlayers[player] != null) {
            InteractiveObject obj = WaitersOfPlayers[player];
            ObjectPanels op = gui.GetObjectPanels(playerID);
            if (op == null) return;
            if (op.GetObjectPanel(obj) is ButtonHint)
                (op.GetObjectPanel(obj) as ButtonHint).Remove();
        }

        //Показ новой
        WaitersOfPlayers[player] = to;

        if (WaitersOfPlayers[player] != null) {
            InteractiveObject obj = WaitersOfPlayers[player];
            ObjectPanels op = gui.GetObjectPanels(playerID);
            if (op == null) return;
            ButtonHint hint = PackedButtonHint.Instance() as ButtonHint;
            hint.ButtonName = "F";
            op.ShowObjectPanel(obj, new Vector2(0, -obj.Heigth), hint);
        }
    }

    //Вызывается, когда игрок был удалён с арены
    protected void PlayerRemoved(int player) {
        if (WaitersOfPlayers.Length > player)
            WaitersOfPlayers[player] = null;
    }

    // protected void StartWaitInteractFrom(Entity e, InteractiveObject obj) {
    //     ArenaGUI gui = ArenaGUI.GetCurrent();
    //     if (gui == null) return;
    //     //Вывод подсказки для игрока
    //     if (e.IsPlayer) {
    //         byte PlayerID = e.PlayerID;

    //         if (WaitingInteractiveObjects.Length <= PlayerID) {
    //             OS.Alert("PlayerID not found", "Error");
    //             return;
    //         }

    //         WaitingInteractiveObjects[PlayerID].Add(obj);
    //         if (WaitingInteractiveObjects[PlayerID].Count == 1) {
    //             ObjectPanels op = gui.GetObjectPanels(e.PlayerID);
    //             if (op == null) return;
    //             ButtonHint hint = PackedButtonHint.Instance() as ButtonHint;
    //             hint.ButtonName = "F";
    //             op.ShowObjectPanel(obj, new Vector2(0, -obj.Heigth), hint);
    //         }
    //     }
    // }

    // protected void StopWaitInteractFrom(Entity e, InteractiveObject obj) {
    //     ArenaGUI gui = ArenaGUI.GetCurrent();
    //     if (gui == null) return;
    //     //Скрытие подсказки для игрока
    //     if (e.IsPlayer) {
    //         byte PlayerID = e.PlayerID;

    //         if (WaitingInteractiveObjects.Length <= PlayerID) {
    //             OS.Alert("PlayerID not found", "Error");
    //             return;
    //         }

    //         int objIndex = WaitingInteractiveObjects[PlayerID].IndexOf(obj);

    //         ///Удаление подсказки, если отключаемый объект первый
    //         if (objIndex == 0) {
    //             ObjectPanels op = gui.GetObjectPanels(e.PlayerID);
    //             if (op == null || !(op.GetObjectPanel(obj) is ButtonHint)) return;
    //             (op.GetObjectPanel(obj) as ButtonHint).Remove();
    //         }


    //         //Показ подсказки первого объекта в списке, если он есть
    //         WaitingInteractiveObjects[PlayerID].Remove(obj);

    //         if (WaitingInteractiveObjects[PlayerID].Count > 0) {
    //             InteractiveObject fObj = WaitingInteractiveObjects[PlayerID][0];
    //             ObjectPanels op = gui.GetObjectPanels(e.PlayerID);
    //             if (op == null) return;
    //             ButtonHint hint = PackedButtonHint.Instance() as ButtonHint;
    //             hint.ButtonName = "F";
    //             op.ShowObjectPanel(fObj, new Vector2(0, -obj.Heigth), hint);
    //         }
            
    //     }
    // }

}
