using Godot;
using System;

//2019 © Даниил Белов
//Создано 09.05.2019

public class InventoryGUISlot : Control
{

    ///<summary>
    ///<para>Номер группы в инвентаре, которая принадлежит этот визуальный слот</para>
    ///</summary>   
    public int Group;

    ///<summary>
    ///<para>Номер слота в группе, который принадлежит этот визуальный слот</para>
    ///</summary>   
    public int Slot;

    ///<summary>
    ///<para>Инветарь, к которому относится этот слот</para>
    ///</summary>  
    public Inventory _inventory;

    private Texture[] Textures = new Texture[2];

    private bool _listenAdding;
    ///<summary>
    ///<para>Слушает ли слот добавление. Если установлено True, контекстное меню не открывается и по нажатию испускается сигнал AddingListened</para>
    ///</summary>
    public bool ListenAdding {
        get { return _listenAdding; }
        set {
            _listenAdding = value;
        }
    }

    private Item _itemOfSlot = null;
    ///<summary>
    ///<para>Предмет, лежащий в слоте, который принадлежит этому визуальному слоту</para>
    ///</summary> 
    public Item ItemOfSlot {
        get { return _itemOfSlot; }
        set {
            _itemOfSlot = value;
            if (_itemOfSlot != null) {
                StreamTexture newItemTexture = ResourceLoader.Load(_itemOfSlot.PathToIcon).Duplicate() as StreamTexture;
                newItemTexture.Flags = 0;
                ItemTexture.Texture = newItemTexture;
            } else {
                ItemTexture.Texture = null;
            }
        }
    }

    ///<summary>
    ///<para>Установлено True, если пользователь сейчас видит контекстное меню этого визуального слота</para>
    ///</summary> 
    public bool ContextShowing = false;

    ///<summary>
    ///<para>Установлено True, если слот ожидает нажатие для добавления предмета; После нажатия подаёт сигнал AddingListened</para>
    ///</summary> 
    public bool ListeningAdding = false;

    private TextureRect ItemTexture {
        get { return GetNode("ItemTexture") as TextureRect; }
    }
    
    [Signal]
    public delegate void ItemStartMove(int group, int slot);

    [Signal]
    public delegate void AddingListened(int group, int slot);

    [Signal]
    public delegate void SlotClicked(int group, int slot);

    [Signal]
    public delegate void ItemUsed(int group, int slot);

    public override void _Ready() {
        Textures[0] = ResourceLoader.Load("res://Images/GUI/Inventory/Slot.png") as Texture;
        Textures[1] = ResourceLoader.Load("res://Images/GUI/Inventory/MainSlot.png") as Texture;
    }

    public override void _Draw() {
        if (_inventory != null) {
            if (!_inventory.UseMainSlot || _inventory.MainSlotGroup != Group || _inventory.MainSlot != Slot)
                DrawTexture(Textures[0], new Vector2(0, 0));
            else if (_inventory.MainSlotGroup == Group && _inventory.MainSlot == Slot)
                DrawTexture(Textures[1], new Vector2(-3, -3));
        } else DrawTexture(Textures[0], new Vector2(0, 0));
    }

    private void PickDownButton_Click() {
        _inventory.RemoveItemAt(Group, Slot); //sex, baby
    }
    
    private void MoveButton_Click() {
        EmitSignal(nameof(ItemStartMove), Group, Slot);
    }

    private void SlotClick() {
        if (ListenAdding)
            EmitSignal(nameof(AddingListened), Group, Slot);
        EmitSignal(nameof(SlotClicked), Group, Slot);
    }

    public void SetInventory(Inventory inv) {
        _inventory = inv;
        Update();
    }


    // public InventoryGUISlot(int group, int slot, Inventory inventory) {
    //     _inventory = inventory;
    // }

}
