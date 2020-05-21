using Godot;
using System;

//2019 © Даниил Белов
//Создано 29.05.2019

///Кастомный progress bar
public class ProcessBar : Control
{
    Texture[] Textures = new Texture[10];

    private Color _barColor = new Color(1, 1, 1, 1);
    [Export]
    ///Цвет счётчика
    public Color BarColor {
        get { return _barColor; }
        set {
            _barColor = value;
            Update();
        }
    }
    private float _maxValue = 100;
    [Export]
    ///Максимальное значение. Это число я вляется 100% заполнения
    public float MaxValue {
        get { return _maxValue; }
        set {
            _maxValue = value;
            Update();
        }
    }

    ///Текущее значение
    private float _value = 0;
    [Export]
    public float Value {
        get { return _value; }
        set {
            _value = value;
            Update();
        }
    }

    public override void _Ready() {
        Textures[0] = ResourceLoader.Load("res://Images/GUI/ProcessBar/ProcessBarBackLeft.png") as Texture;
        Textures[1] = ResourceLoader.Load("res://Images/GUI/ProcessBar/ProcessBarBackMid.png") as Texture;
        Textures[2] = ResourceLoader.Load("res://Images/GUI/ProcessBar/ProcessBarBackRight.png") as Texture;
        Textures[3] = ResourceLoader.Load("res://Images/GUI/ProcessBar/ProcessBarFillLeft.png") as Texture;
        Textures[4] = ResourceLoader.Load("res://Images/GUI/ProcessBar/ProcessBarFillLine.png") as Texture;
        Textures[5] = ResourceLoader.Load("res://Images/GUI/ProcessBar/ProcessBarFillMid.png") as Texture;
        Textures[6] = ResourceLoader.Load("res://Images/GUI/ProcessBar/ProcessBarFillRight.png") as Texture;
        Textures[7] = ResourceLoader.Load("res://Images/GUI/ProcessBar/ProcessBarLineLeft.png") as Texture;
        Textures[8] = ResourceLoader.Load("res://Images/GUI/ProcessBar/ProcessBarLineMid.png") as Texture;
        Textures[9] = ResourceLoader.Load("res://Images/GUI/ProcessBar/ProcessBarLineRight.png") as Texture;
    }

    public override void _Draw() {
        //Белый контур
        DrawTexture(Textures[7], new Vector2(0, 0));
        DrawTextureRect(Textures[8], new Rect2(24, 0, RectSize.x - 24 * 2, 27), true);
        DrawTexture(Textures[9], new Vector2(RectSize.x - 24, 0));
        //Подложка
        DrawTexture(Textures[0], new Vector2(3, 0), BarColor);
        DrawTextureRect(Textures[1], new Rect2(24, 0, RectSize.x - 24 * 2, 27), true, BarColor);
        DrawTexture(Textures[2], new Vector2(RectSize.x - 24, 0), BarColor);
        //Заполнение
        DrawTexture(Textures[3], new Vector2(12, 6), BarColor);
        DrawTextureRect(Textures[5], new Rect2(24, 6, (RectSize.x - 24 * 2) * (Value / MaxValue), 15), true, BarColor);
        DrawTexture(Textures[6], new Vector2((RectSize.x - 24 * 2) * (Value / MaxValue) + 24, 6), BarColor);
        DrawTextureRect(Textures[4], new Rect2(18, 6, (RectSize.x - 24 * 2) * (Value / MaxValue) + 6, 15), true, BarColor);
    }
}
