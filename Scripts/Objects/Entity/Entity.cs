using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 18.04.2019

///<summary>
///<para>Основной объект сущности</para>
///</summary>
public class Entity : KinematicBody, RectSized
{
    ///!!! Не изменять из вне
    public static PonyNodeList Instances = new PonyNodeList();

    [Signal]
    public delegate void EntityInjured(float count, Spatial damager);

    [Signal]
    public delegate void HealthChanged();

    [Signal]
    public delegate void EntityDied();

    [Signal]
    public delegate void InteractAction(int act);

    [Signal]
    public delegate void WaitingInteractForChanged(InteractiveObject to);
    
    [Signal]
    public delegate void InventoryChanged();



    //Прямоугольник объекта
    public float LeftRectOffset {get; set;}

    public float RightRectOffset {get; set;}

    public float FrontRectOffset {get; set;}

    public float BackRectOffset {get; set;}

    public float TopRectOffset {get; set;}

    public float BottomRectOffset {get; set;}

    public Vector3 ZeroOffset {get; set;}
    


    [Export]
    ///Неотображаемое название вида
    protected string _typeName = "Entity";

    public string TypeName {
        get { return _typeName; }
    }

    protected Sprite3D EntityTexture;

    ///Номер игрока этой сущности. Счёт начинается с 1. Если равен 0 – игроком не является
    public byte PlayerID = 0;
    
    ///Возвращает является ли данная сущность игроком
    public bool IsPlayer {
        get { return PlayerID != 0; }
    }
    
    //Здоровье сущности
    private int _health = 100;
    [Export]
    public int Health {
        get { return _health; }
        set {
            int old = _health;
            _health = value;
            //Убить если нет здоровья
            if (_health <= 0) Kill();
            if (old != _health)
                EmitSignal(nameof(HealthChanged));
        }
    }

    [Export]
    ///<summary>
    ///<para>Значение здоровья, выше которого сущность не может быть вылечена</para>
    ///</summary>
    public int MaxHealth = 100;

    [Export]
    ///Путь до сцены с анимацией смерти
    public string PathToDeadAnimation = "";

    ///<summary>
    ///<para>Сила гравитации, действующая на сущность</para>
    ///</summary>
    public float Gravity = 2;
    
    //На сколько умножаются милисекунды между кадрами при обработке в процессе
    protected const float DELTA_MUL = 65;


    [Export]
    ///<summary>
    ///<para>Установлено True если сущность имеет возможность прыгнуть</para>
    ///</summary>
    public bool CanJump = true;


    private bool _canFly = false;
    [Export]
    ///<summary>
    ///<para>Установлено True если сущность может летать</para>
    ///</summary>
    public bool CanFly {
        get { return _canFly; }
        set {
            _canFly = value;
            if (!_canFly) IsFly = false; 
        }
    }


    private bool _isFly = false;
    ///<summary>
    ///<para>Установлено True если сущность находится в полёте</para>
    ///</summary>
    public bool IsFly {
        get { return _isFly; }
        set {
            if (!CanFly)
                _isFly = false;
            else {
                _isFly = value;
                if (_isFly) {
                    FloorHeight = GetGlobalTransform().origin.y;
                    TakeoffTo(5);
                }
            }
            UpdateMovementSpeed();
        }
    }

    [Export]
    ///<summary>
    ///<para>Скорость изменения высоты</para>
    ///</summary>
    public float TakeoffSpeed = 20;

    ///<summary>
    ///<para>Высота пола</para>
    ///</summary>
    private float FloorHeight = 0;


    private float _flyHeight = 0;
    ///<summary>
    ///<para>Высота полёта сущности относительно высоты поля</para>
    ///</summary>
    public float FlyHeight {
        get { return _flyHeight; }
        set {
            _flyHeight = value;
            GlobalFlyHeight = FloorHeight + _flyHeight;
        }
    }

    [Export]
    ///<summary>
    ///<para>Максимальная высота полёта сущности</para>
    ///</summary>
    public float MaxFlyHeight = 10;

    ///<summary>
    ///<para>Высота полёта сущности в глобальной системе координат</para>
    ///</summary>
    private float GlobalFlyHeight = 0;

    ///<summary>
    ///<para>Запрос на изменение высоты в следующей обработке PhysicsProcess.false Указана интенсивность (где 1 - ровно TakeoffSpeed).</para>
    ///<para>Запрос отстуствует если указано 0</para>
    ///</summary>
    private float ChangeFlyHeightRequest = 0;

    ///<summary>
    ///<para>Установлено True если сущность достигает высоты по ключу</para>
    ///</summary>
    private bool IsToKeyProcess = false;

    ///<summary>
    ///<para>Ключ цели высоты, до которой сущность добирается при активном IsToKeyProcess</para>
    ///</summary>
    private float ToFlyHeightKey;

    private float _flySpeed = 8;
    [Export]
    ///<summary>
    ///<para>Скорость полёта сущности</para>
    ///</summary>
    public float FlySpeed {
        get { return _flySpeed; }
        set {
            _flySpeed = value;
            UpdateMovementSpeed();
        }
    }


    [Export]
    ///<summary>
    ///<para>Сила прыжка</para>
    ///</summary>
    public float JumpStrength = 40;

    ///<summary>
    ///<para>Направление по которому передвигается сущность</para>
    ///</summary>
    protected Vector3 MovementDirection = new Vector3(0, 0, 0);

    Vector3 NormalFloor = new Vector3(0, 1, 0);

    private float _movementSpeed = 5;
    [Export]
    ///<summary>
    ///<para>Скорость передвижения сущности</para>
    ///</summary>
    public float MovementSpeed {
        get { return _movementSpeed; }
        set {
            _movementSpeed = value;
            UpdateMovementSpeed();
        }
    }

    protected bool CanMovement = true;

    ///Служит для определения приземления
    private bool OnFloor = true;

    ///Длительность режима атаки в секундах
    [Export]
    public float AtackModeDuration = 2;

    //Отсчёт до окончания режима атаки
    protected float AtackModeWait = 0;
    protected bool AtackMode = false;
    protected Vector3 AtackModeTarget;

    private EntityAction[] _actions;
    protected EntityAction[] Actions {
        get { return _actions; }
        set {
            if (_actions != null)
                foreach (EntityAction act in _actions)
                    act.QueueFree();

            _actions = value;
            IsActionRuning = false;
            isBusy = false;

            AnimationPlayer animations = GetNode("Animations") as AnimationPlayer;
            
            foreach (EntityAction act in _actions) {
                AddChild(act);
                act.Connect(nameof(EntityAction.ActionEnded), this, nameof(ActionEnded));
                act.Slave = this;
                animations.Connect("animation_finished", act, nameof(EntityAction.AnimationEnded));
            } 
        }
    }

    protected bool IsActionRuning;

    ///Индекс текущего запущенного действия
    protected int RuningActionId = 0;

    protected bool isBusy;

    protected AnimationPlayer DamageAnimations;

    ///<summary>
    ///<para>Используемый тип пули в данный момент</para>
    ///</summary>
    public PackedScene CurrentBullet;

    ///Теги исключения для стрельбы
    public int[] ShotExceptions = null;


    private Inventory _entityInventory;
    ///<summary>
    ///<para>Инвентарь у сущности</para>
    ///</summary>
    public Inventory EntityInventory {
        get { return _entityInventory; }
        set {
            if (_entityInventory != null) {
                RemoveChild(_entityInventory);
                _entityInventory.QueueFree();
            }

            if (_entityInventory != value) {
                EmitSignal(nameof(InventoryChanged));
            }

            _entityInventory = value; 
            AddChild(_entityInventory);

            //Подключение сигналов
            _entityInventory.Connect(nameof(Inventory.ItemAdded), this, nameof(Inventory_ItemAdded));
            _entityInventory.Connect(nameof(Inventory.ItemRemoved), this, nameof(Inventory_ItemRemoved));
        }
    }

    protected List<ItemPlace[]> ItemPlaces = new List<ItemPlace[]>();

    //Сущности, находящиеся под этой сущности
    protected List<Entity> EntitiesBellow = new List<Entity>();


    [Export]
    ///Ссылки на объекты глаз сущности (Обязательный тип объекта глаза: Eye)
    protected String[] EntityEyes = new String[0];

    ///Глаза сущности
    protected Eye[] Eyes;

    //На сколько делиться результат взгляда по X. Служит для фикса смещение взгляда от цели
    [Export]
    public float SightHDis = 1;

    //Направление смещения с сущности под этой сущности
    protected Vector3 BellowOffseting = new Vector3(0, 0, 0);

    ///Направления взглядов в градусах поворота. PI*2 = 360
    protected Vector3[] SightAngles;

    ///Количество направлений взгляда
    [Export]
    public int SightsCount {
        get { 
            if (SightAngles == null) return 1;
            return SightAngles.Length;
        }
        set {
            SightAngles = new Vector3[value];
        }
    }

    private AtackListener[] _atackListeners;
    ///Подключенные приёмщикики атак
    protected AtackListener[] AtackListeners {
        get { return _atackListeners; }
        set {
            if (_atackListeners != null)
                foreach (AtackListener listener in _atackListeners)
                    listener.Disconnect(nameof(AtackListener.AtackListened), this, nameof(AtackListened));
            
            _atackListeners = value;
            
            foreach (AtackListener listener in _atackListeners)
                listener.Connect(nameof(AtackListener.AtackListened), this, nameof(AtackListened));
        }
    }

    //Для инспектора
    [Export]
    protected string[] _AtackListeners = new string[0];

    private AtackArea[] _atackAreas;
    ///Подключенные зоны атак
    protected AtackArea[] AtackAreas {
        get { return _atackAreas; }
        set {
            if (_atackAreas != null)
                foreach (AtackArea area in _atackAreas)
                    if (area.AreaOwner == this)
                        area.AreaOwner = null;
            
            _atackAreas = value;
            
            foreach (AtackArea area in _atackAreas)
                area.AreaOwner = this;
        }
    }

    //Для инспектора
    [Export]
    protected string[] _AtackAreas = new string[0];

    [Export]
    ///Высота сущности в двухмерной проекции
    public float Height = 0;

    ///<summary>
    ///<para>Убить сущность</para>
    ///</summary>
    public void Kill() {
        try {
            Spatial deadAnimation = ResourceLoader.Load<PackedScene>(PathToDeadAnimation).Instance() as Spatial;
            if (Arena.GetCurrent() != null) {
                deadAnimation.Translation = GetGlobalTransform().origin - Arena.GetCurrent().Translation;
                Arena.GetCurrent().AddChild(deadAnimation);
            }
        } catch {
            OS.Alert("Invalid death animation", "Error");
        }
        EmitSignal(nameof(EntityDied));
        QueueFree();
    }


    ///<summary>
    ///<para>Ранить сущность на (damage) здоровья</para>
    ///</summary>
    public void Injure(int damage, Spatial damager = null) {
        Health -= damage;
        DamageAnimations.Play("Damage");
        EmitSignal(nameof(EntityInjured), damage, damager);
    }


    ///<summary>
    ///<para>Вылечить сущность на (count) здоровья</para>
    ///</summary>
    public void Heal(int count) {
        int oldHealth = Health;
        Health += count;
        if (oldHealth > MaxHealth)
            Health = MaxHealth;
    }

    public int _animationDirection = (int) EntityStayDirection.left; 
    ///<summary>
    ///<para>Указывает какого направления анимация должна приогрваться</para>
    ///</summary>
    public int AnimationDirection {
        get { return _animationDirection; }
        set {
            _animationDirection = value;
        }
    }

    protected List<InteractiveObject> _WaitingInteractFor = new List<InteractiveObject>();
    ///Приоритетный ожидающий интерактивный объект
    public InteractiveObject WaitingInteractFor {
        get {
            if (_WaitingInteractFor.Count == 0)
                return null;
            return _WaitingInteractFor[0];
        }
    }

    protected InteractiveObject OldWaitingInteractFor = null;

    ///Указывает на активность модального ожидания взаимодействия с объектом
    public bool isModalWaitingInteract {
        get { return (WaitingInteractFor != null); }
    }

    protected InteractiveObject _InteractingWith = null;

    public InteractiveObject InteractingWith {
        get { return _InteractingWith; }
    } 

    public bool IsInteracting {
        get { return _InteractingWith != null; }
    }


    ///<summary>
    ///<para>Возвращает значение типа Vector3, которое обозначает направление по которому сущность сейчасть движется</para>
    ///</summary>
    public Vector3 GetMovementDirection() {
        return MovementDirection;
    }

    //Установить направление движения по x (value.x) и z (value.y);
    public void SetMovementDirection(Vector2 value) {
        MovementDirection = new Vector3(value.x, MovementDirection.y, value.y).Normalized();
        UpdateMovementSpeed();
    }

    ///<summary>
    ///<para>Начать прыжок чущности</para>
    ///</summary>
    public void Jump() {
        //Блокировать, если сущность взаимодействует с каким либо объектом
        if (IsInteracting) return;

        if (IsOnFloor() && CanJump)
            MovementDirection.y = JumpStrength;
    }

    ///<summary>
    ///<para>Начать передвижении сущности по осям x(direction.x), z(direction.y)</para>
    ///</summary>
    public void StartMovement(Vector2 direction) {
        //Блокировать, если сущность взаимодействует с каким либо объектом
        if (IsInteracting) return;

        Vector2 normalizedDir = direction.Normalized();
        if (IsFly) {
            MovementDirection.x = normalizedDir.x * FlySpeed;
            MovementDirection.z = normalizedDir.y * FlySpeed;
            return;
        }
        MovementDirection.x = normalizedDir.x * MovementSpeed;
        MovementDirection.z = normalizedDir.y * MovementSpeed;
    }

    ///<summary>
    ///<para>Обновляет скорость начатого передвижения. Используется при обновлении переменных, от которых зависит скорость передвижения по координатам x и z</para>
    ///</summary>
    private void UpdateMovementSpeed() {
        StartMovement(new Vector2(MovementDirection.x, MovementDirection.z));
    }

    ///<summary>
    ///<para>Остановит передвижении сущности по осям x и z</para>
    ///</summary>
    public void StopMovement() {
        MovementDirection.x = 0;
        MovementDirection.z = 0;
    }


    ///<summary>
    ///<para>Запустить анимацию под названием (name). Защищает от цикличного запуска одной и той же анимации.false Защищает от ошибки несуществующей анимации</para>
    ///</summary>
    public void RunAnimation(string name) {
        AnimationPlayer animations = GetNode("Animations") as AnimationPlayer;
        if (animations.GetCurrentAnimation() != name && animations.HasAnimation(name))
            animations.Play(name);
    }

    public override void _Ready() {
        //Установка плеера анимаций урона
        DamageAnimations = GetNode("DamageAnimations") as AnimationPlayer;

        EntityTexture = GetNode("EntityTexture") as Sprite3D;

        //Установка глаз из путей, указанных в инспекторе
        List<Eye> tempEyes = new List<Eye>();
        foreach (string eyePath in EntityEyes) {
            Node eyeObj = GetNode(eyePath);
            if (eyeObj is Eye) {
                Eye eye = eyeObj as Eye;
                tempEyes.Add(eye);
            }
        }
        Eyes = new Eye[tempEyes.Count];
        for (int i = 0; i < tempEyes.Count; ++i)
            Eyes[i] = tempEyes[i];

        //Установка приёмщиков атак из путей, указанных в инспекторе
        List<AtackListener> tempAL = new List<AtackListener>();
        foreach (string ALPath in _AtackListeners) {
            Node ALObj = GetNode(ALPath);
            if (ALObj is AtackListener) {
                AtackListener al = ALObj as AtackListener;
                tempAL.Add(al);
            }
        }
        if (tempAL.Count > 0) {
            AtackListener[] als = new AtackListener[tempAL.Count];
            for (int i = 0; i < tempAL.Count; ++i)
                als[i] = tempAL[i];
            AtackListeners = als;
        }

        //Установка зон атак из путей, указанных в инспекторе
        List<AtackArea> tempAA = new List<AtackArea>();
        foreach (string AAPath in _AtackAreas) {
            Node AAObj = GetNode(AAPath);
            if (AAObj is AtackArea) {
                AtackArea aa = AAObj as AtackArea;
                tempAA.Add(aa);
            }
        }
        if (tempAA.Count > 0) {
            AtackArea[] aas = new AtackArea[tempAA.Count];
            for (int i = 0; i < tempAA.Count; ++i)
                aas[i] = tempAA[i];
            AtackAreas = aas;
        }

        _UpdateRectSizeBySpriteTexture();
    }


    public override void _PhysicsProcess(float delta) {

        //Действие силы гравитации
        if (!IsFly)
            MovementDirection.y -= Gravity;
        
        WaitInteractProcess();

        //Передвижение сущности
        MovementProcess(delta);

        //Сброс скорости падения если на полу
        if (IsOnFloor()) MovementDirection.y = 0;

        //Работа с аниациями
        UpdateAnimations();
        
        //Обработка палёта если активен
        FlightProcess(delta);

        //Отсчёт до окончания режима атаки
        if (AtackModeWait > 0) AtackModeWait -= delta;
        if (AtackModeWait <= 0 && AtackMode) {
            AtackModeWait = 0;
            AtackMode = false;
        }

        //Определение преземления
        if (OnFloor != IsOnFloor()) {
            OnFloor = IsOnFloor();
            if (OnFloor) IsLanded();
        }
    }

    ///Обработка ожиданий взаимодействия
    protected void WaitInteractProcess() {
        //Определения близжайшего ожидающего интерактивного объекта объекта
        if (_WaitingInteractFor.Count > 0) {
            int closeIndex = 0;
            float minDistance = GetGlobalTransform().origin.DistanceTo(_WaitingInteractFor[0].GetGlobalTransform().origin);
            if (_WaitingInteractFor.Count > 1)
                for (int i = 1; i < _WaitingInteractFor.Count; ++i) {
                    float distance = GetGlobalTransform().origin.DistanceTo(_WaitingInteractFor[i].GetGlobalTransform().origin);
                    if (minDistance > distance) {
                        minDistance = distance;
                        closeIndex = i;
                    }
                }
            //Установка близжайшего объекта первым в списке, если таковым не является
            if (closeIndex != 0) {
                InteractiveObject obj = _WaitingInteractFor[closeIndex];
                _WaitingInteractFor.Remove(obj);
                _WaitingInteractFor.Insert(0, obj);
            }
        }
        //Обработка смены приоритетного ожидающего
        if (WaitingInteractFor != OldWaitingInteractFor) {
            OldWaitingInteractFor = WaitingInteractFor;
            EmitSignal(nameof(WaitingInteractForChanged), WaitingInteractFor);
        }
    }

    protected void MovementProcess(float delta) {
        //Маштаб скорости. 1 - без изменений
        float speedScale = 1;

        if (AtackMode) {
            //Замедление, если сущность идёт в обратном направлении от цели
        }

        Vector3 MovDir = MovementDirection;

        if (!CanMovement || IsInteracting) {
            MovDir.x = 0; MovDir.z = 0;
        }

        //Смещение если внизу сущность
        if (EntitiesBellow.Count > 0) {
            MovDir.x = BellowOffseting.x * 15; MovDir.z = BellowOffseting.z * 15;
        }

        MoveAndSlide(MovDir*speedScale*delta*DELTA_MUL, NormalFloor);
    }

    ///Эта функция вызывается, когда сущность приземляется на землю
    protected virtual void IsLanded() { }

    protected void FlightProcess(float delta) {
        if (IsFly) {
            FlyHeight = GetGlobalTransform().origin.y - FloorHeight; //Обновление переменной высоты полёта

            MovementDirection.y = 0;

            float startHeight = FlyHeight;

            //Запуск изменения высоты по ключу
            if (IsToKeyProcess) {
                if (startHeight < ToFlyHeightKey) {
                    ChangeFlyHeight(1);
                } else if (startHeight > ToFlyHeightKey) {
                    ChangeFlyHeight(-1);
                } else {
                    IsToKeyProcess = false;
                }
            }

            //Изменение высоты полёта по запросу
            if (ChangeFlyHeightRequest != 0) {
                MoveAndSlide(new Vector3(0, ChangeFlyHeightRequest * TakeoffSpeed * delta * DELTA_MUL, 0), NormalFloor);
                if (IsOnFloor()) IsFly = false;
                FlyHeight = GetGlobalTransform().origin.y - FloorHeight;
                //Плавное торможение
                if (ChangeFlyHeightRequest > 0) {
                    ChangeFlyHeightRequest -= 0.2f * delta * DELTA_MUL;
                    if (ChangeFlyHeightRequest < 0)
                        ChangeFlyHeightRequest = 0;
                } else if (ChangeFlyHeightRequest < 0) {
                    ChangeFlyHeightRequest += 0.2f * delta * DELTA_MUL;
                    if (ChangeFlyHeightRequest > 0)
                        ChangeFlyHeightRequest = 0;
                }
            }

            //Проверка не перелетела ли сущность ключ высоты
            if (IsToKeyProcess) {
                if (startHeight < FlyHeight) {
                    if (FlyHeight > ToFlyHeightKey) {
                        //На сколько сущность перелетела ключ
                        float move = FlyHeight - ToFlyHeightKey;
                        //Восстановление высоты сущности до значения ключа
                        Translation = new Vector3(Translation.x, Translation.y - move, Translation.z);
                        FlyHeight = GetGlobalTransform().origin.y - FloorHeight;
                        IsToKeyProcess = false;
                    }
                }
                else if (startHeight > FlyHeight) {
                    if (FlyHeight < ToFlyHeightKey) {
                        //На сколько сущность перелетела ключ
                        float move = FlyHeight - ToFlyHeightKey;
                        //Восстановление высоты сущности до значения ключа
                        Translation = new Vector3(Translation.x, Translation.y + move, Translation.z);
                        FlyHeight = GetGlobalTransform().origin.y - FloorHeight;
                        IsToKeyProcess = false;
                    }
                }
            }

            //Блокировка набора высоты свыше максимальной
            if (GetGlobalTransform().origin.y > MaxFlyHeight) {
                Translation = new Vector3(Translation.x, MaxFlyHeight + (GetGlobalTransform().origin.y -Translation.y), Translation.z);
                GD.Print("ОНА ТЕБЯ СОЖРЁТ!");
            }

            //FlyHeight = GetGlobalTransform().origin.y - FloorHeight; //Обновление переменной высоты полёта
        }
    }

    ///<summary>
    ///<para>Изменить направление взлёта в следующей обработке PhysicsProcess. Елси (to) = 1, то вверх; если (to) = -1, то вниз.</para>
    ///<para>Лучше всего использовать для реализации управления пользователем. Чтобы указать конкретную высоту полёта, используйте TakeoffTo(float to)</para>
    ///</summary>
    public void ChangeFlyHeight(float intensity) {
        ChangeFlyHeightRequest = intensity;
    }

    ///<summary>
    ///<para>Задать ключ высоты существо. Сущность будет изменять высоту в направлении ключа пока не достигнет его значения</para>
    ///</summary>
    public void TakeoffTo(float to) {
        IsToKeyProcess = true;
        ToFlyHeightKey = to;
    }

    protected virtual void UpdateAnimations() {
        if (IsActionRuning) return;
        //Установка направление анимации
        if (!AtackMode) {
            if (MovementDirection.x > 0) {
                AnimationDirection = (int)EntityStayDirection.right;
            } else if (MovementDirection.x < 0) {
                AnimationDirection = (int)EntityStayDirection.left;
            }
        } else {
            if (GetGlobalTransform().origin.x > AtackModeTarget.x) {
                AnimationDirection = (int)EntityStayDirection.left;
            } else {
                AnimationDirection = (int)EntityStayDirection.right;
            }
        }

        if (!IsFly) { //Поход вне полёта
            if (MovementDirection.x == 0 && MovementDirection.z == 0 && IsOnFloor()) { //Стоять
                RunAnimation((AnimationDirection == (int)EntityStayDirection.left) ? "StayLeft" : "StayRight");
            } else if ((MovementDirection.x != 0 || MovementDirection.z != 0) && MovementDirection.y == 0) { //Передвижение по земле
                if (!AtackMode)
                    RunAnimation((AnimationDirection == (int)EntityStayDirection.left) ? "MovementLeft" : "MovementRight");
                else {
                    //Если сущность в режиме атаки идёт в обратном направлении от цели TODO: Фиксить
                    Vector2 entityMovement = new Vector2(MovementDirection.x, MovementDirection.z).Normalized();
                    Vector2 targetVector = (new Vector2(AtackModeTarget.x, AtackModeTarget.z) - new Vector2(Translation.x, Translation.z)).Normalized();
                    float sum = Math.Abs(entityMovement.Angle() - targetVector.Angle());
                    if (sum > Math.PI/2)
                        RunAnimation((AnimationDirection == (int)EntityStayDirection.left) ? "MovementLeftBack" : "MovementRightBack");
                    else
                        RunAnimation((AnimationDirection == (int)EntityStayDirection.left) ? "MovementLeft" : "MovementRight");
                }
            } else if (MovementDirection.y < 0) { //Падение
                RunAnimation((AnimationDirection == (int)EntityStayDirection.left) ? "FallLeft" : "FallRight");
            } else if (MovementDirection.y > 0) { //Прыжок
                RunAnimation((AnimationDirection == (int)EntityStayDirection.left) ? "JumpLeft" : "JumpRight");
            }
        } else if (IsFly) { //В полёте
            if (MovementDirection.x == 0 && MovementDirection.z == 0) { //В воздухе на месте
                RunAnimation((AnimationDirection == (int)EntityStayDirection.left) ? "FlyInPlaceLeft" : "FlyInPlaceRight");
            } else if ((MovementDirection.x != 0 || MovementDirection.z != 0) && MovementDirection.y == 0) { //Передвижение в воздухе
                RunAnimation((AnimationDirection == (int)EntityStayDirection.left) ? "FlyMovementLeft" : "FlyMovementRight");
            }
        }
    }

    ///<summary>
    ///<para>Выстрелить от имени этой сущности в направлении Direction если сущность использует в данный момент какую-то пушку</para>
    ///</summary>
    public virtual void ShotInDirection(Vector3 direction) {
        //Блокировка при взаимодействии
        if (IsInteracting) return;

        Item CurrentUsingItem = EntityInventory.GetItemAt(EntityInventory.MainSlotGroup, EntityInventory.MainSlot);
        if (CurrentUsingItem is Gun && EntityInventory.UseMainSlot) {
            Gun CurrentUsingGun = CurrentUsingItem as Gun;
            Spatial CurrentUsingVisualGun = ItemPlaces[EntityInventory.MainSlotGroup][EntityInventory.MainSlot] as Spatial;
            CurrentUsingGun.Shot(direction - CurrentUsingVisualGun.Translation);
            
            //Активация режима атаки
            AtackModeWait = AtackModeDuration;
            //GD.Print("[Entity] Atack Mode");
            AtackMode = true;
        }
    }

    ///Выстрелить от имени этой сущности в точку (point) если сущность использует в данный момент какую-то пушку. Точка указывается в глобальной системе координат
    ///По возможности можно указать объект цели (target)
    public virtual void Shot(Vector3 point, Spatial target = null) {
        //Блокировка при взаимодействии
        if (IsInteracting) return;
        
        Item CurrentUsingItem = EntityInventory.GetItemAt(EntityInventory.MainSlotGroup, EntityInventory.MainSlot);
        if (CurrentUsingItem != null && CurrentUsingItem is Gun && EntityInventory.UseMainSlot) {
            Gun CurrentUsingGun = CurrentUsingItem as Gun;
            ItemPlace CurrentUsingVisualGun = ItemPlaces[EntityInventory.MainSlotGroup][EntityInventory.MainSlot] as ItemPlace;
            if (CurrentUsingVisualGun == null) {
                OS.Alert("Визуального представления предмета главного слота не существует. Свяжитесь с разработчиками, опишите при каких условиях полусена ошибка", "Ошибка");
                return;
            }
            if (!(CurrentUsingVisualGun.ItemOfPlace is VisualGun)) return;
            CurrentUsingGun.Shot(point - (CurrentUsingVisualGun.ItemOfPlace as VisualGun).GetShotPoint(), this, ShotExceptions);

            //Активация режима атаки
            AtackModeWait = AtackModeDuration;
            //GD.Print("[Entity] Atack Mode");
            AtackMode = true;
            AtackModeTarget = point;
        }
    }

    //Здесь будут принматься сигналы от инвентаря, для обновления визуальных представлений предметов этого инвентаря
    private void Inventory_ItemAdded(int group, int slot) {
        if (ItemPlaces.Count > group) {
            if (ItemPlaces[group].Length > slot) {
                if (ItemPlaces[group][slot] != null)  {
                    VisualItem newItem = (ResourceLoader.Load(EntityInventory.GetItemAt(group, slot).PathToVisualItem) as PackedScene).Instance() as VisualItem;
                    newItem.Setup(EntityInventory.GetItemAt(group, slot), EntityInventory.GetSlotTags(group, slot));
                    ItemPlaces[group][slot].ItemOfPlace = newItem;
                }
            }
        }
    }

    private void Inventory_ItemRemoved(int group, int slot) {
        if (ItemPlaces.Count > group)
            if (ItemPlaces[group].Length > slot)
                if (ItemPlaces[group][slot] != null) {
                    ItemPlaces[group][slot].ItemOfPlace.QueueFree();
                    ItemPlaces[group][slot].ItemOfPlace = null;
                }
    }

    public VisualItem GetVisualItemAt(int group, int slot) {
        if (ItemPlaces.Count > group)
            if (ItemPlaces[group].Length > slot)
                return ItemPlaces[group][slot].ItemOfPlace;
        return null;
    }

    ///<summary>
    ///<para>Повернуть используемый сущностью предмет, чтобы он с мотрел в точку (point). Точка задаётся в глобальной системе координат</para>
    ///</summary>
    public void LookUsingItemAt(Vector3 point) {
        if (EntityInventory.GetItemAt(EntityInventory.MainSlotGroup, EntityInventory.MainSlot) == null) 
            return;
        
        VisualItem CurrentUsingVisualItem = GetVisualItemAt(EntityInventory.MainSlotGroup, EntityInventory.MainSlot);
        CurrentUsingVisualItem.SetGunRotation(point - CurrentUsingVisualItem.GetGlobalTransform().origin);
    }

    public Vector3 GetAtackModeTarget() {
        return (AtackModeTarget != null) ? AtackModeTarget : new Vector3(0, 0, 0);
    }

    public bool RunAction(int actionID) {
        GD.Print("Entity] Try run action");
        if (Actions.Length < actionID || isBusy) return false;
        if (IsActionRuning) {
            if (RuningActionId == actionID) return false;
            Actions[RuningActionId].Stop();
        }
        GD.Print("Entity] Try run action 2");

        bool result = Actions[actionID].Run();
        if (result) {
            if (Actions[actionID].CanBusy) isBusy = true;
            IsActionRuning = true;
        }
        GD.Print("Entity] Try run action 3");

        return result;
    }

    ///Принятие сигналов от действий
    public void ActionEnded(EntityAction action) {
        if (action.CanBusy && isBusy)
            isBusy = false;
        
        IsActionRuning = false;
    }

    ///Указать направление взгляда по точке (point). Точка указывается в глобальной системе координат
    public void SetSightDirectionTo(Vector3 point) {
        if (Eyes == null) return;

        foreach (Eye eye in Eyes) {
            Vector3 eyeGlobalPosition = eye.GetGlobalTransform().origin;
            Vector3 eyeSightDirection = point - eyeGlobalPosition;
            eyeSightDirection.x /= SightHDis;
            eye.PupilPosition = new Vector2(eyeSightDirection.x, eyeSightDirection.y);
        }
        if (SightAngles != null && SightAngles.Length > 0) {
            Vector3 MovDir = point - GetGlobalTransform().origin;
            Vector2 MovDir2 = new Vector2(MovDir.x, MovDir.z);
            Vector3 RayCastDir = new Vector3(0, MovDir2.Angle(), 0);
            SightAngles[0] = -RayCastDir;
        }
    }

    public Vector3[] GetSightAngles() { return SightAngles; }

    //Принимать сигнал от слушателей атаки
    public void AtackListened(int damage, Spatial damager) {
        Injure(damage, damager);
    }

    ///Обработка появления и исчезновения сущностей под этой сущностью
    public void BellowTracker_BodyEntered(Godot.Object body) {
        if (body is Entity && body != this && !EntitiesBellow.Contains(body as Entity)) {
            Entity bellow = body as Entity;
            //Рандомизация направления смещения
            // BellowOffseting = new Vector3(PonyRandom.Rnd.Next(-1, 2), 0, PonyRandom.Rnd.Next(-1, 2));
            // if (BellowOffseting.x == 0 && BellowOffseting.y == 0)
            //     BellowOffseting = new Vector3(0, 0, 1);
            //Указывание близжайшего направления смещения
            Vector3 bellowPos = bellow.GetGlobalTransform().origin;
            Vector3 selfPos = GetGlobalTransform().origin;
            BellowOffseting = new Vector3(
                (Math.Abs(bellowPos.x - selfPos.x) > 0.8) ? -1 : (Math.Abs(bellowPos.x - selfPos.x) < 0.8 ? 1 : 0),
                0,
                (bellowPos.z > selfPos.z) ? -1 : 1
            );

            EntitiesBellow.Add(bellow);
        }
    }

    public void BellowTracker_BodyExited(Godot.Object body) {
        if (body is Entity && body != this && EntitiesBellow.Contains(body as Entity)) {
            EntitiesBellow.Remove(body as Entity);
        }
    }

    ///Начать модальное ожидание объекта взаимодействия от этой сущности
    ///Возрящает True при успехе. Если уже активно модальное ожидание другого объекта, возвращает false;
    ///!!! Вызывается только от интерактивного объекта. Посторонние вызовы могут привести к сбоям
    public bool WaitInteractFor(InteractiveObject obj) {
        //TODO: Так же блокировать если сущность уже взаимодействует с каким либо объектом
        //if (isModalWaitingInteract) return false;
        _WaitingInteractFor.Add(obj);
        return true;
    }
    
    public void CancelWaitInteract(InteractiveObject obj) {
        _WaitingInteractFor.Remove(obj);
    }

    ///Начать взаисодействие с ожидающим объектом
    public bool Interact() {
        if (WaitingInteractFor == null) return false;
        if (IsInteracting) return false;
        InteractiveObject obj = WaitingInteractFor;
        bool result = WaitingInteractFor._Interact(this);
        if (result) {
            DebugConsole.Shared.Output("[Entity] Interact OK");
            _InteractingWith = obj;
        }
        return result;
    }

    ///Остановить взаисодействие с ожидающим объектом
    public bool StopInteract() {
        if (InteractingWith == null) return false;
        InteractiveObject obj = InteractingWith;
        bool result = obj._StopInteract(this);
        if (result) {
            DebugConsole.Shared.Output("[Entity] Stop interact OK");
            _InteractingWith = null;
        }
        return result;
    }

    ///Отправить действие взаимодействия (act) текущему интерактивному объекту
    public void SendInteractAction(int act) {
        if (!IsInteracting) return;
        InteractingWith._GetInteractActionFromEntity(this, act);
        EmitSignal(nameof(InteractAction), act);
    }

    //Работа со списком всех экземпляров
    protected void TreeEntered() {
        if (!Instances.Contains(this))
            Instances.Add(this);
    }
    protected void TreeExited() {
        Instances.Remove(this);
    }

    ///Обновление прямоугольника объекта
    protected void _UpdateRectSizeBySpriteTexture() {
        if (EntityTexture == null) return;
        Texture texture = EntityTexture.Texture;
        
        if (texture == null) return;

        Vector2 size = texture.GetSize();

        LeftRectOffset = size.x / 2 * 0.02f;
        RightRectOffset = LeftRectOffset;
        TopRectOffset = size.y / 2 * 0.02f;
        BottomRectOffset = TopRectOffset;
        FrontRectOffset = 1;
        BackRectOffset = 1;
        ZeroOffset = new Vector3(0, LeftRectOffset, 0);
    }

}

public enum EntityStayDirection {left, right}