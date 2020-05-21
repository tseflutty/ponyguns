using Godot;
using System;

//2019 © Даниил Белов
//Создано 02.09.2019


///Пара указывающих меню для GUI на арене
public class PointerMenuCouple : Control
{

    protected CouplePosition _position = CouplePosition.leftRight;

    [Export]
    ///Вариант положения меню
    public CouplePosition Position {
        get { return _position; }
        set {
            _position = value;
            ///TODO сделать обновление позиций меню
        }
    }

    ///Объект для указания первым меню
    public Spatial FirstObject;

    ///Объект для указания вторым меню
    public Spatial SecondObject;

    //Для инспектора
    [Export]
    protected NodePath _FirstObject_;
    [Export]
    protected NodePath _SecondObject_;

    protected PointerMenuContainer FirstMenu;
    protected PointerMenuContainer SecondMenu;

    [Export]
    ///Локальный номер игрока
    public byte PlayerID = 1;


    public override void _Ready() {
        //Установка объектов для указывания из инспектора
        if (FirstObject == null)
            SetupFromInspectorSpatial(out FirstObject, _FirstObject_);
        if (SecondObject == null)
            SetupFromInspectorSpatial(out SecondObject, _SecondObject_);

        FirstMenu = GetNode("PointerMenuContainer1") as PointerMenuContainer;
        SecondMenu = GetNode("PointerMenuContainer2") as PointerMenuContainer;

        FirstMenu.AntiExitBounds = true;
        SecondMenu.AntiExitBounds = true;
        _UpdateGUIsPositions();

        FirstMenu.Connect("tree_exited", this, nameof(_Menu_Removed));
        SecondMenu.Connect("tree_exited", this, nameof(_Menu_Removed));
    }

    protected void SetupFromInspectorSpatial(out Spatial s, NodePath np) {
        if (np != null && HasNode(np)) {
            Node n = GetNode(np);
            if (n is Spatial) {
                s = n as Spatial;
                return;
            }
        }
        s = null;
    }

    public override void _PhysicsProcess(float delta) {
        _UpdateGUIsPositions();
        _UpdateTarackToObjects();
    }

    ///Воспроизводит анимацию скрытия, после чего удаляет
    public void RemoveWithAnimation() {
        FirstMenu.RemoveWithAnimation();
        SecondMenu.RemoveWithAnimation();
    }

    //Для удаления после анимации
    protected void _Menu_Removed() {
        QueueFree();
    }

    protected void _UpdateTarackToObjects() {
        Spatial[] objects = new Spatial[2] {FirstObject, SecondObject};
        PointerMenuContainer[] menus = new PointerMenuContainer[2] {FirstMenu, SecondMenu};

        //Привязка к объектам
        if (FirstObject != null && SecondObject != null) {
            ///Временно пока по MainCamera
            Arena arena = Arena.GetCurrent();
            if (arena != null) {
                for (int i = 0; i < 2; ++i) {
                    Vector3 offset = new Vector3();
                    if (objects[i] is RectSized) {
                        RectSized obj = objects[i] as RectSized;
                        if (menus[i].GUIPosition == PointerMenuContainer.MenuPosition.left) {
                            offset.x = -obj.LeftRectOffset;
                        }
                        if (menus[i].GUIPosition == PointerMenuContainer.MenuPosition.right) {
                            offset.x = obj.RightRectOffset;
                        }
                        if (menus[i].GUIPosition == PointerMenuContainer.MenuPosition.top) {
                            offset.x = -obj.BottomRectOffset;
                        }
                        if (menus[i].GUIPosition == PointerMenuContainer.MenuPosition.bottom) {
                            offset.x = obj.TopRectOffset;
                        }
                        offset += obj.ZeroOffset;
                        
                    }

                    Vector2 pos = arena.MainCamera.UnprojectPosition(objects[i].GetGlobalTransform().origin + offset);
                    menus[i].RectPosition = pos;
                }
            }
        }
    }


    protected void _UpdateGUIsPositions() {
        Arena arena = Arena.GetCurrent();
        if (FirstObject != null && SecondObject != null && arena != null) {
            ///Временно пока по MainCamera
            PointerMenuContainer.MenuPosition[] positions =
                (Position == CouplePosition.leftRight)
                ? new PointerMenuContainer.MenuPosition[2] {PointerMenuContainer.MenuPosition.left, PointerMenuContainer.MenuPosition.right}
                : new PointerMenuContainer.MenuPosition[2] {PointerMenuContainer.MenuPosition.top, PointerMenuContainer.MenuPosition.bottom};


            float firstObjPos = (Position == CouplePosition.leftRight)
                ? arena.MainCamera.UnprojectPosition(FirstObject.GetGlobalTransform().origin).x
                : arena.MainCamera.UnprojectPosition(FirstObject.GetGlobalTransform().origin).y;
            
            float secondObjPos = (Position == CouplePosition.leftRight)
                ? arena.MainCamera.UnprojectPosition(SecondObject.GetGlobalTransform().origin).x
                : arena.MainCamera.UnprojectPosition(SecondObject.GetGlobalTransform().origin).y;

            FirstMenu.PriorityGUIPoisution = (firstObjPos < secondObjPos) ? positions[0] : positions[1];
            SecondMenu.PriorityGUIPoisution = (firstObjPos > secondObjPos) ? positions[0] : positions[1];
        }
    }
    
    public enum CouplePosition {
        leftRight,
        topBottom
    }

}
