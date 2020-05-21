using Godot;
using System;

//2019 © Даниил Белов
//Создано 07.08.2019

public class PortableHealthBar : Control
{
    protected bool _removeAfterTime = true;
    [Export]
    ///Удаляется ли через время (RemoveDelay)
    public bool RemoveAfterTime {
        get { return _removeAfterTime; }
        set {
            _removeAfterTime = value;
            RemoveWait = RemoveDelay;
            if (RemoveDelay == 0 && RemoveAfterTime)
                Remove(false);
        }
    }
    
    protected float _removeDelay = 3;
    [Export]
    ///Задержка перед удалением при включённом (RemoveAfterTime)
    public float RemoveDelay {
        get { return _removeDelay; }
        set {
            _removeDelay = value;
            RemoveWait = _removeDelay;
            if (RemoveDelay == 0 && RemoveAfterTime)
                Remove(false);
        }
    }

    ///Отсчёт до удаления
    protected float RemoveWait = 0;

    ///Cчётчик здоровья
    protected HealthBar HPBar {
        get { return GetNode("HealthBar") as HealthBar; }
    }

    ///Анимации
    protected AnimationPlayer Animations;

    ///Сущность, показатели здоровья которой будут показаны на счётчике
    public Entity ResearchEntity {
        get { return HPBar.Slave; }
        set { HPBar.Slave = value; }
    }

    public override void _Ready() {
        Animations = GetNode("Animations") as AnimationPlayer;
        ResetRemoveWait();
    }

    public override void _PhysicsProcess(float delta) {
        if (RemoveAfterTime && RemoveWait != 0) {
            if (RemoveWait > 0)
                RemoveWait -= delta;
            if (RemoveWait <= 0) {
                RemoveWait = 0;
                Remove(true);
            }
        }
    }

    ///Начинает отсёт до удаления сначала
    public void ResetRemoveWait() {
        RemoveWait = RemoveDelay;
    }

    ///Коректное удаление
    public void Remove(bool animated) {
        if (Animations == null || !animated) QueueFree();
        Animations.Play("Hide");
    }

    //Действия по окончании анимации (animName)
    protected void Animations_AnimationFinished(string animName) {
        if (animName == "Hide") 
            QueueFree();
    }

}
