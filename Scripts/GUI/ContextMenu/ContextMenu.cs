using Godot;
using System;

//2019 © Даниил Белов
//Создано 25.06.2019

public class ContextMenu : Control
{

    private string[] _items = new string[0];
    [Export]
    ///Список элементов списка
    public string[] Items {
        get { return _items; }
        set {
            _items = value;
            UpdateItems();
        }
    }

    private ContextMenuItem[] ItemObjects = new ContextMenuItem[0];

    private
     string _title = "";
    [Export]
    ///Заголовок (Отображается сверху)
    public string Title {
        get { return _title; }
        set {
            _title = value;
            if (TitleLabel != null) TitleLabel.Text = _title;
            //UpdateWidth();
        } 
    }

    [Export]
    ///Будет ли отображаться заголовок
    public bool ShowTitle = true;

    private PackedScene PackedItem;

    //Размер заголовка задаётся начиная с высшей точки и основной линии
    private PonyLabel TitleLabel;

    public override void _Ready() {
        PackedItem = ResourceLoader.Load("res://GUI/ContextMenu/ContextMenuItem.tscn") as PackedScene;
        TitleLabel = GetNode("Title") as PonyLabel;
        UpdateItems();
    }

    [Signal]
    public delegate int ItemSelected();

    public override void _Draw() {
        UpdateWidth();

        DrawRect(new Rect2(-3, -3, RectSize.x+6, RectSize.y+6), new Color(0.41176f, 0.34901f, 0.48235f, 1));
        DrawRect(new Rect2(0, 0, RectSize.x, RectSize.y), new Color(0, 0, 0, 1));

        //Рисование раздлителя, если включено отображения заголовка
        if (ShowTitle)
            DrawRect(new Rect2(3, TitleLabel.RectSize.y+9, RectSize.x-6, 3), 
                     new Color(0.168627f, 0.1372549f, 0.207843f, 1));
    }

    //Подгонка по ширине
    private void UpdateWidth() {
        float MaxWidth = (ShowTitle && TitleLabel != null) ? TitleLabel.Size.x + TitleLabel.RectPosition.x * 2 : 10;
        foreach (ContextMenuItem item in ItemObjects) {
            GD.Print(">> ", item.RectSize.x);
            if (item.RectSize.x > MaxWidth) MaxWidth = item.RectSize.x;
        }
        GD.Print("[ContextMenu] ", MaxWidth);
        //Установка ширины
        RectSize = new Vector2(MaxWidth, RectSize.y);
        foreach (ContextMenuItem item in ItemObjects)
            item.RectSize = new Vector2(MaxWidth, item.RectSize.y);
    }

    private void VisibilityChanged() {
        //Элементы скрываются вместе с меню для того, чтобы они не нажимались
        foreach (ContextMenuItem item in ItemObjects)
            item.Visible = Visible;
    }

    //Когда один из элементов был выбран
    private void ItemSelectedAtIndex(int i) {
        EmitSignal(nameof(ItemSelected), i);
        Hide();
    }

    public override void _Input(InputEvent e) {
        if ((e is InputEventMouseButton && !e.IsPressed() && !GetGlobalRect().HasPoint(GetGlobalMousePosition()) && Visible) || e is InputEventKey) {
            Hide();
        }
    }

    ///Показать меню в точке (point). Точка указывается в глобалной системе коорднат
    public void ShowIn(Vector2 point) {
        SetGlobalPosition(point);
        Show();
    }

    public void UpdateItems() {
        if (TitleLabel == null) return;
        TitleLabel.Visible = ShowTitle;
        TitleLabel.Text = Title;
        
        //Удаление старых элементов
        foreach (ContextMenuItem item in ItemObjects)
            item.QueueFree();

        ItemObjects = new ContextMenuItem[Items.Length]; 

        float menuHeight = TitleLabel.RectSize.y + 15; //Высота заголовка + Отступ перед элементами списка

        //Добавление элементов
        for (int i = 0; i < Items.Length; ++i) {
            ContextMenuItem newItem = PackedItem.Instance() as ContextMenuItem;
            newItem.RectSize = new Vector2(RectSize.x, newItem.RectSize.y);
            newItem.Title = Items[i];
            newItem.RectPosition = new Vector2(0, menuHeight);
            newItem.Index = i;
            newItem.Connect(nameof(ContextMenuItem.ItemSelected), this, nameof(ItemSelectedAtIndex));
            newItem.Visible = Visible;

            ItemObjects[i] = newItem;
            AddChild(newItem);
            menuHeight += newItem.RectSize.y;
        }
        RectSize = new Vector2(RectSize.x, menuHeight);

        Update();
    }
}
