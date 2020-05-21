using Godot;
using System;
using System.Collections.Generic;

///Дополненный InventoryBasedObject с возвожность отображать предметы, лежащие в интвентаре, в игровом мире
///с помощтю ItemPlace
public class VisualInventoryObject : InventoryBasedObject
{   
    public override AdvancedInventory Content {
        get { return base.Content; }
        set { 
            base.Content = value;
            _UpdateItemPlaces();
        }
    }

    //ItemPlaces[group][slot]
    protected List<InventoryItemPlace[]> _itemPlaces = new List<InventoryItemPlace[]>();
    
    public InventoryItemPlace[][] GetItemPlaces() {
        return _itemPlaces.ToArray(); 
    }

    ///Массив мест для отображения визуальных представлений предмета
    ///Задаётся в формате list[group][slot] 
    ///Если для определённого слота не нужно место для отображения, то можно указать null
    public void SetItemPlaces(InventoryItemPlace[][] list) {
        _itemPlaces.Clear();
        foreach (InventoryItemPlace[] l in list) {
            _itemPlaces.Add(l);
        }
        _UpdateItemPlaces();
    }

    protected virtual void _UpdateItemPlaces() {
        if (Content == null) return;
        for (int group = 0; group < _itemPlaces.Count; group++) {
            for (int slot = 0; slot < _itemPlaces[group].Length; slot++) {
                InventoryItemPlace place = _itemPlaces[group][slot];
                if (place != null) {
                    place.Used = Content;
                    place.Group = group;
                    place.Slot = slot;
                }
            }
        }
    }

    
    

}
