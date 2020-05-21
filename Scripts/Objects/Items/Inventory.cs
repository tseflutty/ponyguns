using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 02.05.2019 в 00:36

///!!! Для нормальной работоспособности должен добавлен в дерево с помощью AddChild(...)
public class Inventory : Node
{

    [Signal]
    public delegate void ItemAdded(int group, int slot);

    [Signal]
    public delegate void ItemRemoved(int group, int slot);

    [Signal]
    public delegate void ItemRemoving(int group, int slot, Item item);

    //Вызывается Когда пользователь нажал кнопку использовать
    [Signal]
    public delegate void ItemUse(int group, int slot);

    [Export]
    ///Заголовок инвентаря
    public string InventoryName = "";

    ///Группы слотов инвентаря
    protected List<InventorySlotGroup> Groups;

    ///Группа главного слота
    public int MainSlotGroup = 0;

    ///Номер главного слота в группе
    public int MainSlot = 0;

    ///Будет ли использоваться главный слот
    public bool UseMainSlot = false;

    ///Удалить предмент с сылкой (item). Возращает True, если предмет для удаления (item) найден в этом инвентаре, в противном случае возвращает False
    public bool RemoveItem(Item item) {
        GD.Print("[Inventory] RemoveItem");
        for (int group = 0; group < Groups.Count; ++group) {
            for (int slot = 0; slot < Groups[group].Count; ++slot) {
                if (Groups[group].Slots[slot].ItemOfSlot != null && Groups[group].Slots[slot].ItemOfSlot == item) {
                    if (!(Groups[group].Slots[slot].ItemOfSlot.GetParent() is Inventory))
                        RemoveChild(Groups[group].Slots[slot].ItemOfSlot);
                    GD.Print("EMIT RMng SIGNAL");
                    EmitSignal(nameof(ItemRemoving), group, slot, Groups[group].Slots[slot].ItemOfSlot);
                    Groups[group].Slots[slot].ItemOfSlot = null;
                    GD.Print("EMIT RM SIGNAL");
                    EmitSignal(nameof(ItemRemoved), group, slot);
                    return true;
                }
            }
        }
        GD.Print("[Inventory] RemoveItem Cancel");
        return false;
    }

    ///Удалить предмент в группе под индексом (group) в слоте под индексом (slot). Возращает True, если слот с предметом для удаления в указанном месте не пустой, в противном случае возвращает False
    public bool RemoveItemAt(int group, int slot) {
        GD.Print("[Inventory] RemoveItemAt");
        if (group > Groups.Count-1) return false;
        if (slot > Groups[group].Count-1) return false;

        if (Groups[group].Slots[slot].ItemOfSlot != null) {
            if (!(Groups[group].Slots[slot].ItemOfSlot.GetParent() is Inventory))
                RemoveChild(Groups[group].Slots[slot].ItemOfSlot);
            EmitSignal(nameof(ItemRemoving), group, slot, Groups[group].Slots[slot].ItemOfSlot);
            Groups[group].Slots[slot].ItemOfSlot = null;
            GD.Print("EMIT RM SIGNAL");
            EmitSignal(nameof(ItemRemoved), group, slot);
            return true;
        }
        return false;
    }

    ///Удалить первый предмет с UID (uID).
    ///Возвращает True, Если предмет с UID (uID) существует и был удалён, в противном случае возвращает False
    public bool RemoveItemByUID(int uID) {
        for (int group = 0; group < Groups.Count; ++group) {
            for (int slot = 0; slot < Groups[group].Count; ++slot) {
                if (Groups[group].Slots[slot].ItemOfSlot != null && Groups[group].Slots[slot].ItemOfSlot.UID == uID) {
                    if (!(Groups[group].Slots[slot].ItemOfSlot.GetParent() is Inventory))
                        RemoveChild(Groups[group].Slots[slot].ItemOfSlot);
                    GD.Print("EMIT RMng SIGNAL");
                    EmitSignal(nameof(ItemRemoving), group, slot, Groups[group].Slots[slot].ItemOfSlot);
                    Groups[group].Slots[slot].ItemOfSlot = null;
                    GD.Print("EMIT RM SIGNAL");
                    EmitSignal(nameof(ItemRemoved), group, slot);
                    return true;
                }
            }
        }
        return false;
    }

    ///Возращает True, если предмет (item) найден в этом инвентаре, в противном случае возвращает False
    public bool HasItem(Item item) {
        for (int group = 0; group < Groups.Count; ++group) {
            for (int slot = 0; slot < Groups[group].Count; ++slot) {
                if (Groups[group].Slots[slot].ItemOfSlot == item) {
                    return true;
                }
            }
        }
        return false;
    }

    ///Добавить предмет (item) в этот инвентарь. Возвращает True если место для этого предмента есть, в противном случае возвращает False
    public bool AddItem(Item item) {
        if (item == null) return false;
        for (int group = 0; group < Groups.Count; ++group) {
            for (int slot = 0; slot < Groups[group].Count; ++slot) {
                if (Groups[group].Slots[slot].ItemOfSlot == null) {
                    if (!(item.GetParent() is Inventory))
                        AddChild(item);
                    Groups[group].Slots[slot].ItemOfSlot = item;
                    EmitSignal(nameof(ItemAdded), group, slot);
                    return true;
                }
            }
        }
        return false;
    }

    ///Добавить предмет (item) в этот инвентарь в группу под индексом (group) в слот под индексом (slot).
    ///Возвращает True если указанный слот ссвободен для добавления, в противном случае возвращает False
    public bool AddItemTo(int group, int slot, Item item) {
        if (item == null) return false;
        if (group > Groups.Count-1) return false;
        if (slot > Groups[group].Count-1) return false;

        if (Groups[group].Slots[slot].ItemOfSlot == null) {
            if (!(item.GetParent() is Inventory))
                AddChild(item);
            Groups[group].Slots[slot].ItemOfSlot = item;
            EmitSignal(nameof(ItemAdded), group, slot);
            return true;
        }

        return false;
    }

    ///Возвращает True, если для предмета (item) найдётся место в слоте (slot) группы (group)
    public bool HasSpaceForIn(int group, int slot, Item item) {
        //TODO в будущем будет в слоте установлены ограничения на некоторые параметры предметов. Учитывать это
        if (item == null) return false;
        if (group > Groups.Count-1) return false;
        if (slot > Groups[group].Count-1) return false;
        if (Groups[group].Slots[slot].ItemOfSlot == null)
            return true;
        return false;
        
    }

    ///Возвращает True, если для предмета (item) найдётся место в инвентаре
    public bool HasSpaceFor(Item item) {
        if (item == null) return false;
        for (int group = 0; group < Groups.Count; ++group) {
            for (int slot = 0; slot < Groups[group].Count; ++slot) {
                if (HasSpaceForIn(group, slot, item))
                    return true;
            }
        }
        return false;
    }

    ///Использует слот по порядковому номеру (index) среди списка всех слотов в этом инвентаре
    public void UseSlotAtIndex(int index) {
        // List<int>[] list = GetUsableSlots();

        // int currentIndex = 0;

        // int size = 0;
        // for (int group = 0; group < list.Length; ++group) {
        //     for (int slot = 0; slot < list[group].Count; ++slot) {
        //         size++;
        //     }
        // }

        // //Исправление отрицательного числа
        // int normalIndex = index;
        // while (normalIndex < 0)
        //     normalIndex += size;

        // int searchingIndex = normalIndex % size;
        // GD.Print("[Inventory] requested : ", index, " sindex : ", searchingIndex);

        // for (int group = 0; group < list.Length; ++group) {
        //     for (int slot = 0; slot < list[group].Count; ++slot) {
        //         if (currentIndex == searchingIndex) {
        //             EmitSignal(nameof(ItemUse), group, list[group][slot]);
        //             return;
        //         }
        //         currentIndex++;
        //     }
        // }
        //Реализовать потом
        //TODO: Теперь у предмета будет булева переменная о том, можно ли его использовать через инвентарь
    }

    ///Возвращает предмет, лежащий в слоте по указанному индексу (index)
    public Item GetItemAtIndex(int index) {
        int currentIndex = 0;

        int size = GetSlotsCount();

        //Исправление отрицательного числа
        int normalIndex = index;
        while (normalIndex < 0)
            normalIndex += size;

        int searchingIndex = normalIndex % size;
        GD.Print("[Inventory] requested : ", index, " sindex : ", searchingIndex);

        for (int group = 0; group < Groups.Count; ++group) {
            for (int slot = 0; slot < Groups[group].Count; ++slot) {
                if (currentIndex == searchingIndex)
                    return GetItemAt(group, slot);
                currentIndex++;
            }
        }
        return null;
    }

    ///Возвращает индекс слота по адресу (slot, group) среди списка всех слотов этого инвентаря
    public int GetSlotIndexOf(int group, int slot) {
        int index = 0;

        for (int g = 0; g < Groups.Count; ++g) {
            for (int s = 0; s < Groups[g].Count; ++s) {
                if (g == group && s == slot)
                    return index;
                index++;
            }
        }

        return index;
    }

    ///Перемещает или меняет местами предметы по указанным индексам
    public bool MoveItemAtIndexToIndex(int from, int to) {
        int size = GetSlotsCount();

        //Исправление отрицательного числа
        int normalFrom = from;
        while (normalFrom < 0)
            normalFrom += size;

        int indexFrom = normalFrom % size;

        //Исправление отрицательного числа
        int normalTo = to;
        while (normalTo < 0)
            normalTo += size;

        int indexTo = normalTo % size;

        int searchingFrom = 0, searchingTo = 0;

        int fromGroup = 0, fromSlot = 0;
        int toGroup = 0, toSlot = 0;
        
        //Определение группы и номера слота для слота from
        for (int g = 0; g < Groups.Count; ++g) {
            bool br = false;
            for (int s = 0; s < Groups[g].Count; ++s) {
                if (searchingFrom == from) {
                    fromGroup = g; fromSlot = s;
                    br = true;
                    break;
                }
                searchingFrom++;
            }
            if (br) break;
        }

        //Определение группы и номера слота для слота to
        for (int g = 0; g < Groups.Count; ++g) {
            bool br = false;
            for (int s = 0; s < Groups[g].Count; ++s) {
                if (searchingTo == to) {
                    toGroup = g; toSlot = s;
                    br = true;
                    break;
                }
                searchingFrom++;
            }
            if (br) break;
        }

        Item MovingItem = GetItemAt(fromGroup, fromSlot);
        Item ItemInSlotTo = GetItemAt(toGroup, toSlot);

        if (ItemInSlotTo == null) {
            if (RemoveItemAt(fromGroup, fromSlot)) {
                AddItemTo(toGroup, toSlot, MovingItem);
                return true;
            }
        } else {
            GD.Print("[Inventory] Items swap");
            if (RemoveItemAt(fromGroup, fromSlot)) {
                GD.Print("[Inventory] Items swap 1: ", MovingItem.ItemName, "; ", ItemInSlotTo.ItemName);
                AddItemTo(fromGroup, fromSlot, ItemInSlotTo);
                RemoveItemAt(toGroup, toSlot);
                AddItemTo(toGroup, toSlot, MovingItem);
                GD.Print("[Inventory] After swap 1: ", MovingItem.ItemName, "; ", ItemInSlotTo.ItemName);
                return true;
            } else {
                GD.Print("[Inventory] Items swap 2");
                AddItemTo(fromGroup, fromSlot, ItemInSlotTo);
                RemoveItemAt(toGroup, toSlot);
                return true;
            }
        }

        return false;
    }

    ///Установить теги (tags) для слота (slot) группы (group)
    public void SetSlotTags(int group, int slot, int[] tags) {
        if (group > Groups.Count-1) return;
        if (slot > Groups[group].Count-1) return;
        Groups[group].Slots[slot].Tags = tags;
    }

    ///Возвращает массив тегов для слота (slot) группы (group). В случае отсутствия списка тегов, возвращает null
    public int[] GetSlotTags(int group, int slot) {
        if (group > Groups.Count-1) return null;
        if (slot > Groups[group].Count-1) return null;
        return Groups[group].Slots[slot].Tags;
    }

    ///Возвращает количество слотов во всех группах инвентаря
    public int GetSlotsCount() {
        int count = 0;

        for (int g = 0; g < Groups.Count; ++g)
            for (int s = 0; s < Groups[g].Count; ++s)
                count++;

        return count;
    }

    ///Возвращает количество групп слотов
    public int GetGroupsCount() { return Groups.Count; }

    ///Возвращает количество слотов в группе под индексом (group)
    public int GetSlotCountOfGroup(int group) { return Groups[group].Count; }

    ///Возвращает предмент, лежащий в указанном слоте
    public Item GetItemAt(int group, int slot) { return Groups[group].Slots[slot].ItemOfSlot; }

    ///Возвращает название грыппы под индексом (group) 
    public string GetGroupName(int group) { return Groups[group].GroupName; }

    public Inventory(List<InventorySlotGroup> groups) {
        Groups = groups;
    }

    public Inventory() {
        Groups = new List<InventorySlotGroup>();

        InventoryName = "No name";
        
        Groups.Add(new InventorySlotGroup("group", 1));
    }

    ///Создаёт группы для инвентаря в клоичстве размера миссива (sizes) с пустыми слотами в размере числового значения миссива (sizes) по индексу группы
    public Inventory(string inventoryName, int[] sizes, string[] names) {
        //Создание массива груп
        Groups = new List<InventorySlotGroup>();

        //Установка имени инвентаря
        InventoryName = inventoryName;
        
        //Создание слотов и групп в нужном количестве
        for (int i = 0; i < sizes.Length; ++i)
            Groups.Add(new InventorySlotGroup(names[i], sizes[i]));
    }

    ///Создаёт группы для инвентаря в клоичстве размера миссива (sizes) с пустыми слотами в размере числового значения миссива (sizes) по индексу группы
    ///Назначает главный слот в группе (mainSlotGroup) под номером (mainSlot) и включает его использование
    public Inventory(string inventoryName, int[] sizes, string[] names, int mainSlotGroup, int mainSlot) {
        //Создание массива груп
        Groups = new List<InventorySlotGroup>();

        //Установка имени инвентаря
        InventoryName = inventoryName;
        
        //Создание слотов и групп в нужном количестве
        for (int i = 0; i < sizes.Length; ++i)
            Groups.Add(new InventorySlotGroup(names[i], sizes[i]));
        
        //Установка сведений о главном слоте
        MainSlotGroup = mainSlotGroup;
        MainSlot = mainSlot;
        UseMainSlot = true;
    }
}


///Объект информации о предмете инвентаря
public class Item : Node {

    ///Видимое название предмета
    public string ItemName;

    ///Ссылка на файл текстуры иконки предмета
    public string PathToIcon;

    ///Уникальный числовой идентификатор вида предмета
    public int UID;

    public int[] ItemTags;

    private int[] _slotTags = new int[0];
    ///Теги слота, в котором лежит этот предмет
    public int[] SlotTags {
        get { return _slotTags; }
        set {
            _slotTags = value;
            UpdateTags();
        }
    }

    public string PathToVisualItem = "res://Objects/VisualItem/VisualItem.tscn"; 

    public Item(string iName, string iPathToIcon, int iUID, int[] itemTags = null) {
        ItemName = iName; PathToIcon = iPathToIcon; UID = iUID;
        if (itemTags != null) ItemTags = itemTags;
    }

    protected void UpdateTags() {

    }

    public Item() {
        ItemName = "Предмет"; PathToIcon = ""; UID = 0;
    }
}

public enum ItemTag {
    Standart,
    HasGrip,
    Small
}