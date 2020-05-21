using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 15.07.2019

///Невраждебный ИИ. Просто блуждает по карте в случайных направлениях
///!!! Выходит из строя при смене раба во время игры
public class BaseMobAI : MobAI
{

    [Export]
    protected NodePath _SigthRayCast;
    
    protected RayCast SigthRayCast = null;


    [Export]
    protected NodePath _SightArea;

    private Area _sightArea = null;
    ///Предполагаемая область видимости
    protected Area SightArea {
        get { return _sightArea; }
        set {
            if (_sightArea != null) {
                _sightArea.Disconnect("body_entered", this, nameof(SigthArea_BodyEntered));
                _sightArea.Disconnect("body_exited", this, nameof(SigthArea_BodyExited));
            }
            _sightArea = value;
            _sightArea.Connect("body_entered", this, nameof(SigthArea_BodyEntered));
            _sightArea.Connect("body_exited", this, nameof(SigthArea_BodyExited));
        }
    }

    ///Объекты, которые сущность возможно видит
    protected List<Spatial> SigthAreaObjects = new List<Spatial>();

    [Export]
    ///Задержка смены движения вне режима атаки в секундах
    public float ChangeMovDirDelay = 8;

    [Export]
    ///Погрешность(рандомно) +- задержки смены движения вне режима атаки в секундах
    public float ChangeMovDirDelayRand = 3f;

    //Оставшееся время в секундах до рандомного смены направления
    protected float ChangeMovDirWait = 0;

    protected bool AllowRandomMovement = true;

    public override void _Ready() {
        if (_SigthRayCast != null) {
            Node n = GetNode(_SigthRayCast);
            if (n is RayCast) {
                RayCast rc = n as RayCast;
                SigthRayCast = rc;
            }
        }
        if (_SightArea != null) {
            Node n = GetNode(_SightArea);
            if (n is Area) {
                Area area = n as Area;
                SightArea = area;
            }
        }
        //TODO: сделать в сеттере раба и рейкаста
        if (Slave != null && SigthRayCast != null) {
            SigthRayCast.AddException(Slave);
        }
        base._Ready();
    }

    public override void _PhysicsProcess(float delta) {
        if (Slave == null) return;
        MovementControlProcess(delta);
        SightControlProcess(delta);
    }

    protected virtual void MovementControlProcess(float delta) {
        if (Slave == null) return;
        if (AllowRandomMovement) {
            ChangeMovDirWait -= delta;
            if (ChangeMovDirWait <= 0) {
                ChangeMovDirWait = ChangeMovDirDelay + ((float)PonyRandom.Rnd.NextDouble() * ChangeMovDirDelayRand - 0.5f) * 2;
                
                Vector2 newDir = new Vector2(
                    PonyRandom.Rnd.Next(-1, 2),
                    PonyRandom.Rnd.Next(-1, 2)
                );
                Vector3 newDir3 = new Vector3(newDir.x, 0, newDir.y);
                Slave.SetMovementDirection(newDir);
                Slave.SetSightDirectionTo(Slave.GetGlobalTransform().origin + newDir3);
            }
        }
    }

    protected virtual void SightControlProcess(float delta) {
        if (SigthRayCast != null && Slave.GetSightAngles() != null && Slave.GetSightAngles().Length > 0 && SightArea != null) {
            SigthRayCast.Rotation = Slave.GetSightAngles()[0];
            SigthRayCast.RotationDegrees += new Vector3(0, 90, 0);
            SightArea.Rotation = Slave.GetSightAngles()[0];
            SightArea.RotationDegrees += new Vector3(0, 90, 0);
        }
    }

    public void SigthArea_BodyEntered(Godot.Object body) {
        if (body is Spatial && !SigthAreaObjects.Contains(body as Spatial)) {
            SigthAreaObjects.Add(body as Spatial);
            //DebugConsole.Shared.Outpup("[Entity] Sees " + body);
        }
    }

    public void SigthArea_BodyExited(Godot.Object body) {
        if (body is Spatial && SigthAreaObjects.Contains(body as Spatial)) {
            SigthAreaObjects.Remove(body as Spatial);
            //DebugConsole.Shared.Outpup("[Entity] Not sees " + body);
        }
    }

    ///Возвращает объекты, которы попадают под предпологаемую зону видимости сущности
}
