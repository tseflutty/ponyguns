using Godot;
using System;

//2019 © Даниил Белов
//Создано 11.07.2019

public class Eye : Spatial
{

    [Signal]
    public delegate void EyeBlink();
    

    protected Sprite Top {
        get { return GetNode("EyeTexture/EyeTop") as Sprite; }
    }

    protected AnimationPlayer Animations {
        get { return GetNode("Animations") as AnimationPlayer; }
    }

    protected Sprite Pupil;

    protected Sprite EyeBack;
    protected Sprite EyeTop;
    protected Sprite3D EyeSprite;
    protected Viewport EyeTexture;

    protected Texture _smallPupilTexture;
    [Export]
    ///Текстура суженного зрачка
    public Texture SmallPupilTexture {
        get { return _smallPupilTexture; }
        set {
            _smallPupilTexture = value;
            _UpdatePupilTexture();
        }
    }

    protected Texture _normalPupilTexture;
    [Export]
    ///Текстура обычного зрачка
    public Texture NormalPupilTexture {
        get { return _normalPupilTexture; }
        set {
            _normalPupilTexture = value;
            _UpdatePupilTexture();
        }
    }

    protected Texture _bigPupilTexture;
    [Export]
    ///Текстура расширенного зрачка
    public Texture BigPupilTexture {
        get { return _bigPupilTexture; }
        set {
            _bigPupilTexture = value;
            _UpdatePupilTexture();
        }
    }

    protected EyePupilSize _pupilScale;
    [Export]
    ///Размер зрачка
    public EyePupilSize PupilScale {
        get { return _pupilScale; }
        set {
            _pupilScale = value;
            _UpdatePupilTexture();
        }
    }
    
    private Color _tempEyeColor;
    
    [Export]
    ///Цвет глаза
    public Color EyeColor {
        get {
            if (Pupil == null) return new Color(0, 0, 0, 1);
            return Pupil.Modulate;
        }
        set {
            if (Pupil != null) {
                Pupil.Modulate = value; 
            } else {
                _tempEyeColor = value;
            }
        }
    }

    [Export]
    ///Если установлено True, глаз моргает не по времени, а вместе с другими указанным глазом по сигналам
    public bool BlinkWithConnected = false;

    protected Eye _connectedEye;
    ///Глаз вместе с которым моргает этот, если активно свойство BlinkWithConnected
    public Eye ConnectedEye {
        get { return _connectedEye; }
        set {
            if (_connectedEye != null)
                _connectedEye.Disconnect(nameof(EyeBlink), this, nameof(ConectedEye_EyeBlink));
            
            _connectedEye = value;
            _connectedEye.Connect(nameof(EyeBlink), this, nameof(ConectedEye_EyeBlink));
        }
    }

    [Export]
    ///Отражение глаз
    public bool FlipH = false;

    static protected Random EyeRandom = new Random();

    [Export]
    public NodePath ConnectedEyePath;

    [Export]
    //Время между морганиями в секундах
    public float BlinkDelay = 4;

    [Export]
    //Рандом разницы времени между морганиями (+-BlinkDelayRandom) в секундах
    public float BlinkDelayRandom = 1;

    protected Vector2 _pupilMoveBounds = new Vector2(0, 0);

    [Export]
    ///Максимальные значения по ширине и высоте на которые зрачки могу отводиться от центра вперёд и назад.
    public Vector2 PupilMoveBounds {
        get { return _pupilMoveBounds; }
        set {
            _pupilMoveBounds = value;
            _UpdatePupilPosition();
        }
    }


    protected Vector2 _pupilPosition = new Vector2(0, 0);

    ///Позиция относительно центра зрачков в процентах (от -1 до 1)
    [Export]
    public Vector2 PupilPosition {
        get { return _pupilPosition; } 
        set {
            _pupilPosition = value;

            if (Pupil == null) return;

            //Ограничение значения (от -1 до 1, остальное обрезать)
            if (_pupilPosition.x > 1) _pupilPosition.x = 1;  if (_pupilPosition.x < -1) _pupilPosition.x = -1;
            if (_pupilPosition.y > 1) _pupilPosition.y = 1;  if (_pupilPosition.y < -1) _pupilPosition.y = -1;

            _UpdatePupilPosition();
        }
    }

    [Export]
    ///Размер сетки для привязки к ней
    public int PointSize = 3;

    //Отсчёт в секундах до следующего моргания
    protected float BlickWait = 0;

    //Принимает сигнал от другого глаза по морганию
    protected void ConectedEye_EyeBlink() {
        if (BlinkWithConnected)
            Blink();
    }

    //Моргнуть
    public void Blink() {
        if (Animations.HasAnimation("Blink"))
            Animations.Play("Blink");
        EmitSignal(nameof(EyeBlink));
    }

    protected Vector2 _pupilSize;
    ///Размер зрачка
    public Vector2 PupilSize {
        get { return _pupilSize; }
        set {
            _pupilSize = value;
        }
    }
    public override void _Ready() {

        Pupil = GetNode("EyeTexture/Pupil") as Sprite;
        EyeBack = GetNode("EyeTexture/EyeBack") as Sprite;
        EyeTop = GetNode("EyeTexture/EyeTop") as Sprite;
        EyeSprite = GetNode("EyeSprite") as Sprite3D;
        EyeTexture = GetNode("EyeTexture") as Viewport;

        //Удаления ноды предпросмотра для редактора
        GetNode("EyePreview").QueueFree();

        //Показать спрайт глаза
        EyeSprite.Show();

        if (_tempEyeColor != null) EyeColor = _tempEyeColor;

        if (ConnectedEyePath != null) {
            Node eye = GetNode(ConnectedEyePath);
            if (eye is Eye)
                ConnectedEye = eye as Eye;
        }
        BlickWait = BlinkDelay;

        _UpdatePupilTexture();
        _UpdatePupilPosition();
        _UpdateEyeTexture(false);
    }

    public override void _PhysicsProcess(float delta) {
        if (!BlinkWithConnected) {
            //Отсчёт до следующего моргания
            BlickWait -= delta;
            //Моргание и назначение нового времени по истечению отсчёта
            if (BlickWait <= 0) {
                BlickWait = BlinkDelay + ((float)EyeRandom.NextDouble() - 0.5f)*BlinkDelayRandom*2;
                Blink();
            }
        }
        //Дублирование на верх FlipH у родителя
        if (Top.FlipH != FlipH || EyeSprite.FlipH != FlipH) {
            EyeBack.FlipH = FlipH;
            Top.FlipH = FlipH;
        }
        _UpdateEyeTexture(true);

    }

    protected void _UpdateEyeTexture(bool whenNeed) {
        if (EyeBack.Texture != null) {
            Vector2 eyeSize = EyeBack.Texture.GetSize();
            if (EyeTexture.Size != eyeSize || !whenNeed) {
                EyeTexture.Size = eyeSize;
            }
        }
    }

    protected void _UpdatePupilTexture() {
        if (Pupil == null) return;
        Texture[] t = new Texture[3] {SmallPupilTexture, NormalPupilTexture, BigPupilTexture};
        Pupil.Texture = t[ (int)PupilScale ];
        PupilSize = (Pupil.Texture != null) ? Pupil.Texture.GetSize() : Vector2.One;
    }

    static Vector2 PupilPosMask = new Vector2(1, -1);
    protected void _UpdatePupilPosition() {
        if (Pupil == null) return;
        // Pupil.Position = (Offsets - PupilSize/2) * PupilPosition * PupilPosMask;
        // Vector2 PointOffset = new Vector2(Pupil.Position.x%PointSize, Pupil.Position.y%PointSize);
        // Pupil.Position -= PointOffset;

        ///Копируем значения допустимого смещения зрачка для обработки
        Vector2 offsets = PupilMoveBounds/2;
        //Смещение отображения зрачка
        Vector2 viewOffset = Vector2.Zero;

        if ( (int)(offsets.x*2 - PupilSize.x) % PointSize == 0 
            && ((int)((offsets.x*2 - PupilSize.x)/2) % PointSize) != 0 ) {
            float of = (offsets.x*2 - PupilSize.x)/2;
            float cut = PupilSize.x * (int)(of/PupilSize.x);
            of -= cut;
            offsets += new Vector2(of, 0);
            viewOffset -= new Vector2(of, 0);
        }
        if ( (int)(offsets.y*2 - PupilSize.y) % PointSize == 0 
            && ((int)((offsets.y*2 - PupilSize.y)/2) % PointSize) != 0 ) {
            float of = (offsets.y*2 - PupilSize.y)/2;
            float cut = PupilSize.y * (int)(of/PupilSize.y);
            of -= cut;
            offsets += new Vector2(0, of);
            viewOffset -= new Vector2(0, of);
        }

        Pupil.Position = new Vector2(
            (((PupilPosition.x < 0) ? PupilMoveBounds.x/2 : offsets.x) - PupilSize.x/2) * PupilPosition.x,
            (((PupilPosition.y < 0) ? PupilMoveBounds.y/2 : offsets.y) - PupilSize.y/2) * PupilPosition.y
        );
        Pupil.Position = new Vector2(
            (int)(Pupil.Position.x) - (int)(Pupil.Position.x) % PointSize,
            (int)(Pupil.Position.y) - (int)(Pupil.Position.y) % PointSize
        );

        Pupil.Position += viewOffset;
        
        // Pupil.Position = new Vector2(
        //     (PupilMoveBounds.x/2 - PupilSize.x/2) * PupilPosition.x,
        //     (PupilMoveBounds.y/2 - PupilSize.y/2) * PupilPosition.y
        // );

        Pupil.Position *= PupilPosMask;
        // Pupil.Position = new Vector2(
        //     (int)(Pupil.Position.x) - (int)(Pupil.Position.x) % PointSize,
        //     (int)(Pupil.Position.y) - (int)(Pupil.Position.y) % PointSize
        // );

        //Pupil.Position = Vector2.Zero;
    }

    public enum EyePupilSize {
        Small, Normal, Big
    }

}
