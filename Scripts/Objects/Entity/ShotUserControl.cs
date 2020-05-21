using Godot;
using System;

//2019 © Даниил Белов
//Создано 13.05.2019

public class ShotUserControl : Control
{

    //Текущее положение курсора в 3D пространстве
    Vector3 CursorPostion = new Vector3(0, 0, 0);

    //Зажатали кнопка мыши (левая)
    bool IsPressed = false;
    
    private PonyCamera _currentCamera;
    ///<summary>
    ///<para>Камера, на основе которой определяется точка направления выстрела</para>
    ///</summary>
    public PonyCamera CurrentCamera {
        get { return _currentCamera; }
        set {
            if (_currentCamera != null)
                _currentCamera.Disconnect(nameof(PonyCamera.CursorMoved), this, nameof(Camera_CurserMoved));

            _currentCamera = value;
            //_currentCamera.Connect(nameof(FollowingCamera.UserClickedOn3DBody), this, nameof(UserClickedOn3DBody));
            if (_currentCamera != null)
                _currentCamera.Connect(nameof(PonyCamera.CursorMoved), this, nameof(Camera_CurserMoved));
            //GD.Print(_currentCamera);
        }
    }

    ///<summary>
    ///<para>Сущность, от имени которой игрок производит выстрелы</para>
    ///</summary>
    public Entity Slave;

    public bool Enable = true;


    protected Vector2 _aimTextureSize;
    protected Texture _aimTexture = null;

    ///Текстура прицела для суверенного отображении прицела
    [Export]
    public Texture AimTexture {
        get { return _aimTexture; }
        set {
            _aimTexture = value;
            _aimTextureSize = _aimTexture.GetSize();
        }
    }

    protected bool _showAim = false;
    
    protected bool ShowAim  {
        get { return _showAim; }
        set {
            _showAim = value;
            Update();
        }
    }

    // private void UserClickedOn3DBody(Vector3 pos, Spatial body = null) {
    //     GD.Print("Shot ", Enable, Slave);
    //     if (Enable && Slave != null) {
    //         Slave.Shot(pos);
    //         GD.Print("Shot ", pos);
    //     }
    // }

    public override void _Input(InputEvent e) {
        if (e.IsAction("ui_click")) {
            //Определение зажатия кнопок для стрельбы
            if (e.IsPressed()) {
                IsPressed = true;
            } else {
                IsPressed = false;
            }
        }
    }

    public override void _Draw() {
        if (ShowAim && AimTexture != null && CurrentCamera != null) {
            Vector2 scale = CurrentCamera.ViewportContainerSize / CurrentCamera.ViewportSize;
            DrawTexture(AimTexture, new Vector2(
                CurrentCamera.Cursor2DPosition.x * scale.x - _aimTextureSize.x/2, 
                CurrentCamera.Cursor2DPosition.y * scale.y -_aimTextureSize.y/2)
            );
        }
    }

    public override void _PhysicsProcess(float delta) {
        if (CurrentCamera != null)
            ShowAim = CurrentCamera.CursorControlDevice != 0;
        //Выстрелы если зажато
        if (Enable && IsPressed && Slave != null) {
            CursorPostion = CurrentCamera.GetCursorPosition();
            Spatial CursorCollider = CurrentCamera.CursorCollider;
            if (CursorCollider != null) {
                if (CursorCollider.GetName().ToLower().Contains("floor"))
                    Slave.Shot(CursorPostion + new Vector3(0, 1, 0));
                else
                    Slave.Shot(CursorPostion, CurrentCamera.CursorCollider);
            }
        }
    }

    private void Camera_CurserMoved(Vector3 to) {
        if (ShowAim)
            Update();
        Slave.LookUsingItemAt(to);
        Slave.SetSightDirectionTo(to);
        CursorPostion = to;
    }

    
}
