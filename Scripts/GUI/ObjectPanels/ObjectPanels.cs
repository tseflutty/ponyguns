using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 28.07.2019

public class ObjectPanels : Control
{
    protected struct ObjectPanelData {
        public Control panel;
        public Vector2 offset;

        public ObjectPanelData(Control p, Vector2 o) {
            panel = p; offset = o;
        }
    }

    protected Dictionary<Spatial, ObjectPanelData> Panels = new Dictionary<Spatial, ObjectPanelData>();

    public int playerUI;

    ///Показать панель (panel) над объектом (obj) со смещением (offset)
    public void ShowObjectPanel(Spatial obj, Vector2 offset, Control panel) {
        if (panel == null)  return;

        //Удаление старой панели если она есть
        if (Panels.ContainsKey(obj)) {
            Panels[obj].panel.QueueFree();
            //TODO: Убрать, сделать очищение по сигналу удаления
            Panels.Remove(obj);

            obj.Disconnect("tree_exited", this, nameof(ObjectRemoved));
        }
        
        //Добавление панели на сцену и в словарь
        AddChild(panel);
        Panels.Add(obj, new ObjectPanelData(panel, offset));

        Godot.Collections.Array bindsForObjSignal = new Godot.Collections.Array();
        bindsForObjSignal.Add(obj);
        Godot.Collections.Array bindsForPanelSignal = new Godot.Collections.Array();
        bindsForPanelSignal.Add(panel);
        obj.Connect("tree_exited", this, nameof(ObjectRemoved), bindsForObjSignal);
        panel.Connect("tree_exited", this, nameof(PanelRemoved), bindsForPanelSignal);
    }

    ///Удаляет панель объекта (obj)
    public void HideObejctPanel(Spatial obj) {
        if (!Panels.ContainsKey(obj)) return;
        Panels[obj].panel.QueueFree();
        //TODO: Убрать, сделать очищение по сигналу удаления
        Panels.Remove(obj);
        obj.Disconnect("tree_exited", this, nameof(ObjectRemoved));
    }

    ///Изменить смещение панели объекта (obj) на (offset)
    public void ChangeOffsetOfObjectPanel(Spatial obj, Vector2 offset) {
        if (!Panels.ContainsKey(obj)) return;
        ObjectPanelData data = Panels[obj];
        data.offset = offset;
        Panels[obj] = data;
    }

    ///Получить панель объекта (obj)
    public Control GetObjectPanel(Spatial obj) {
        if (!Panels.ContainsKey(obj)) return null;
        return Panels[obj].panel;
    }

    public override void _PhysicsProcess(float delta) {
        if (Arena.GetCurrent() == null) return;
        //Привязка положения панели к объекту
        foreach (Spatial obj in Panels.Keys) {
            Control panel = Panels[obj].panel;
            Vector2 offset = Panels[obj].offset;
            Arena currentArena = Arena.GetCurrent();
            Camera cam = currentArena.MainCamera;
            Vector3 pos = obj.GetGlobalTransform().origin;
            panel.SetPosition(cam.UnprojectPosition(pos) + offset);
        } 
        //ПОТОМ TODO: Сделать привязку к камерам отдельных игроков
    }

    protected void ObjectRemoved(Spatial obj) {
        Panels[obj].panel.Disconnect("tree_exited", this, nameof(PanelRemoved));
        obj.Disconnect("tree_exited", this, nameof(ObjectRemoved));
        Panels[obj].panel.QueueFree();
        Panels.Remove(obj);
    }

    protected void PanelRemoved(Control control) {
        foreach (Spatial obj in Panels.Keys) {
            if (Panels[obj].panel == control) {
                Panels.Remove(obj);
                obj.Disconnect("tree_exited", this, nameof(ObjectRemoved));
                return;
            }
        }
    }
}
