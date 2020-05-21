using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 01.08.2019

public class ModalControls : Control
{

    protected PackedScene PackedLayer = ResourceLoader.Load<PackedScene>("res://GUI/ModalControls/ModalControlLayer.tscn");
    
    protected Dictionary<int, ModalControlLayer> Layers = new Dictionary<int, ModalControlLayer>();

    ///Номер игрока для этого интерфейса
    public int Player;

    ///Модально показать элемент интерфейса (control) для слоя (layer) с привязкой к существованию объекта (obj).
    ///Возвращает успешность. При провале (control) удаляется
    public bool ShowModal(Control control, int layer, Node obj = null) {
        //Отклонить при занятости слоя
        if (Layers.ContainsKey(layer)) {
            control.QueueFree();
            return false;
        }
        //Отклонение при наличии слоёв выше
        foreach (int l in Layers.Keys) {
            if (l > layer) {
                control.QueueFree();
                return false;
            }
        }
        
        ModalControlLayer newLayer = PackedLayer.Instance() as ModalControlLayer;

        Godot.Collections.Array bindsForLayerSignal = new Godot.Collections.Array();
        bindsForLayerSignal.Add(layer);
        newLayer.Connect("tree_exited", this, nameof(_LayerRemoved), bindsForLayerSignal);

        newLayer.Setup(control, obj);
        AddChild(newLayer);

        //Срытие слоев внизу, если они есть
        foreach (int l in Layers.Keys)
            if (l < layer)
                Layers[l].HideLayer();
        
        Layers.Add(layer, newLayer);
        
        //Показать, так как в любом случае уже есть хотя бы слой
        Show();

        return true;
    }

    public void StopModal(int layer) {
        //Отклонить при отсутствии слоя
        if (!Layers.ContainsKey(layer)) return;

        Layers[layer].QueueFree();
    }

    ///Получить элемент интерфейса на слое (layer)
    public Control GetControlOf(int layer) {
        if (!Layers.ContainsKey(layer)) return null;
        return Layers[layer].GetControlOfLayer();
    }

    ///Действия при удалении слоя (layer)
    protected void _LayerRemoved(int layer) {
        if (!Layers.ContainsKey(layer)) return;
        Layers.Remove(layer);

        DebugConsole.Shared.Output("[Modal] Removed "+layer);

        //Показ близжайшего слоя внизу, если есть
        if (Layers.Count > 0) {
            int max = int.MinValue;
            foreach (int l in Layers.Keys)
                if (max == int.MinValue)
                    max = l;
                else if (l > max)
                    max = l;
            Layers[max].ShowLayer();
            DebugConsole.Shared.Output("[Modal] Showed old "+max);
        } else {
            //Спрятать если нет слоёв
            DebugConsole.Shared.Output("[Modal] Hidden");
            Hide();
        }
    }

}
