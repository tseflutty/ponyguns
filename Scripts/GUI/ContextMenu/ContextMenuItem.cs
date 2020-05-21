using Godot;
using System;

//2019 © Даниил Белов
//Создано 25.06.2019

public class ContextMenuItem : Control
{
    //Порядковый номер элемента в меню
    public int Index = 0;
    
    private bool _cursorInItem = false;

    private bool CursorInItem {
        get { return _cursorInItem; }
        set {
            bool oldValue = _cursorInItem;
            _cursorInItem = value;
            if (oldValue != _cursorInItem) Update();
        }
    }

    private bool _pressed = false;
    private bool Pressed {
        get { return _pressed; }
        set {
            bool oldValue = _pressed;
            _pressed = value;
            if (oldValue != _pressed) Update();
        }
    }
    
    public string Title {
        get {
            PonyLabel label = GetNode("Title") as PonyLabel;
            return label.Text;
        }
        set {
            PonyLabel label = GetNode("Title") as PonyLabel;
            label.Text = value;

            GD.Print("[ContextMenuItem] ", label.Size.x);

            //Подгонка под размеры
            if (RectSize.x < label.Size.x + 9 * 2) RectSize = new Vector2(label.Size.x + 9 * 2, RectSize.y);
        }
    }

    [Signal]
    public delegate void ItemSelected(int i);

    public override void _Input(InputEvent e) {
        if (e is InputEventMouseMotion)
            CursorInItem = GetGlobalRect().HasPoint(GetGlobalMousePosition());
        if (e is InputEventMouseButton) {
            InputEventMouseButton em = e as InputEventMouseButton;
            if (em.ButtonIndex == 1) {
                Pressed = em.IsPressed();
                if (IsVisible() && !em.IsPressed() && CursorInItem) {
                    EmitSignal(nameof(ItemSelected), Index);
                    GD.Print("Click");
                }
            }
        }
    }

    public override void _Draw() {
        if (CursorInItem) {
            if (!Pressed) {
                DrawRect(new Rect2(0, 0, RectSize.x, RectSize.y), new Color(0.6235294f, 0.4705882f, 1, 1));
                DrawRect(new Rect2(3, 3, RectSize.x-3, RectSize.y-3), new Color(0.3843137f, 0, 0.8235294f, 1));
                DrawRect(new Rect2(3, 3, RectSize.x-6, RectSize.y-6), new Color(0.4666666f, 0, 1, 1));
            } else {
                DrawRect(new Rect2(0, 0, RectSize.x, RectSize.y), new Color(0.3843137f, 0, 0.8235294f, 1));
                DrawRect(new Rect2(3, 3, RectSize.x-3, RectSize.y-3), new Color(0.6235294f, 0.4705882f, 1, 1));
                DrawRect(new Rect2(3, 3, RectSize.x-6, RectSize.y-6), new Color(0.4666666f, 0, 1, 1));
            }
        }
    }

}
