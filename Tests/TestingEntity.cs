using Godot;
using System;

//2019 © Даниил Белов
//Создано 15.05.2019

public class TestingEntity : Entity
{
    
    public override void _Ready() {
        EntityInventory = new Inventory("Test", new int[2] {2, 3}, new string[2] {"g1", "g2"});

        Actions = new EntityAction[2] {new EntityAction("a1", false, 2), new EntityAction("a2", true)};

        ItemPlaces.Add(new ItemPlace[1] {
            GetNode("ItPlOnHead") as ItemPlace
        });
    }


}
