using Godot;
using System;

//2019 © Даниил Белов
//Создано 16.05.2019

public class Unicorn : Pony
{
    //Позиции для плавного перемещения предметов, кодорые удерживаются магией рога
    private float[] CornItemPositions = new float[2];
    private float[] CornItemCurrentPositions = new float[2];

    ///Контейнеры для ItemPlace удержания рогом
    private Spatial[] CornItems = new Spatial[2];

    ///Для анимации вкл/выкл облкак магии рога и 2х предметов
    private bool isCornMagicShowed = false;
    private bool[] isCornMagicShowedItems = new bool[2] {false, false};

    protected Sprite3D CornMagic {
        get { 
            if (!HasNode("EntityTexture/CornMagic")) return null;
            return GetNode("EntityTexture/CornMagic") as Sprite3D;
        }
    }

    protected Color _cornMagicColor = new Color(1,0,0);
    [Export]
    public Color CornMagicColor {
        get {
            Sprite3D cm = CornMagic;
            if (cm == null) return new Color(1,0,0);
            return cm.Modulate;
        }
        set {
            if (CornMagic != null)
                CornMagic.Modulate = value;
            foreach (ItemPlace[] places in ItemPlaces)
                foreach (ItemPlace place in places)
                    if (place != null && place is CornMagicPlace)
                        (place as CornMagicPlace).CloudColor = value;
            _cornMagicColor = value;
        }
    }

    ///Плеер с анимациями для показа/скрытия облачка магии рога и 2х предметов
    private AnimationPlayer OnOffCornAnimations;
    private AnimationPlayer CornAnimations;

    public override void _Ready()
    {
        base._Ready();
        
        EntityInventory = new AdvancedInventory("Единорог", new int[2] {2, 1}, new string[2] {"Телекинез", "Во рту"}, 0, 0);
        EntityInventory.SetSlotTags(0, 0, new int[1] {1});

        ItemPlaces.Add(new ItemPlace[2] {
            GetNode("CornItem1/ItPlCorn1") as ItemPlace,
            GetNode("CornItem2/ItPlCorn2") as ItemPlace
        });
        ItemPlaces.Add(new ItemPlace[1] {
            GetNode("EntityTexture/ItPlInMouth") as ItemPlace
        });
        
        //!!! Выдаст ошибку если у главного слота не будет визуального воплощения
        ItemPlace MainItemPlace = ItemPlaces[EntityInventory.MainSlotGroup][EntityInventory.MainSlot];
        if (MainItemPlace != null && MainItemPlace is CornMagicPlace) {
            (MainItemPlace as CornMagicPlace).IsMain = true;
        }

        CornItems[0] = GetNode("CornItem1") as Spatial;
        CornItemPositions[0] = 2.623f;
        CornItems[1] = GetNode("CornItem2") as Spatial;
        CornItemPositions[1] = 1.982f;

        OnOffCornAnimations = GetNode("OnOffCornAnimations") as AnimationPlayer;
        CornAnimations = GetNode("CornAnimations") as AnimationPlayer;

        EntityInventory.Connect(nameof(Inventory.ItemAdded), this, nameof(UpdateCornMagic));
        EntityInventory.Connect(nameof(Inventory.ItemRemoved), this, nameof(UpdateCornMagic));

        ///Установка цвета магии рога
        CornMagic.Modulate = _cornMagicColor;
        foreach (ItemPlace[] places in ItemPlaces)
                foreach (ItemPlace place in places)
                    if (place != null && place is CornMagicPlace)
                        (place as CornMagicPlace).CloudColor = _cornMagicColor;
    }

    public override void _PhysicsProcess(float delta) {
        base._PhysicsProcess(delta);

        //Плавное перемещение предметов, которые удерживаются магией рога
        for (int i = 0; i < CornItemPositions.Length; ++i) {
            CornItemCurrentPositions[i] = (AnimationDirection == 0) ? -CornItemPositions[i] : CornItemPositions[i];
            CornItems[i].Translation = new Vector3(CornItems[i].Translation.x + (CornItemCurrentPositions[i] - CornItems[i].Translation.x) / 8 * delta * DELTA_MUL,
                                                   CornItems[i].Translation.y, CornItems[i].Translation.z);
        }
        
    }

    public void UpdateCornMagic(int group, int slot) {
        if (EntityInventory.GetItemAt(EntityInventory.MainSlotGroup, EntityInventory.MainSlotGroup) != null || EntityInventory.GetItemAt(0, 1) != null) {
            if (!isCornMagicShowed) {
                isCornMagicShowed = true;
                OnOffCornAnimations.Play("On");
            }
        } else {
            if (isCornMagicShowed) {
                isCornMagicShowed = false;
                CornAnimations.Stop();
                OnOffCornAnimations.Play("Off");
            }
        }
    }

    protected void OnOffCornAnimations_AnimationFinished(string animName) {
        if (animName == "On")
            CornAnimations.Play("CornMagic");
    }

    public override void ShotInDirection(Vector3 direction) {
        if (ShotExceptions != null)
            ShotExceptions = null;
    }

    public override void Shot(Vector3 point, Spatial target = null) {
        if (target != this)
            ShotExceptions = new int[1] {2};
        else
            ShotExceptions = null;
        
        base.Shot(point);
    }


}
