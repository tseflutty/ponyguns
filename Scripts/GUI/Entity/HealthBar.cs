using Godot;
using System;

//2019 © Даниил Белов
//Создано 20.07.2019

public class HealthBar : ProcessBar
{   

    protected Entity _slave;

    public Entity Slave {
        get { return _slave; }
        set {
            if (_slave != null) {
                _slave.Disconnect("tree_exited", this, nameof(SlaveRemoved));
            }

            _slave = value;
            
            if (_slave != null) {
                _slave.Connect("tree_exited", this, nameof(SlaveRemoved));
                _slave.Connect(nameof(Entity.HealthChanged), this, nameof(Slave_HealthChanged));
                Value = Slave.Health;
                MaxValue = _slave.MaxHealth;
            }
        }
    }

    [Export]
    ///Ссылка на сущность для этого счётчик
    public NodePath _Slave_ = null;
    
    public override void _Ready()
    {
        base._Ready();
        if (_Slave_ != null)
            Slave = GetNode(_Slave_) as Entity;
    }

    protected void SlaveRemoved() { _slave = null; }

    protected virtual void Slave_HealthChanged() {
        Value = Slave.Health;
        MaxValue = _slave.MaxHealth;
    }
        
    
}
