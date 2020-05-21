using Godot;
using System;

//2019 © Даниил Белов
//Создано 20.05.2019

///<summary>
///<para>Основной объект дальнобойного оружия. Содержит показатели и предназначен для хранения в инвентаре</para>
///</summary>
public class Gun : Item
{
    [Signal]
    public delegate void WhenShot(Vector3 direction, Spatial owner, int[] exceptions);
    
    public string PathToBullet;

    ///<summary>
    ///<para>Сколько патронов вмещает один магазин</para>
    ///</summary>
    public int BulletsInOneShop =  5;

    ///<summary>
    ///<para>Количество магазинов</para>
    ///</summary>
    public int ShopsCount = 10;

    ///<summary>
    ///<para>Количество патронов в текущем магащине</para>
    ///</summary>
    public int BulletsInShop = 5;

    ///Минимальная задержка в секундах между выстрелами
    public float DelayBetweenShots;
    private float WaitToNextShot = 0;

    public bool Shot(Vector3 direction, Spatial owner = null, int[] exceptions = null) {
        //Временное отключений ограничения по патронам
        // if (ShopsCount <= 0 && BulletsInShop <= 0) return false;

        // BulletsInShop--;
        // if (BulletsInShop <= 0 && ShopsCount > 0) {
        //     --ShopsCount;
        //     BulletsInShop = BulletsInOneShop;
        // }
        if (WaitToNextShot <= 0) {
            //GD.Print("[Gun] Shot");
            EmitSignal(nameof(WhenShot), direction, owner, exceptions);
            WaitToNextShot = DelayBetweenShots;
        }

        return true;
    }

    public override void _PhysicsProcess(float delta) {
        if (WaitToNextShot > 0) WaitToNextShot -= delta;
        if (WaitToNextShot < 0) WaitToNextShot = 0;
    }
    
    public Gun(string iName, string iPathToIcon, int iUID, string iPathToBullet, float iDelayBetweenShots = 0.1f, 
    int[] itemTags = null) : base(iName, iPathToIcon, iUID, itemTags) {
        PathToBullet = iPathToBullet; DelayBetweenShots = iDelayBetweenShots;
        PathToVisualItem = "res://Objects/VisualItem/Weapons/VisualGuns/VisualGun.tscn";
    }

    public Gun() {
        ItemName = "Пушечка"; PathToIcon = ""; UID = 0; PathToBullet = "";
        DelayBetweenShots = 0.1f;
        PathToVisualItem = "res://Objects/VisualItem/Weapons/VisualGuns/VisualGun.tscn";
    }
}
