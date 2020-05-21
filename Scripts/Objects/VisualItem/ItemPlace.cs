using Godot;
using System;

//2019 © Даниил Белов
//Создано 15.05.2019

///<summary>
///<para>Точка в трёхмерном пространстве, где может выводиться визуальное представлнение предмета</para>
///</summary>
public class ItemPlace : Spatial
{
    
    protected VisualItem _itemOfPlace;

    ///<summary>
    ///<para>Объект предмента, который отображается непосредственно на сцене</para>
    ///</summary>
    public virtual VisualItem ItemOfPlace {
        get { return _itemOfPlace; }
        set {
            if (_itemOfPlace != null)
                _itemOfPlace.QueueFree();
            _itemOfPlace = null;
            if (value != null) {
                _itemOfPlace = value;
                AddChild(_itemOfPlace);
            }
        }
    }

}
