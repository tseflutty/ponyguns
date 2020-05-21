using Godot;
using System;

//2019 © Даниил Белов
//Создано 13.05.2019


///<summary>
///<para>Основной объект арены</para>
///</summary>
public class Arena : Spatial
{

    private static Arena current;

    private Entity[] Players = new Entity[4];

    ///UID предметов, выдающиеся игрокам в начале игры
    [Export]
    private int[] StartPlayerItems = new int[0];

    public PonyCamera MainCamera;

    public EntityUserControl EntityPlayerControl;

    public ShotUserControl ShotPlayerControl;

    public override void _Ready() {
        MainCamera = GetNode("MainCamera") as PonyCamera;
        EntityPlayerControl = GetNode("EntityPlayerControl") as EntityUserControl;
        ShotPlayerControl = GetNode("ShotPlayerControl") as ShotUserControl;

        //Запись в static переменную для быстрого доступа от других объектов
        current = this;

        //Спаун игрока в стартовой точке
        Spatial startPoint = GetNode("StartPoint") as Spatial;
        PackedScene packedPony = ResourceLoader.Load("res://Objects/Entity/Pony/Unicorn.tscn") as PackedScene;
        Pony player = packedPony.Instance() as Pony;
        player.PlayerID = 1;
        player.Translation = startPoint.Translation;
        startPoint.QueueFree();
        AddChild(player);
        Players[0] = player;

        //Выдача игрокам стартовых предметов
        foreach (Entity p in Players) {
            if (p == null) continue;
            if (p.EntityInventory == null) continue;

            foreach (int itemUID in StartPlayerItems)
                p.EntityInventory.AddItem(Items.Get(itemUID));
        }

        //Установка игрока, в качестве цели для слежения основной камерой
        MainCamera.Pursued = player;

        //Настройка EntityUserControl для управления с клавиатуры/геймпада игроком
        EntityPlayerControl.SlaveEntity = player;

        //Настройка ShotUserControl для выстрелов игрока
        ShotPlayerControl.Slave = player;
        ShotPlayerControl.CurrentCamera = MainCamera;

        //Переммещение камеры вперёд по дереву после создания игроков
        MoveChild(MainCamera, GetChildCount());

        //Установка курсора-прицела
        CursorController.Sharred.SetCursorByIndex(1);
    }
    
    ///<summary>
    ///<para>Выпустить пулю (bullet) в напрвлении (direction) из точки (from)</para>
    ///</summary>  
    public void SpawnShot(Bullet bullet, Vector3 direction, Vector3 from, Spatial owner = null, int[] exceptions = null) {
        bullet.Translation = from;
        AddChild(bullet);
        bullet.Start(direction, owner, exceptions);
    }

    public void EntityShot(Entity entity, Bullet bullet, Vector3 direction, Vector3 offset) {
        bullet.BulletOwner = entity;
        SpawnShot(bullet, direction, entity.GetGlobalTransform().origin + offset, null, null);
    }

    ///Возвращает сущность игрока под номером (player). Отсчёт начинается с 1
    public Entity GetPlayer(byte player) {
        return Players[player - 1];
    }

    ///Возвращает массив сущностей игроков
    public Entity[] GetPlayers() {
        return Players;
    }

    ///Возвращает навигацию этой арены
    public Navigation GetNavigation() { return GetNode("Navigation") as Navigation; }

    ///<summary>
    ///<para>Возвращает текущую арену, которая загружена в игре</para>
    ///</summary>  
    public static Arena GetCurrent() { return current; }

}
