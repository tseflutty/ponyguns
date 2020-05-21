using Godot;
using System;
using System.Collections.Generic;

public class ButtonHint : Control
{
    protected AnimationPlayer Animations;

    protected TextureRect IconRect {
        get { return GetNode("IconRect") as TextureRect; }
    }

    protected string _buttonName = "E";

    [Export]
    public string ButtonName {
        get { return _buttonName; }
        set {
            _buttonName = value;
            UpdateIcon();
        }
    }

    protected bool _pressed = false;

    public bool Pressed {
        get { return _pressed; }
        set {
            _pressed = value;
            _pressed = value;
        }
    }

    protected void UpdateIcon() {
        if (Textures == null || PressedTextures == null) return;
        Dictionary<string, Texture> textures = (Pressed) ? PressedTextures : Textures;
        string buttonkey = ButtonName.ToLower();
        if (!textures.ContainsKey(buttonkey)) return;
        IconRect.Texture = textures[buttonkey];
    }

    //Текстуры иконок кнопок
    protected static Dictionary<string, Texture> Textures;
    //Текстуры иконок зажатых кнопок
    protected static Dictionary<string, Texture> PressedTextures;

    public override void _Ready() {
        Animations = GetNode("Animations") as AnimationPlayer;

        //TODO сделать загрузку текстур иконок по ini файлы
        if (Textures == null) {
            Textures = new Dictionary<string, Texture>();
            Textures.Add("e", ResourceLoader.Load<Texture>("res://Images/GUI/Buttons/ButtonE.png"));
            Textures.Add("f", ResourceLoader.Load<Texture>("res://Images/GUI/Buttons/ButtonF.png"));
        }
        if (PressedTextures == null) {
            PressedTextures = new Dictionary<string, Texture>();
            PressedTextures.Add("e", ResourceLoader.Load<Texture>("res://Images/GUI/Buttons/ButtonEPressed.png"));
            PressedTextures.Add("f", ResourceLoader.Load<Texture>("res://Images/GUI/Buttons/ButtonFPressed.png"));
        }
        UpdateIcon();
    }

    ///Удалить объект с анимацией
    public void Remove() {
        Animations.Play("Hide");
    }

    //Когда анимация закончилась
    protected void AnimationFinished(string animName) {
        //Удалить, после анимации скрытия
        if (animName == "Hide")
            QueueFree();
    }


}
