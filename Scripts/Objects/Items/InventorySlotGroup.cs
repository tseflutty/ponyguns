using Godot;
using System;

//2019 © Даниил Белов
//Создано 06.05.2019

public class InventorySlotGroup
{
    ///<summary>
    ///<para>Заголовок группы в инвентаре</para>
    ///</summary>
    public string GroupName = "Инвентать";
    
    private int _count = 0;

    ///<summary>
    ///<para>Количество предметов в группе</para>
    ///</summary>
    public int Count {
        get { return _count; }
        set { 
            int oldCount = _count;
            _count = value;

            //Создание нового массива предметов
            InventorySlot[] newSlots = new InventorySlot[_count];
            
            //Перенос предметов из старого массива в новый. Если новый массив меньше старого, лишние предметы удаляются;
            if (Slots != null) {
                for (int i = 0; i < ((oldCount < _count) ? oldCount : _count); ++i)
                    newSlots[i] = Slots[i];
            } else {
                for (int i = 0; i < _count; ++i) {
                    newSlots[i] = new InventorySlot();
                    newSlots[i].Tags = new int[0];
                }
            }

            Slots = newSlots;
        }
    }

    ///<summary>
    ///<para>Слоты группы</para>
    ///</summary>
    public InventorySlot[] Slots;

    public InventorySlotGroup(string iName, int iCount) {
        GroupName = iName; Count = iCount;
    }
    
}
