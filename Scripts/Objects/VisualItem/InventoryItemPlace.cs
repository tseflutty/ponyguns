using Godot;
using System;

///ItemPlace c привязкой к слоту в инвентаре
public class InventoryItemPlace : ItemPlace
{
    [Signal]
    public delegate void ItemUpdated();
    
    protected Inventory _used;
    ///Инвентарь, который использовать для привязки к слоту
    public Inventory Used {
        get { return _used; }
        set {
            if (_used != null) {
                _used.Disconnect(nameof(Inventory.ItemAdded), this, nameof(_Inventory_ItemAdded));
                _used.Disconnect(nameof(Inventory.ItemRemoved), this, nameof(_Inventory_ItemRemoved));
            }
            _used = value;
            if (_used != null) {
                _used.Connect(nameof(Inventory.ItemAdded), this, nameof(_Inventory_ItemAdded));
                _used.Connect(nameof(Inventory.ItemRemoved), this, nameof(_Inventory_ItemRemoved));
            }
            UpdateItem();
        }
    }

    protected int _group = 0;
    [Export]
    ///Номер группы привязанного слота
    public int Group {
        get { return _group; }
        set {
            _group = value;
            UpdateItem();
        }
    }

    protected int _slot = 0;
    [Export]
    ///Номер привязанного слот
    public int Slot {
        get { return _slot; }
        set {
            _slot = value;
            UpdateItem();
        }
    }

    private void _Inventory_ItemAdded(int group, int slot) {
        if (slot == Slot && group == Group) {
            UpdateItem();
        }
    }

    private void _Inventory_ItemRemoved(int group, int slot) {
        if (slot == Slot && group == Group) {
            UpdateItem();
        }
    }

    protected void UpdateItem() {
        if (Used == null) {
            ItemOfPlace = null;
            return;
        }
        if (Used.GetGroupsCount() > Group && Used.GetSlotCountOfGroup(Group) > Slot) {
            Item item = Used.GetItemAt(Group, Slot);
            if (item == null) {
                ItemOfPlace = null;
                return;
            }

            VisualItem newVisual = ResourceLoader.Load<PackedScene>(item.PathToVisualItem).Instance() as VisualItem;
            ItemOfPlace = newVisual;
        } else {
            ItemOfPlace = null;
        }
        EmitSignal(nameof(ItemUpdated));
    }

}
