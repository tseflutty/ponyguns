using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 29.07.2019

///Оболочка для System.Collectios.Generic.List с поддержкой сигналов Godot о добавлении, удалении предметов
public class PonyNodeList : Node
{

    [Signal]
    public delegate void ItemAdded(Node item);

    [Signal]
    public delegate void ItemRemoved(Node item);

    protected List<Node> Arr = new List<Node>();

    public int Count {
        get { return Arr.Count; }
    }

    public void Add(Node item) {
        Arr.Add(item);
        EmitSignal(nameof(ItemAdded), item);
    }

    public void Remove(Node item) {
        if (Arr.Remove(item))
            EmitSignal(nameof(ItemRemoved), item);
    }

    public void RemoveAt(int index) {
        if (Arr.Count <= index) return;
        Node item = Arr[index];
        Arr.Remove(item);
        EmitSignal(nameof(ItemRemoved), item);
    }

    public void Clear() {
        foreach (Node item in Arr)
            EmitSignal(nameof(ItemRemoved), item);
        Arr.Clear();
    }

    public bool Contains(Node item) {
        return Arr.Contains(item);
    }

    public Node[] GetArr() {
        Node[] r = new Node[Arr.Count];
        for (int i = 0; i < r.Length; ++i)
            r[i] = Arr[i];
        return r;
    }
    
}
