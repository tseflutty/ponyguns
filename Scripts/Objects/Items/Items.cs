using Godot;
using System;


///Содержит список всех предметов игры. В этом объекте вы можете получить предмет по его UID (Items.Get(int UID))
public class Items
{
    public static Item Get(int UID) {
        switch (UID) {
            case 1:
                return new Item("Тестовое яблочко", "res://Tests/testitem.png", 1);
            case 2:
                return new TestGun();
        }
        return new Item("Какашка :3", "res://Images/VisualItem/VisualItem.png", 0);
    }

///Получить данные предмета без самого предмета по UID (UID)
    public static ItemData GetItemData(int UID) {
        Item item = Get(UID);
        ItemData data = new ItemData(item.ItemName, item.PathToIcon);
        item.QueueFree();
        return data;
    }

    public struct ItemData {
        public string Name;
        public string PathToIcon;

        public ItemData(string name, string pathToIcon) {
            Name = name; PathToIcon = pathToIcon;
        }
    }
}
