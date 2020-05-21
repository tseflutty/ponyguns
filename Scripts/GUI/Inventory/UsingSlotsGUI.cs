using Godot;
using System;

//2019 © Даниил Белов
//Создано 31.05.2019


///Графический интерфейс, предназначеный для отображения используемого предмета игрока
public class UsingSlotsGUI : Control
{
    //Текстуры предметов в интерфейсе по проядку: следующий, текущий, предыдущий
    TextureRect[] ItemTextures = new TextureRect[3];

    AnimationPlayer Animations;

    int nextIndex = 1;

    private Inventory _inventoryOwner;
    ///Инвентарь у которого берёться информаци о предметах
    public Inventory InventoryOwner {
        get { return _inventoryOwner; }
        set {
            _inventoryOwner = value;
            //_inventoryOwner.Connect(nameof(Inventory.ItemsSwapped), this, nameof(Inventory_ItemsMovedFrom));
            _inventoryOwner.Connect(nameof(Inventory.ItemRemoved), this, nameof(ItemUpdated));
            _inventoryOwner.Connect(nameof(Inventory.ItemAdded), this, nameof(ItemUpdated));
            ItemUpdated(0, 0);
        }
    }

    private EntityUserControl _playerEntityControl;
    public EntityUserControl PlayerEntityControl {
        get { return _playerEntityControl; }
        set {
            _playerEntityControl = value;
            _playerEntityControl.Connect(nameof(EntityUserControl.ItemSwapPress), this, nameof(PlayerEntityControl_ItemsSwapped));
        }
    }

    public override void _Ready() {
        ItemTextures[0] = GetNode("Slots/TopSlot/ItemTexture") as TextureRect;
        ItemTextures[1] = GetNode("Slots/CurrentSlot/ItemTexture") as TextureRect;
        ItemTextures[2] = GetNode("Slots/BottomSlot/ItemTexture") as TextureRect;
        Animations = GetNode("Animations") as AnimationPlayer;
    }

    private void PlayerEntityControl_ItemsSwapped() {
        int indexOfMainSlot = InventoryOwner.GetSlotIndexOf(InventoryOwner.MainSlotGroup, InventoryOwner.MainSlot);
        Animations.Play("Change");
        nextIndex = PlayerEntityControl.recentMovedSlotIndex+1;
        if (nextIndex >= InventoryOwner.GetSlotsCount())
            nextIndex = 0;
        if (nextIndex == indexOfMainSlot)
            nextIndex++;
        Item newItem = InventoryOwner.GetItemAtIndex(nextIndex);
        ItemTextures[0].Texture = (newItem != null) ? ResourceLoader.Load(newItem.PathToIcon) as Texture : null;

        Item previousItem = InventoryOwner.GetItemAtIndex(PlayerEntityControl.recentMovedSlotIndex);
        ItemTextures[2].Texture = (previousItem != null) ? ResourceLoader.Load(previousItem.PathToIcon) as Texture : null;
    }

    private void ItemUpdated(int group, int slot) {
        Item item = InventoryOwner.GetItemAtIndex(nextIndex);
        ItemTextures[0].Texture = (item != null) ? ResourceLoader.Load(item.PathToIcon) as Texture : null;

        GD.Print("[UsingSlotsGUI] Next item is ", item);

        int indexOfMainSlot = InventoryOwner.GetSlotIndexOf(InventoryOwner.MainSlotGroup, InventoryOwner.MainSlot);
        if (InventoryOwner.GetSlotIndexOf(group, slot) == indexOfMainSlot) {
            Item it = InventoryOwner.GetItemAtIndex(indexOfMainSlot);
            ItemTextures[1].Texture = (it != null) ? ResourceLoader.Load(it.PathToIcon) as Texture : null;
        }
    }
}
