using Godot;
using System;

//2019 © Даниил Белов
//Создано 13.05.2019

///<summary>
///<para>Основной объект выпускаемой на арену пули</para>
///</summary>  
public class Bullet : Spatial
{

    ///<summary>
    ///<para>Здесь указан тот, кто выпустил эту пулю, если это надо</para>
    ///</summary>   
    public Spatial BulletOwner {
        get { return _AtackArea.AreaOwner; }
        set { _AtackArea.AreaOwner = value; }
    }

    private Vector3 _direction;
    ///<summary>
    ///<para>Направление, по которому летит пуля</para>
    ///</summary>   
    public Vector3 Direction {
        get { return _direction; }
        set {
            _direction = value.Normalized() * Speed;
        }
    }

    //На сколько умножаются милисекунды между кадрами при обработке в процессе
    const float DELTA_MUL = 30;

    private float _speed = 2;
    [Export]
    ///<summary>
    ///<para>Скорость, с которой летит пуля</para>
    ///</summary>
    public float Speed  {
        get { return _speed; }
        set {
            Direction /= _speed;
            _speed = value;
            Direction *= _speed;
        }
    }

    [Export]
    ///<summary>
    ///<para>Урон, который наносит пуля, вонзаясь в жертву</para>
    ///</summary>
    public int Damage {
        get {
            if (!HasNode("BodyEntered")) return 1;
            AtackArea aa = GetNode("AtackArea") as AtackArea;
            return aa.Damage;
        }
        set {
            AtackArea aa = GetNode("AtackArea") as AtackArea;
            aa.Damage = value;
        }
    }

    protected AtackArea _AtackArea {
        get { return GetNode("AtackArea") as AtackArea; }
    }

    private bool isStarted = false;

    ///<summary>
    ///<para>Запустить пулю в направлении (direction)</para>
    ///</summary>
    public void Start(Vector3 direction, Spatial owner, int[] exceptions = null) {
        if (exceptions != null)
            _AtackArea.TagExceptions = exceptions;
        if (owner != null)
            BulletOwner = owner;
        Direction = direction;
        isStarted = true;
    }

    public override void _PhysicsProcess(float delta) {
        if (isStarted)
            Translate(Direction * delta * DELTA_MUL);
    }

    private void BodyEntered(Godot.Object body) {
        if (!(body is Entity)) {
            QueueFree();
        }
    }

    private void AreaAtacked() {
        QueueFree();
    }

}
