using Godot;
using System;

public class Slime : Entity
{

    ///Задержка между прыжками при передвижении в секундах
    protected float JumpDelay = 0.6f;

    protected float JumpDelayRandom = 0.05f;

    ///Оставшееся время до следующего прыжка
    protected float JumpWait = 0;

    public override void _Ready() {
        JumpWait = (float)PonyRandom.Rnd.NextDouble() * JumpDelay;
        base._Ready();
    }

    public override void _PhysicsProcess(float delta) {
        base._PhysicsProcess(delta);

        if ((MovementDirection.x != 0 || MovementDirection.z != 0) && CanMovement && IsOnFloor()) {
            Jump();
        }
        
        if (!CanMovement && IsOnFloor()) {
            if (JumpWait > 0) {
                JumpWait -= delta;
            }
            if (JumpWait <= 0) {
                JumpWait = 0;
                CanMovement = true;
            }
        }
    }

    protected override void IsLanded() {
        CanMovement = false;
        JumpWait = JumpDelay + ((float)PonyRandom.Rnd.NextDouble() - 0.5f) * 2 * JumpDelayRandom;
    }

}
