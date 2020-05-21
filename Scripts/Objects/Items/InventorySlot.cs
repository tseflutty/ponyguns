using Godot;
using System;

//2019 © Даниил Белов
//Создано 06.05.2019


public struct InventorySlot
{
    public Item ItemOfSlot;
    public int[] Tags;
}

public enum SlotTags {
    Standart,
    MagicWithUse,
    Mouth
}