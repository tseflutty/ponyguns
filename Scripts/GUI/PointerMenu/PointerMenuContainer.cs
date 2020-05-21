using Godot;
using System;

//2019 © Даниил Белов
//Создано 29.08.2019

public class PointerMenuContainer : Control
{
    ///Стрелка указатель
    protected TextureRect ArrowTextureRect;

    protected AnimationPlayer Animations;
    
    protected Control _menu;
    ///GUI для этого контейнера
    public Control Menu {
        get { return _menu; }
        set {
            if (_menu != null) {
                Menu.Disconnect("draw", this, nameof(_Menu_ResizedOrMoved));
            }

            _menu = value;
            _UpdateMenuRect();

            if (_menu != null) {
                Menu.Connect("draw", this, nameof(_Menu_ResizedOrMoved));
            }
        }
    }

    [Export]
    ///Настройка GUI для этого контейнера в Инспекторе
    protected NodePath _Menu_;

    [Export]
    ///Показать с анимацией при старте
    public bool ShowAtReady = false;

    protected bool _antiExitBounds = false;
    [Export]
    ///Включение предотврощения выхода за рамки
    ///При true, контейнер может менять направление, если
    ///не удаётся уместить при приоритеном (PriorityGUIPoisution)
    public bool AntiExitBounds {
        get { return _antiExitBounds; }
        set {
            _antiExitBounds = value;
            _UpdateMenuRect();
        }
    }

    protected MenuPosition _priorityGUIPosition = MenuPosition.top;

    [Export]
    ///Приоритетное направление меню относительно точки, на которую надо указывать, при
    ///предотвращении выхода за границы
    public MenuPosition PriorityGUIPoisution {
        get { return _priorityGUIPosition; }
        set {
            _priorityGUIPosition = value;
            _UpdateMenuRect();
        }
    }

    protected float _boundsPadding = 10;
    [Export]
    public float BoundsPadding {
        get { return _boundsPadding; }
        set {
            _boundsPadding = value;
            _UpdateMenuRect();
        }
    }


    protected MenuPosition _GUIPosition = MenuPosition.left;

    [Export]
    ///Положение меню относительно точки, на которою надо указывать
    public MenuPosition GUIPosition {
        get { return _GUIPosition; }
        set {
            _GUIPosition = value;
            _UpdateArrowRotation();
            _UpdateMenuPostion();
        }
    }

    public override void _Ready() {
        base._EnterTree();

        ArrowTextureRect = GetNode("ArrowTextureRect") as TextureRect;
        Animations = GetNode("Animations") as AnimationPlayer;

        //Установка GUI для этого контейнера из инспектора
        if (_Menu_ != null && HasNode(_Menu_)) {
            Node n = GetNode(_Menu_);
            Menu = (n is Control) ? n as Control : null;
        }

        _UpdateArrowRotation();
        _UpdateMenuRect();


        if (ShowAtReady)
            ShowGUI(true);
    }

    public override void _Draw() {
        _UpdateArrowRotation();
        _UpdateMenuRect();
    }

    protected void _UpdateArrowRotation() {
        if (ArrowTextureRect == null) return;
        switch (GUIPosition) {
            case MenuPosition.bottom:
                ArrowTextureRect.RectRotation = 180;
                break;
            case MenuPosition.right:
                ArrowTextureRect.RectRotation = 90;
                break;
            case MenuPosition.top:
                ArrowTextureRect.RectRotation = 0;
                break;
            case MenuPosition.left:
                ArrowTextureRect.RectRotation = 270;
                break;
        }
    }

    public Rect2 GetArrowRect() {
        Rect2 ArrowRect = new Rect2();
        if (ArrowTextureRect == null) return ArrowRect;

        switch (GUIPosition) {
            case MenuPosition.bottom:
                ArrowRect = new Rect2(
                    -ArrowTextureRect.RectSize.x/2,
                    0,
                    ArrowTextureRect.RectSize.x,
                    ArrowTextureRect.RectSize.y
                );
                break;
            case MenuPosition.right:
                ArrowRect = new Rect2(
                    0,
                    -ArrowTextureRect.RectSize.x/2,
                    ArrowTextureRect.RectSize.y,
                    ArrowTextureRect.RectSize.x
                );
                break;
            case MenuPosition.top:
                ArrowRect = new Rect2(
                    -ArrowTextureRect.RectSize.x/2,
                    -ArrowTextureRect.RectSize.y,
                    ArrowTextureRect.RectSize.x,
                    ArrowTextureRect.RectSize.y
                );
                break;
            case MenuPosition.left:
                ArrowRect = new Rect2(
                    -ArrowTextureRect.RectSize.y,
                    -ArrowTextureRect.RectSize.x/2,
                    ArrowTextureRect.RectSize.y,
                    ArrowTextureRect.RectSize.x
                );
                break;
        }

        return ArrowRect;
    }

    ///Обновляет позицию относительно указателя
    protected void _UpdateMenuPostion() {
        if (Menu == null || ArrowTextureRect == null) return;

        //Получение прямоугольника указателя
        Rect2 ArrowRect = GetArrowRect();

        //Привязка к указателю
        if (GUIPosition == MenuPosition.left || GUIPosition == MenuPosition.right) {
            Menu.RectPosition = new Vector2(
                (GUIPosition == MenuPosition.left) ? -ArrowRect.Size.x - Menu.RectSize.x : 
                                                     ArrowRect.Size.x, 
                -Menu.RectSize.y/2
            );
        } else {
            Menu.RectPosition = new Vector2(
                -Menu.RectSize.x/2, 
                (GUIPosition == MenuPosition.top) ? -ArrowRect.Size.y - Menu.RectSize.y : 
                                                    ArrowRect.Size.y
            );
        }

        //Ограничение выхода за края
        Node parrent = GetParent();
        if (parrent != null && parrent is Control) {
            Control controlParent = parrent as Control;
            Vector2 menuPos = Menu.GetGlobalPosition() - controlParent.GetGlobalPosition();
            if (GUIPosition == MenuPosition.left || GUIPosition == MenuPosition.right) {
                if (menuPos.y < BoundsPadding) {
                    Menu.SetGlobalPosition(new Vector2(Menu.GetGlobalPosition().x, 
                        controlParent.GetGlobalPosition().y + BoundsPadding)
                    );
                } else if (menuPos.y + Menu.RectSize.y > controlParent.RectSize.y - BoundsPadding) {
                    Menu.SetGlobalPosition(new Vector2(Menu.GetGlobalPosition().x, 
                        controlParent.GetGlobalPosition().y + 
                        controlParent.RectSize.y - BoundsPadding - Menu.RectSize.y)
                    );
                }
            } else {
                if (menuPos.x < BoundsPadding) {
                    Menu.SetGlobalPosition(new Vector2(
                        controlParent.GetGlobalPosition().x + BoundsPadding, 
                        Menu.GetGlobalPosition().y)
                    );
                } else if (menuPos.x + Menu.RectSize.x > controlParent.RectSize.x - BoundsPadding) {
                    Menu.SetGlobalPosition(new Vector2(
                        controlParent.GetGlobalPosition().x + 
                        controlParent.RectSize.x - BoundsPadding - Menu.RectSize.x,            
                        Menu.GetGlobalPosition().y)
                    );
                }
            }
        }
    }

    protected void _UpdateMenuRect() {
        _UpdateMenuPostion();
        if (AntiExitBounds) {
            _UpdateAntiExitBounds();
        }
    }

    protected void _UpdateAntiExitBounds() {
        if (Menu == null || ArrowTextureRect == null) return;
        GUIPosition = PriorityGUIPoisution;
        
        // Node parrent = GetParent();
        // if (parrent != null && parrent is Control) {
        //     Control controlParent = parrent as Control;
        //     Rect2 menuRect = Menu.GetGlobalRect();
        //     Rect2 parentRect = controlParent.GetGlobalRect();
            
        //     if (menuRect.Position.x < parentRect.Position.x)
        //         GUIPosition = MenuPosition.right;
        //     if (menuRect.Position.y < parentRect.Position.y)
        //         GUIPosition = MenuPosition.bottom;
        //     if (menuRect.Position.x + menuRect.Size.x > parentRect.Position.x + menuRect.Size.x)
        //         GUIPosition = MenuPosition.left;
        //     if (menuRect.Position.y + menuRect.Size.y > parentRect.Position.y + menuRect.Size.y)
        //         GUIPosition = MenuPosition.top;
        // }
    }

    protected void _Menu_ResizedOrMoved() {
        _UpdateMenuPostion();
    }

    public void ShowGUI(bool animated) {
        Show();
        if (Animations == null || !animated) {
            Animations.Play("ToDefaults");
            return;
        }
        Animations.Play("Show");
    }

    public void HideGUI(bool animated) {
        if (Animations == null || !animated) {
            Hide();
            return;
        }
        Animations.Play("Hide");
    }

    public void RemoveWithAnimation() {
        HideGUI(true);
        Animations.Connect("animation_finished", this, nameof(_Animations_AnimationEnded));
    }

    //Для удаления по окончанию анимации
    protected void _Animations_AnimationEnded(string animName){
        QueueFree();
    }

    public enum MenuPosition {
        left,
        right,
        top,
        bottom
    }

}
