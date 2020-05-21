using Godot;
using System;

//2019 © Даниил Белов
//Создано 26.04.2019

public class EntityUserControl : Node
{
    [Signal]
    public delegate void ItemSwapPress();
    
    [Export]
    ///<summary>
    ///<para>Номер игрока (от 1), которым будет управляться сущность</para>
    ///</summary>
    public short PlayerID = 1;

    ///<summary>
    ///<para>Сущность, которой управляет игрок</para>
    ///</summary>
    public Entity SlaveEntity = null;

    //Отсчёт милисекунд после предыдущего нажатия Accept в обратном порядке. Для выслеживания двойного нажатия
    private float AcceptWait = 0;

    ///Индекс слота, который был недавно перемещён в главный слот с помощью tab
    public int recentMovedSlotIndex = 0;


    NodePath _slaveLink;
    [Export]
    public NodePath Slave {
        set {
            Node n = GetNode(value);
            GD.Print("[EntityUserControl] ", value, " ", n);
            if (n is Entity) SlaveEntity = n as Entity;
            _slaveLink = value;
        }
        get {
            return _slaveLink;
        }
    }

    public override void _Ready() {
        if (_slaveLink != null)
            Slave = _slaveLink;
    }


    public override void _PhysicsProcess(float delta) {
        if (SlaveEntity != null) {
            Vector2 movementDirection = new Vector2(0, 0);
            bool isMoving = false;
            if (Input.IsActionPressed("ui_up")) {
                movementDirection.y = -1;
                isMoving = true;
            }
            if (Input.IsActionPressed("ui_down")) {
                movementDirection.y = 1;
                isMoving = true;
            }

            if (Input.IsActionPressed("ui_right")) {
                movementDirection.x = 1;
                isMoving = true;
            }
            if (Input.IsActionPressed("ui_left")) {
                movementDirection.x = -1;
                isMoving = true;
            }

            if (isMoving) {
                SlaveEntity.StartMovement(movementDirection);
            } else {
                SlaveEntity.StopMovement();
            }

            if (Input.IsActionPressed("ui_accept")) {
                //GD.Print("[EntityUserControl] Jump");
                if (!SlaveEntity.IsFly)
                    SlaveEntity.Jump();
                else {
                    SlaveEntity.ChangeFlyHeight(1); //Набирание высоты в полёте
                }
            }

            if (Input.IsActionPressed("ui_cancel")) {
                if (SlaveEntity.IsFly)
                    SlaveEntity.ChangeFlyHeight(-1); //Сбрасывание высоты в полёте
                
                //Отмена взаимодействия
                if (SlaveEntity.IsInteracting)
                    SlaveEntity.StopInteract();
            }

            if (Input.IsActionJustPressed("ui_accept")) {
                //Активация полёта по двойному нажатию Accept
                if (SlaveEntity.CanFly) {
                    if (AcceptWait <= 0)
                        AcceptWait = 0.2f;
                    else {
                        SlaveEntity.IsFly = (SlaveEntity.IsFly) ? false : true;
                        AcceptWait = 0;
                    }
                }
            }

            //Взаимодействие
            if (Input.IsActionJustPressed("ui_interact"))
                SlaveEntity.Interact();


            //По умолчанию tab
            //Для переключения используемых слотов
            if (Input.IsActionJustPressed("ui_focus_next"))
                if (SlaveEntity.EntityInventory != null && SlaveEntity.EntityInventory.UseMainSlot) {
                    recentMovedSlotIndex++;
                    Inventory inv = SlaveEntity.EntityInventory;
                    int mainSlotIndex = inv.GetSlotIndexOf(inv.MainSlotGroup, inv.MainSlot);
                    if (recentMovedSlotIndex >= inv.GetSlotsCount())
                        recentMovedSlotIndex = 0;
                    if (recentMovedSlotIndex == mainSlotIndex)
                        recentMovedSlotIndex++;
                    GD.Print("[EntityUserControl] Move item from ", recentMovedSlotIndex, " to main slot ", mainSlotIndex, "; slots count: ", inv.GetSlotsCount());

                    inv.MoveItemAtIndexToIndex(recentMovedSlotIndex, mainSlotIndex);

                    EmitSignal(nameof(ItemSwapPress));
                }


            //Отсчёт времени между нажатий
            if (AcceptWait < 0) {
                AcceptWait = 0;
            } else if (AcceptWait > 0) {
                AcceptWait -= delta;
            }
        }
    }
}
