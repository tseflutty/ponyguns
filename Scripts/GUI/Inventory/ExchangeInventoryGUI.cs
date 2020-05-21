using Godot;
using System;

//2019 © Даниил Белов
//Создано 23.08.2019

///Интерфейс инвентаря для обмена
///!!! Slave должен быть типа AdvancedInventory !!!
///!!! Перед использованием вызвать Init(...) !!!
public class ExchangeInventoryGUI : InventoryGUI
{

    [Signal]
    public delegate void OrderedToFront();

    protected AdvancedInventory _slavePartner;
    ///Партнёр Slave
    ///Если указан парнёр, будет использоваться он вместо Slave
    ///В данном случае SlavePartner будет выступать как тот, с кем обменивается Slave
    ///Требуется разрешение на взаимодействие от SlavePartner для Slave
    public AdvancedInventory SlavePartner {
        get {
            return _slavePartner;
        }
        set {
            if (value == null) {
                _slavePartner.Disconnect(nameof(Inventory.ItemRemoved), this, nameof(ItemRemoved));
                _slavePartner.Disconnect(nameof(Inventory.ItemAdded), this, nameof(ItemAdded));
            }

            _slavePartner = value;

            if (SlavePartner != null) {
                if (Slave != null) {
                    Slave.Disconnect(nameof(Inventory.ItemRemoved), this, nameof(ItemRemoved));
                    Slave.Disconnect(nameof(Inventory.ItemAdded), this, nameof(ItemAdded));
                }
                _slavePartner.Connect(nameof(Inventory.ItemRemoved), this, nameof(ItemRemoved));
                _slavePartner.Connect(nameof(Inventory.ItemAdded), this, nameof(ItemAdded));
                UpdateAllGUISlots();
            } else {
                Slave.Connect(nameof(Inventory.ItemRemoved), this, nameof(ItemRemoved));
                Slave.Connect(nameof(Inventory.ItemAdded), this, nameof(ItemAdded));
            }

            UpdateAllGUISlots();
        }
    }

    public override Inventory Slave {
        get { return _slave; }
        set {
            _slave = value;
            GD.Print("Connect");
            if (SlavePartner == null && value != null) {
                value.Connect(nameof(Inventory.ItemRemoved), this, nameof(ItemRemoved));
                value.Connect(nameof(Inventory.ItemAdded), this, nameof(ItemAdded));
                UpdateAllGUISlots();
            }
        }
    }

    public override Inventory _InventoryToUse {
        get { 
            // GD.Print("NEW!");
            return (SlavePartner == null) ? Slave : SlavePartner;
        }
    }

    ///Подключенные GUI интерфейсов для взаимодействия обмена
    protected ExchangeInventoryGUI[] ConnectedGUIs;

    ///От кого режим перемещения
    ///От этой переменной будет зависить вызываемый метод при перемещении предмета. Либо base.AddingListened для стандартного перемещения,
    ///в случае если ItemMoveModeFrom равняется этому интерфейсу и не указан SlavePartner, либо ExchangeMoveItem для обмена пердметом или перемещения
    ///внутри партнёра основным взаимодействующим (Slave)
    protected ExchangeInventoryGUI ItemMoveModeFrom;

    ///Когда происходит свап, сюда записывается тот, от кого обмен.
    ///В следующей обработке PhysicsProcess этот GUI перенесёься на передний план и переменной
    ///присвоиться null
    protected ExchangeInventoryGUI _swapFrom = null;

    ///Сихроинизовать режим перемещения предмета с подключенными GUI (ConnectedGUIs)
    protected void SynchronizeMoveMode() {
        if (ConnectedGUIs == null) return;

        foreach (ExchangeInventoryGUI gui in ConnectedGUIs) {
            gui.isItemMoveMode = isItemMoveMode;
            gui.MovableItem = MovableItem;
            gui.MovableFromGroup = MovableFromGroup;
            gui.MovableFromSlot = MovableFromSlot;
            gui.ItemMoveModeFrom = this;
        }
    }

    public override void _ItemStartMove(int group, int slot) {
        if (Slave == null) {
            OS.Alert("Exchanger not found", "InventoryGUI Error");
        }
        if (SlavePartner != null) {
            if (!SlavePartner.HasAccesToExchange(Slave)) return;
        }
        if (isItemMoveMode) return;
        base._ItemStartMove(group, slot);
        Node parent = GetParent();
        if (parent != null) {
            //Перемещение на передний план
            parent.MoveChild(this, parent.GetChildCount()-1);
            EmitSignal(nameof(OrderedToFront));
        }
        ItemMoveModeFrom = this;
        MoveToFront();
        SynchronizeMoveMode();
    }

    protected override void _AddingListened(int group, int slot) {
        GD.Print("[ExchangeInventoryGUI] Adding Listened");
        if (!(Slave is AdvancedInventory)) {
            OS.Alert("Exchanger isn't AdvancedInventory!", "InventoryGUI Error");
            return;
        }

        if (ItemMoveModeFrom == null) {
            OS.Alert("Item owner is not found", "InventoryGUI Error");
            return;
        }

        if (_InventoryToUse == null) {
            OS.Alert("GUI's inventory is not found", "InventoryGUI Error");
            return;
        }

        AdvancedInventory exchanger = Slave as AdvancedInventory;

        if (!isItemMoveMode) return;

        if (ItemMoveModeFrom == this && _InventoryToUse == Slave) {
            GD.Print("->)> 1");
            base._AddingListened(group, slot);
        } else {
            //Кладётся ли предмет обратно в тот же слот
            bool isPutBack = (group == MovableFromGroup && slot == MovableFromSlot && ItemMoveModeFrom._InventoryToUse == _InventoryToUse);

            //Произойдёт ли свап
            bool isSwap = _InventoryToUse.GetItemAt(group, slot) != null && !isPutBack;

            if (exchanger.ExchangeMoveItem(ItemMoveModeFrom._InventoryToUse, MovableFromGroup, MovableFromSlot, _InventoryToUse, group, slot) || isPutBack) {
                isItemMoveMode = false;
                ItemMoveModeFrom.MovableItemTexture.Hide();
                OneBlockContext = true;
                if (ItemMoveModeFrom != this)
                    ItemMoveModeFrom.OneBlockContext = false;
                if (isPutBack)
                    GUISlots[group][slot].ItemOfSlot = _InventoryToUse.GetItemAt(group, slot);
                if (isSwap) {
                    SynchronizeMoveMode();
                    ItemMoveModeFrom._ItemStartMove(MovableFromGroup, MovableFromSlot);
                    _swapFrom = ItemMoveModeFrom;
                    return;
                }
            } else {
                GD.Print("[ExchangeInventoryGUI] Not acces to Exchange or some Inventory is not AdvancedInventory");
                return;
            }
            GD.Print("->)> 2");
        }

        SynchronizeMoveMode();
    }

    ///!!! Использовать только один раз перед использованием объекта !!!
    public void Init(ExchangeInventoryGUI[] connectedGUIs) {
        ConnectedGUIs = connectedGUIs;
    }

    protected override void SlotClicked(int group, int slot) {
        base.SlotClicked(group, slot);
        MoveToFront();
    }

    protected void MoveToFront() {
        Node parent = GetParent();
        if (parent != null) {
            //Перемещение на передний план
            parent.MoveChild(this, parent.GetChildCount()-1);
            EmitSignal(nameof(OrderedToFront));
        }
    }

    public override void _PhysicsProcess(float delta) {
        base._PhysicsProcess(delta);
        if (_swapFrom != null) {
            _swapFrom.MoveToFront();
            _swapFrom = null;
        }
    }


}
