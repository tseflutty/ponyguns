using Godot;
using System;

//2019 © Даниил Белов
//Создано 01.08.2019

public class ModalControlLayer : Control
{
    
    protected AnimationPlayer Animations;

    protected Control _controlOfLayer = null;

    ///Элемент интерфейса на этом слое
    protected Control ControlOfLayer {
        get { return _controlOfLayer; }
        set {
            if (_controlOfLayer != null) {
                _controlOfLayer.Disconnect("tree_exited", this, nameof(_ObjOrLayerRemoved));
                _controlOfLayer.QueueFree();
            }
            _controlOfLayer = value;
            if (_controlOfLayer != null) {
                _controlOfLayer.Connect("tree_exited", this, nameof(_ObjOrLayerRemoved));
                AddChild(_controlOfLayer);
            }
        }
    }

    protected Node _obj;

    //Объект, от существования которого зависит этот слой
    protected Node Obj {
        get { return _obj; }
        set {
            if (_obj != null)
                _obj.Disconnect("tree_exited", this, nameof(_ObjOrLayerRemoved));
            _obj = value;
            if (_obj != null)
                _obj.Connect("tree_exited", this, nameof(_ObjOrLayerRemoved));
        }
    }

    public override void _Ready() {
        Animations = GetNode("Animations") as AnimationPlayer;
    }

    ///Показать слой с анимацией
    ///Анимация может не работать если метод вызван сразу после создания
    public void ShowLayer() {
        if (Animations == null) {
            Show();
            return;
        }
        Animations.Stop();
        Animations.PlayBackwards("Hide");
        DebugConsole.Shared.Output("[ModalLayer] Showed");
    }

    ///Скрыть слой с анимацией
    ///Анимация может не работать если метод вызван сразу после создания
    public void HideLayer() {
        if (Animations == null) {
            Hide();
            return;
        }
        Animations.Stop();
        Animations.Play("Hide");

        DebugConsole.Shared.Output("[ModalLayer] Hidden");
    }

    public Control GetControlOfLayer() { return ControlOfLayer; }

    ///Установить слой с элементом интерфейса (control) и зависимостью от существования (obj)
    public void Setup(Control control, Node obj) {
        ControlOfLayer = control;
        Obj = obj;
    }

    //Удалить слой, когда Obj или ControlOfLayer удаляется
    protected void _ObjOrLayerRemoved() {
        QueueFree();
    }

    //Действия при изменении размера
    protected void _Resized() {
        ///Изменение точки, на основе который масштабируется слой (По центру)
        SetPivotOffset(GetSize()/2);
    }

    

}
