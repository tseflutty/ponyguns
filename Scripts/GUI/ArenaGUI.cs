using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 29.05.2019

///Интерфейс, который отображается по нахождению игрока на арене
public class ArenaGUI : Control
{  
    ///Текущий GUI арены
    private static ArenaGUI current;

    private Arena CurrentArena {
        get {
            return Arena.GetCurrent();
        }
    }

    protected ProcessBar HealthBar;

    protected InventoryGUI PlayerInventoryGUI;

    protected UsingSlotsGUI PlayerUsingSlotGUI;

    ///Области панелей объектов для каждого из игроков
    protected ObjectPanels[] ObjPanels = new ObjectPanels[4];

    ///Области модального показа элементов интерфейса для каждого из игроков
    protected ModalControls[] MdlControls = new ModalControls[4];

    //Для установки PlayerUsingSlotGUI
    //При первой обработке PhysicsProcess присваивается true
    //В первой обработке PhysicsProcess, когда значение этой переменной ещё false:
    //Усианавоивается инвентарь для PhysicsProcess
    private bool Setuped = false;

    public override void _Ready() {
        //Установка текущего для быстрого доступа из вне
        current = this;

        HealthBar = GetNode("HealthBar") as ProcessBar;
        PlayerInventoryGUI = GetNode("PlayerInventoryGUI") as InventoryGUI;
        PlayerUsingSlotGUI = GetNode("PlayerUsingSlotsGUI") as UsingSlotsGUI;

        //(Временно) Настройка области панелей объектов для первого игрока
        //TODO: Сделать процедурное создание и настройку областей панелей объектов в зависимости от количества игроков
        //(Всё это относится и к ModalControls)
        ObjPanels[0] = GetNode("ObjectPanels") as ObjectPanels;
        MdlControls[0] = GetNode("ModalControls") as ModalControls;
    }

    public override void _Input(InputEvent e) {
        //Показ/скрытие инвентаря по нажатию клавиши инвентаря (По умолчанию I)
        if (e.IsAction("ui_inventory") && e.IsPressed()) {
            if (!PlayerInventoryGUI.Visible) {
                Entity currentPlayer = CurrentArena.GetPlayer(1);
                //Подключение инвентаря игрока, если он не был подключен
                if (PlayerInventoryGUI.Slave == null && currentPlayer != null) {
                    GD.Print("[Arena] Player Invenory: OK");
                    PlayerInventoryGUI.Slave = currentPlayer.EntityInventory;
                }
                PlayerInventoryGUI.ShowGUI(false);
                CurrentArena.ShotPlayerControl.Enable = false;
                CursorController.Sharred.SetCursorByIndex(0);
            } else {
                PlayerInventoryGUI.HideGUI(false);
                CurrentArena.ShotPlayerControl.Enable = true;
                CursorController.Sharred.SetCursorByIndex(1);
            }
        }
    }

    public override void _PhysicsProcess(float delta) {
        if (CurrentArena != null) {
            Arena arena = CurrentArena;

            Entity currentPlayer = arena.GetPlayer(1);
            if (currentPlayer != null) {
                //Обновление счётчика здоровья
                HealthBar.MaxValue = currentPlayer.MaxHealth;
                HealthBar.Value = currentPlayer.Health;
                //Настройка интерфейса используемых слотов
                if (!Setuped && currentPlayer.EntityInventory != null) {
                    PlayerUsingSlotGUI.InventoryOwner = currentPlayer.EntityInventory;
                    Setuped = true;
                    currentPlayer.EntityInventory.UseSlotAtIndex(0);
                    PlayerUsingSlotGUI.PlayerEntityControl = CurrentArena.EntityPlayerControl;
                }
            }
        }
    }

    ///Возвращает область панелей объектов игрока под номером (player). Счёт начинается с 1
    public ObjectPanels GetObjectPanels(byte player) {
        int num = player - 1;
        if (ObjPanels.Length <= num || num < 0) return null;
        return ObjPanels[num];
    }

    ///Возвращает область модального показа элементов интерфейса игрока под номером (player). Счёт начинается с 1
    public ModalControls GetModalControls(byte player) {
        int num = player - 1;
        if (MdlControls.Length <= num || num < 0) return null;
        return MdlControls[num];
    }


    public static ArenaGUI GetCurrent() { return current; }
}
