using Godot;
using System;

//2019 © Даниил Белов
//Создано 07.07.2019

public class CornMagicPlace : ItemPlace
{

    public override VisualItem ItemOfPlace {
        get { return base.ItemOfPlace; }
        set {
            base.ItemOfPlace = value;

            if (value != null) {
                int[] ItemTags = value.GetItem().ItemTags;
                if (ItemTags != null) {
                    //Изменение текстуры облачка магии в зависимости от тегов предмета
                    if (Array.IndexOf(ItemTags, (int)ItemTag.HasGrip) >= 0 && IsMain) {
                        if (Array.IndexOf(ItemTags, (int)ItemTag.Small) >= 0) {
                            MagivCloudAnimations.Play(CloudAnimations[1]);
                        } else {
                            MagivCloudAnimations.Play(CloudAnimations[0]);
                        }
                    } else {
                        MagivCloudAnimations.Play(CloudAnimations[0]);
                    }
                } else {
                    MagivCloudAnimations.Play(CloudAnimations[0]);
                }
            }
        }
    }

    protected Sprite3D MagicCloud {
        get {
            if (!HasNode("MagicCloud")) return null;
            return GetNode("MagicCloud") as Sprite3D;
        }
    }
    
    protected Color _cloudColor;
    public Color CloudColor {
        get { 
            if (MagicCloud == null) return new Color(1,0,0);
            return MagicCloud.Modulate;
        }
        set {
            _cloudColor = value;
            if (MagicCloud != null) MagicCloud.Modulate = value;
        }
    }
    
    public bool IsMain = false;

    protected bool MagicCloudShowed = false;

    protected AnimationPlayer Animations {
        get { return GetNode("Animations") as AnimationPlayer; }
    }

    protected AnimationPlayer MagivCloudAnimations {
        get { return GetNode("CloudAnimations") as AnimationPlayer; }
    }

    protected Sprite3D MagicCloudSprite {
        get { return GetNode("MagicCloud") as Sprite3D; }
    }

    protected static string[] CloudAnimations;

    public override void _Ready() {
        if (CloudAnimations == null) {
            CloudAnimations = new string[2];
            CloudAnimations[0] = "Item";
            CloudAnimations[1] = "SmallWeapon";
        }

        //Установка цвета облочка
        MagicCloud.Modulate = _cloudColor;
    }
    public override void _PhysicsProcess(float delta) {
        base._PhysicsProcess(delta);

        if (ItemOfPlace == null) {
                if (MagicCloudShowed) {
                    Animations.PlayBackwards("Show");
                    MagicCloudShowed = false;
                }
        } else {
            if (!MagicCloudShowed) {
                Animations.Play("Show");
                MagicCloudShowed = true;
            }
        }
    }

}
