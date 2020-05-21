using Godot;
using System;

//2019 © Даниил Белов
//Создано 15.07.2019

public class MobAI : Node
{
    [Export]
    public NodePath AiSlave;

    protected Entity Slave;

    public override void _Ready() {
        //Установка раба ИИ
        Node slave = GetNode(AiSlave);
        if (slave is Entity) {
            Slave = slave as Entity;
        }
    }
}
