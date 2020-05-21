using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 14.05.2019


///<summary>
///<para>3D камера с дополнительным функционалом:</para>
///<para> - Следование за указанным объектом</para>
///<para> - Определение нажатий по 3D телам</para>
///</summary>
public class PonyCamera : Camera
{

    [Signal]
    public delegate void UserClickedOn3DBody(Vector3 pos, Spatial body);

    [Signal]
    public delegate void CursorMoved(Vector3 to);
    
    ///<summary>
    ///<para>Объект, за которым данная камера следит</para>
    ///</summary>
    public Spatial Pursued;

    ///<summary>
    ///<para>Установлено True, если камера должна следить за указанным объектом</para>
    ///</summary>
    public bool FollowToObject = true;
    
    [Export]
    ///<summary>
    ///<para>С какой силой сглаживается передвижение камеры</para>
    ///</summary>
    public float SmoothStrength = 10;

    private float _distanceToObject = 26;
    [Export]
    ///<summary>
    ///<para>Какое расстояние от камеры до точки следования</para>
    ///</summary>
    public float DistanceToObject {
        get { return _distanceToObject;}
        set {
            (GetNode("ObjectPoint") as Spatial).Translation = new Vector3(0, 0, value * -1); 
            _distanceToObject = value;
        }
    }


    [Export]
    ///Смещение камеры относительно цели при фключенном (FollowToObject)
    public Vector3 Offset;

    [Export]
    ///<summary>
    ///<para>Если установлено True, производится отслежования 3D тел, на котором лежит курсор и нажатие на эти тела</para>
    ///</summary>
    public bool AllowCursorTracking = true;


    [Export]
    ///Какое устройство (Игровой контроллер) будет управлять курсором (От 1 до 8). Если установлено 0 - управление мышью
    public byte CursorControlDevice = 0;

    //Точка относительно камеры, которая находится в нулевой точке объекта, когда камера смотрит непосредственно на него (Проще: Смещение камеры)
    private Spatial ObjectPoint;

    private Spatial RayCastParent;
    private RayCast rayCast;


    private Spatial _cursorCollider = null;


    private string[] _exceptionKeys = new string[0];

    [Export]
    //Ключевые слова в названиях объектов для исключения рейкастинга курсора
    protected string[] _ExceptionKeys {
        get { return _exceptionKeys; }
        set {
            _exceptionKeys = value;
            RayCastExceptions.Clear();
        }
    }

    protected Godot.Collections.Array RayCastExceptions = new Godot.Collections.Array();

    protected Spatial cursorCollider {
        get { return _cursorCollider; }
        set {
            if (_cursorCollider != null) _cursorCollider.Disconnect("tree_exited", this, nameof(CurosrColliderRemoved));
            _cursorCollider = value;
            _cursorCollider.Connect("tree_exited", this, nameof(CurosrColliderRemoved));
        }
    }

    ///Объект в трёхмерном пространстве на котором лежит курсор
    public Spatial CursorCollider {
        get { return _cursorCollider; }
    }

    protected Vector3 _cursorPoint = new Vector3(0, 0, 0);

    ///Точка в трёхмерном пространстве в которой находиться курсора
    public Vector3 CursorPoint {
        get { return _cursorPoint; }
    }

    protected Vector2 _cursor2DPosition;

    public Vector2 Cursor2DPosition {
        get { return _cursor2DPosition; }
    }

    [Export]
    ///Размер Viewport этой камеры
    public Vector2 ViewportSize = new Vector2(1280, 720);

    [Export]
    ///Размер контейнера Viewport этой камеры
    public Vector2 ViewportContainerSize = new Vector2(1280, 720);

    //На сколько умножаются милисекунды между кадрами при обработке в процессе
    const float DELTA_MUL = 30;

    const float rayLength = 1000;

    ///Путь до объекта (Для настройки в инспекторе)
    [Export]
    protected NodePath CameraTarget;

    ///Позиция камера в предыдущем цикле PhysicsProcess. Для обработки передвижения
    protected Vector3 OldPos;

    public override void _Ready() {
        ObjectPoint = GetNode("ObjectPoint") as Spatial;
        RayCastParent = GetNode("RayCastParent") as Spatial;
        rayCast = GetNode("RayCastParent/RayCast") as RayCast;

        //Установка за кем следить, если он подключен через инспектор
        if (CameraTarget != null && HasNode(CameraTarget)) {
            Node target = GetNode(CameraTarget);
            if (target is Spatial)
                Pursued = target as Spatial;
        }

    }
    
    public override void _Input(InputEvent ev) {
        //Обновление данных о позиции курсора и объекте, на котором стоит курсор и испускание сигнала
        if (ev is InputEventMouseMotion && AllowCursorTracking) {
            InputEventMouseMotion e = ev as InputEventMouseMotion;
            _cursor2DPosition = e.Position / (ViewportContainerSize / ViewportSize);
            UpdateCursor2DPosition();
        }
        if (ev is InputEventMouseButton && ev.IsPressed() && AllowCursorTracking) {
            InputEventMouseButton e = ev as InputEventMouseButton;
            if (e.ButtonIndex == 1) {
                //GD.Print("[Following Camera] Click in ", rayCast.GetCollisionPoint());
                EmitSignal(nameof(UserClickedOn3DBody), CursorPoint, ((CursorCollider is Spatial) ? CursorCollider as Spatial : null));
            }
        }
    }

    ///Обновить позицию курсора
    protected void UpdateCursor2DPosition() {
        if (CursorControlDevice > 0) {
            float[] axis = new float[2] {
                Input.GetJoyAxis(CursorControlDevice-1, 2),
                Input.GetJoyAxis(CursorControlDevice-1, 3)
            };
            _cursor2DPosition += new Vector2(
                (Math.Abs(axis[0]) > 0.1) ? axis[0]*15 : 0,
                (Math.Abs(axis[1]) > 0.1) ? axis[1]*15 : 0
            );
        }
        //Блокировка выхода за рамки
        Vector2 viewPortSize = ViewportSize;
        if (_cursor2DPosition.x < 0)
            _cursor2DPosition.x = 0;
        if (_cursor2DPosition.x > viewPortSize.x)
            _cursor2DPosition.x = viewPortSize.x;
        if (_cursor2DPosition.y < 0)
            _cursor2DPosition.y = 0;
        if (_cursor2DPosition.y > viewPortSize.y)
            _cursor2DPosition.y = viewPortSize.y;
        UpdateCursorPositionAndCollider();
    }


    ///Обновить позицию курсора в трёхмерном пространстве и коллайдера(На чём лежит курсор)
    protected void UpdateCursorPositionAndCollider() {
        if (AllowCursorTracking) {
            Vector2 mousePos = Cursor2DPosition;
            Vector3 from = ProjectRayOrigin(mousePos);
            Vector3 to = from + ProjectRayNormal(mousePos) * rayLength;

            PhysicsDirectSpaceState spaceState = GetWorld().DirectSpaceState;
            while (true) {
                Dictionary result = spaceState.IntersectRay(from, to, RayCastExceptions);
                if (result == null || !result.ContainsKey("collider") || !(result["collider"] is Spatial))  break;

                Spatial collider = result["collider"] as Spatial;

                bool isException = false;
                foreach (string key in _ExceptionKeys) {
                    if (collider.Name.ToLower().Contains(key.ToLower())) {
                        isException = true;
                        break;
                    }
                }

                if (isException) {
                    if (!RayCastExceptions.Contains(collider)) {
                        RayCastExceptions.Add(collider);
                    }
                } else {
                    if (RayCastExceptions.Contains(collider)) {
                        RayCastExceptions.Remove(collider);
                    }
                    cursorCollider = result["collider"] as Spatial;
                    _cursorPoint = (Vector3)result["position"];
                    break;
                }

            }

            EmitSignal(nameof(CursorMoved), CursorPoint);
        }
    }

    public override void _PhysicsProcess(float delta) {
        //Управление геймпадом
        if (CursorControlDevice > 0)
            UpdateCursor2DPosition();

        if (FollowToObject && Pursued != null)
            Translation += (Pursued.GetGlobalTransform().origin + Offset - ObjectPoint.GetGlobalTransform().origin) / (SmoothStrength * DELTA_MUL * delta);

        //Обработка передвижения
        Vector3 currentPos = GetGlobalTransform().origin;
        if (OldPos != currentPos) {
            UpdateCursorPositionAndCollider();
            OldPos = currentPos;
        }
    }

    protected void CurosrColliderRemoved() {
        DebugConsole.Shared.Output("[FollowingCamerd] CursorCollider Removed");
        _cursorCollider = null;
    }

    //Получить положение курсора в трёхмерном пространстве
    public Vector3 GetCursorPosition() {
        return _cursorPoint;
    }

    ///Установить ключевые слова в названии объекта исключения рейкастинга курсора.
    ///Объекты, названия которых будут содержать один или несколько из перечисленных ключей (exceptionKeys), не будут учитываться для
    ///определения позиции курсора в трёхмерном пространстве
    public void SetExceptionKeys(string[] exceptionKeys) => _ExceptionKeys = exceptionKeys;

    ///Получить ключевые слова в названии объекта исключения рейкастинга курсора
    public string[] GetExceptionKeys() => _ExceptionKeys;

}
