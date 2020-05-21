using Godot;
using System;

//2019 © Даниил Белов
//Создано 07.07.2019

//TODO: Заблокировать одной и той же возможности, если она занята
///Скрипт действия сущности. Требует подключения сигнала окончания анимации от сущности к методу AnimationEnded()
public class EntityAction : Node
{
    protected bool _canBusy = false;

    ///Возможно ли запускать другие действия во время действия этого
    public bool CanBusy {
        get { return _canBusy; }
    }

    ///Время перезарядки в секундах
    protected float Recharge = 0;

    ///Время до конца перезарядки
    protected float RechargeWait = 0;

    ///Ссылка на сущность, к которой прикреплён этот скрипт. Используется для взаимодействия
    public Entity Slave;

    protected string[] AnimationNames = new string[0];

    protected bool _isRuning = false;

    public bool IsRuning {
        get { return _isRuning; }
    }

    //Сигналы
    [Signal]
    public delegate void ActionStarted(EntityAction action);

    [Signal]
    public delegate void ActionEnded(EntityAction action);

    ///Запуск действия
    public bool Run() {
        GD.Print("[EntityAction] Run action, ", RechargeWait);
        //Отклонить в случае перезарядки
        if (RechargeWait > 0) return false;
        //Отклонить в случае отсутсвия подключенной сущности
        if (Slave == null) return false;
        _isRuning = true;
        RunScript();
        EmitSignal(nameof(ActionStarted), this);
        return true;
    }

    ///Действия старта действия
    public virtual void RunScript() {
        if (AnimationNames != null && AnimationNames.Length > 0) {
            GD.Print("[EntityAction] Animation OK");
            Slave.RunAnimation(AnimationNames[0]);
        } else {
            End();
        }
    }

    ///По сигналу окончания анимаци
    public virtual void AnimationEnded(string name) {
        if (_isRuning)
            End();
    }

    public void Stop() {
        GD.Print("[EntityAction] Stop");
        End();
    }

    protected void End() {
        GD.Print("[EntityAction] End");
        _isRuning = false;
        RechargeWait = Recharge;
        EmitSignal(nameof(ActionEnded), this);
    }

    public override void _PhysicsProcess(float delta) {
        if (RechargeWait > 0)
            RechargeWait -= delta;

        if (RechargeWait < 0)
            RechargeWait = 0;
    }


    public EntityAction(string animationName, bool canBusy, float recharge = 0) {
        AnimationNames = new string[1] {animationName};
        _canBusy = canBusy; Recharge = recharge;
    }

    public EntityAction() {}

}
