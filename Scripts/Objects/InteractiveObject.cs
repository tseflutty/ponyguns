using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

//2019 © Даниил Белов
//Создано 26.07.2019

///Интерактивный объект.
///С такими объектами сущности могут взаимодействовать используя функцию Interact(), находясь около одного из таких.
///Игрок может взаимодействовать с такими объектами, нажав F, находясь около одного из таких
public class InteractiveObject : Spatial, RectSized
{
    ///!!! Не изменять из вне
    public static PonyNodeList Instances = new PonyNodeList();

    //Сигналы
    [Signal]
    public delegate void StartWaitInteractFrom(Entity e, InteractiveObject obj);

    [Signal]
    public delegate void StopWaitInteractFrom(Entity e, InteractiveObject obj);

    [Signal]
    public delegate void StartInteractFrom(Entity e, InteractiveObject obj);

    //Почти тоже самое, что и StartInteractFrom. Вызывается уже после занесения всех переменных на следующем цикле PhysicsProcess
    [Signal]
    public delegate void StartedInteractFrom(Entity e, InteractiveObject obj);

    [Signal]
    public delegate void StopInteractFrom(Entity e, InteractiveObject obj);

    [Signal]
    public delegate void InteractMessageSended(InteractMessage msg, InteractiveObject obj, Entity user);

    [Signal]
    public delegate void InteractActionReceivedFrom(int act, InteractiveObject obj, Entity from);


    //Прямоугольник объекта
    [Export]
    public float LeftRectOffset {get; set;}

    [Export]
    public float RightRectOffset {get; set;}

    [Export]
    public float FrontRectOffset {get; set;}

    [Export]
    public float BackRectOffset {get; set;}

    [Export]
    public float TopRectOffset {get; set;}

    [Export]
    public float BottomRectOffset {get; set;}

    [Export]
    public Vector3 ZeroOffset {get; set;}

    protected bool _enable = true;

    ///Если установлено True, с этим объектом можно взаиможействовать
    [Export]
    public bool Enable {
        get { return _enable; }
        set {
            _enable = value;
            if (!Enable) {
                DisconectAllWaits(); //Отключение всех ожидаемых
            }
        }
    }

    protected bool _canUserStopInteract = true;
    [Export]
    ///Может ли сущность самостоятельно прекратить взаимодействие с этим объектом
    public bool CanUserStopInteract {
        get { return _canUserStopInteract; }
        set {
            if (value != _canUserStopInteract) {
                _canUserStopInteract = value;
                LockStopInteract.Clear();
                if (!_canUserStopInteract)
                    foreach (Entity user in Users)
                        LockStopInteract.Add(user);
            }
        }
    }

    ///Зона взаимодействия
    protected Area InteractArea {
        get {
            if (!HasNode("InteractArea")) return null;
            return GetNode("InteractArea") as Area;
        }
    }
    
    ///Список сущностец, вщаимодействующих с этим объектом
    protected List<Entity> Users = new List<Entity>();

    protected int _maxUsers = 1;
    [Export]
    ///Максимальное количество сущностей, которые могут одновременно взаимодействовать с объектом
    public int MaxUsers {
        get { return _maxUsers; }
        set {
            _maxUsers = value;
            //TODO: отключать последних пользователей если значение уменьшается
        }
    }

    ///Список пользователей, для испускания сигнала StartedInteractFrom в близжайшем цикле PhysicsProcess
    ///Заполняется в _Interact(Entity user)
    ///Очищается в близжайшем цикле PhysicsProcess вместе с испусканием сигнала
    protected List<Entity> WaitToStartedSignal = new List<Entity>();
    
    ///Список пользователей, которым блокируется ручная остановка взаимодействия
    protected List<Entity> LockStopInteract = new List<Entity>();

    ///Высота предмета в 2х мерной проекции
    [Export]
    public float Heigth = 0;

    protected bool _allowWhiteList = false;
    ///Использовать ли белый список для проверки доступа дляпользователя
    public bool AllowWhiteList {
        get { return _allowWhiteList; }
        set {
            _allowWhiteList = value;
            //TODO
            //Сделать отключение неуказанных в белом списке пользователей
        }
    }

    //Для обработки изменений в списке
    protected String[] _oldWhiteList;
    [Export]
    ///Белый список. Указываються названия типов сущностей (Entity.TypeName)
    String[] WhiteList = new String[0];

    //Для обработки изменений в списке
    protected String[] _oldBlackList;
    [Export]
    ///Чёрный список. Указываються названия типов сущностей (Entity.TypeName)
    String[] BlackList = new String[0];

    ///Начать взаимодействие сущности (user) с этим объектом
    ///Возвращает True при успехе. Если достигнуто максимальное количество пользователей, то овзвращает False
    ///!!! Вызывается только от сущности
    ///TODO: Переписать на сигналы
    public bool _Interact(Entity user) {
        if (user.WaitingInteractFor != this)  return false;
        if (Users.Count >= _maxUsers)         return false;
        if (!HasInteractAccessFor(user))       return false;
        CancelWaitInteractFrom(user);
        Users.Add(user);
        DebugConsole.Shared.Output("[InteractObject] INTERACT");
        //Отключить все ожидания при достижении макс кол-ва пользователей
        if (Users.Count >= _maxUsers)   DisconectAllWaits();
        EmitSignal(nameof(StartInteractFrom), user, this);
        WaitToStartedSignal.Add(user);

        //Включать блокировку прекращения взаимодействия игрока если это включено
        if (!_canUserStopInteract)
            LockStopInteract.Add(user);

        //Подключение сигнала удаления
        Godot.Collections.Array bindsForUsrSignal = new Godot.Collections.Array();
        bindsForUsrSignal.Add(user);
        user.Connect("tree_exited", this, nameof(UserFreed), bindsForUsrSignal);

        return true;
    }

    ///Остановить взаисодействие сущности (user)
    public bool StopInteract(Entity user) {
        if (LockStopInteract.Contains(user)) LockStopInteract.Remove(user);
        GD.Print("---> ", user.InteractingWith);
        if (!Users.Contains(user) || user.InteractingWith != this)  return false;
        return user.StopInteract();
    }
    
    ///Для вызова от сущности
    ///Завершить взаимодействие сущности (user) с этим объектом
    ///Возвращает True при успехе. Если указанная сущность не взаимодействует с этим объектом, то возвращает False
    ///!!! Вызывается только от сущности
    ///TODO: Переписать на сигналы
    public bool _StopInteract(Entity user) {
        if (!Users.Contains(user))  return false;
        if (LockStopInteract.Contains(user)) return false;
        Users.Remove(user);
        EmitSignal(nameof(StopInteractFrom), user, this);
        user.Disconnect("tree_exited", this, nameof(UserFreed));
        return true;
    }

    ///Отключить ожидания взаимодействия от всех сущностей
    public void DisconectAllWaits() {
        Godot.Collections.Array bodies = InteractArea.GetOverlappingBodies();
        foreach (Godot.Object body in bodies) {
            if (!(body is Entity)) continue;
            Entity e = body as Entity;
            if (e.WaitingInteractFor == this) CancelWaitInteractFrom(e);
        }
    }

    ///Включение ожидания взаимодействия по вхождению сущности в зону взаимодействия
    protected void InteractArea_BodyEntered(Godot.Object body) {
        if (!(body is Entity) || Users.Count >= MaxUsers) return;
        Entity e = body as Entity;
        if (e.IsInteracting) return;
        _WaitInteractFrom(e);
    }


    protected void _WaitInteractFrom(Entity e) {
        if (!Enable)  return;
        if (!HasInteractAccessFor(e)) return;
        e.WaitInteractFor(this);
        EmitSignal(nameof(StartWaitInteractFrom), e, this);
    }
    protected void CancelWaitInteractFrom(Entity e) {
        e.CancelWaitInteract(this);
        EmitSignal(nameof(StopWaitInteractFrom), e, this);
    }

    ///Отключение ожиданий при выходе сущности из зоны взаимодействия
    protected void InteractArea_BodyExited(Godot.Object body) {
        if (!(body is Entity)) return;
        Entity e = body as Entity;
        if (!e.isModalWaitingInteract) return;
        CancelWaitInteractFrom(e);
    }

    ///Отправить сообщение взаимодействия (msg) для пользователя (user)
    ///Проверка наличия пользователя (user) в списке (Users)
    ///Возвращает успешность отправки
    protected bool SendInteractMessage(InteractMessage msg, Entity user) {
        if (!Users.Contains(user))  return false;
        EmitSignal(nameof(InteractMessageSended), msg, this, user);
        return true;
    }

    public override void _PhysicsProcess(float delta) {
        //Отключение пользователей при выключенном Enable
        if (!Enable && Users.Count > 0)
            while (Users.Count != 0)
                StopInteract(Users[0]);
        //Испускание сигнала StartedInteractFrom
        if (WaitToStartedSignal.Count > 0) {
            foreach (Entity user in WaitToStartedSignal)
                EmitSignal(nameof(StartedInteractFrom), user, this);
            WaitToStartedSignal.Clear();
        }

        ///Обработка изменений в белых и чёрных списках
        if (_oldBlackList != BlackList || _oldWhiteList != WhiteList) {
            foreach (Entity user in Users.ToArray()) {
                if (!HasInteractAccessFor(user))
                    StopInteract(user);
            }

            _oldBlackList = BlackList;  _oldWhiteList = WhiteList;
        }
    }

    ///Для отправки действия от сущности
    public virtual int _GetInteractActionFromEntity(Entity from, int interactAction) {
        EmitSignal(nameof(InteractActionReceivedFrom), interactAction, this, from);
        return 0;
    }


    ///Проверяет сущность (user) в чёрном и белом(Если включён (AllowWhiteList)) списках.
    ///Возвращает имеет ли сущность (user) право на взаимодействие с этим объектом
    public bool HasInteractAccessFor(Entity user) {
        if (user == null) return false;
        if (BlackList.Contains(user.TypeName) ||
           (AllowWhiteList && WhiteList.Contains(user.TypeName))) {
            return false;
        }

        return true;
    }

    protected void UserFreed(Entity user) {
        if (Users.Contains(user))
            StopInteract(user);
    }

    //Работа со списком всех экземпляров
    protected void TreeEntered() {
        if (!Instances.Contains(this))
            Instances.Add(this);
    }
    protected void TreeExited() {
        Instances.Remove(this);
    }

}

public enum InteractMessage {
    None,
    ReadyToUse,
    OK,
    No
}
