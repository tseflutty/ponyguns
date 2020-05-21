using Godot;
using System;

//2019 © Даниил Белов
//Создано 20.05.2019

public class VisualGun : VisualItem
{
    // [Signal]
    // public delegate void WhenShot(Bullet bull, Vector3 direction, VisualGun gun);

    [Export]
    ///Смещение текстуры в режиме смещения. Измеряется в пикселях текстуры
    public Vector2 RotationOffset = new Vector2(0, 0);

    [Export]
    ///Теги при которых будет активироваться режим смещения текстуры
    public int[] RotationOffsetTags = new int[1] {1};

    protected bool RotationOffsetMode = false;

    protected Spatial ShotPoint {
        get { return GetNode("ShotPoint") as Spatial; }
    }

    protected float RotationTarget = 0;
    protected const float DELTA_MUL = 30;

    //Перевёрнуто
    private bool ShotPointFlipped = false;

    ///<summary>
    ///<para>Выстрелить из этой пушки. Возвращает True если есть патроны, в противном случае возвращает False</para>
    ///</summary>
    public bool Shot(Vector3 direction) {
        if (!(ItemOfVisual is Gun)) return false;
        return (ItemOfVisual as Gun).Shot(direction);
    }

    public override void _PhysicsProcess(float delta) {
        base._PhysicsProcess(delta);
        
        //Плавный поворот
        if (RotationOffsetMode) {
            float v2 = 0;
            if (RotationTarget > (float)Math.PI)
                v2 = RotationTarget - 2*(float)Math.PI;
            if (RotationTarget < (float)Math.PI)
                v2 = RotationTarget + 2*(float)Math.PI;
            
            float r = Rotation.z;
            
            if (Math.Abs(r - v2) > Math.Abs(r - RotationTarget))
                Rotation -= new Vector3(0, 0, (Rotation.z - RotationTarget) / 2 * delta * DELTA_MUL);
            else
                Rotation -= new Vector3(0, 0, (Rotation.z - v2) / 2 * delta * DELTA_MUL);
                
            
            if (Rotation.z < 0)
                Rotation = new Vector3(Rotation.x, Rotation.y, 2*(float)Math.PI - Rotation.z);
            else
                Rotation -= new Vector3(0, 0, (float) (2*Math.PI * (int)(Math.Abs((Rotation.z) / (2*Math.PI)))));


            if (RotationDegrees.z < 270 && RotationDegrees.z > 90) {
                ItemTexture.SetFlipV(true);
                ItemTexture.Offset = RotationOffset * new Vector2(1, -1);
                if (!ShotPointFlipped) {
                    ShotPoint.Translation *= new Vector3(1, -1, 1);
                    ShotPointFlipped = true;
                }
            } else {
                ItemTexture.SetFlipV(false);
                ItemTexture.Offset = RotationOffset;
                if (ShotPointFlipped) {
                    ShotPoint.Translation *= new Vector3(1, -1, 1);
                    ShotPointFlipped = false;
                }
            }
        }
    }

    private void Gun_WhenShot(Vector3 direction, Spatial owner, int[] exceptions) {
        if (!(ItemOfVisual is Gun)) return;
        //GD.Print("[VisualGun] Shot");
        Gun g = (ItemOfVisual as Gun);
        PackedScene b = ResourceLoader.Load(g.PathToBullet) as PackedScene;
        //EmitSignal(nameof(WhenShot), (b.Instance() as Bullet), direction, this);
        SetGunRotation(direction);
        Arena.GetCurrent().SpawnShot(b.Instance() as Bullet, direction, ShotPoint.GetGlobalTransform().origin, owner, exceptions);

        RunAnimation("Shot");
    }

    protected override void UpdateTags() {
        RotationOffsetMode = false;
        ItemTexture.Offset = new Vector2(0, 0);
        foreach (int rotationOffsetTag in RotationOffsetTags)
            if (Array.IndexOf(Tags, rotationOffsetTag) >= 0) {
                GD.Print("[VisualGun] Offser Mode ", RotationOffset);
                ItemTexture.Offset = RotationOffset;
                RotationOffsetMode = true;
                break;
            }
        
        base.UpdateTags();
    }

    public override void Setup(Item item, int[] tags) {
        if (item is Gun) item.Connect(nameof(Gun.WhenShot), this, nameof(Gun_WhenShot));
        GD.Print("[VisualGun] Gun Setup");

        base.Setup(item, tags);
    }

    ///<summary>
    ///<para>Повернуть пушку по направлению вектора (direction)</para>
    ///</summary>
    public override void SetGunRotation(Vector3 direction) {
        if (RotationOffsetMode) {
            RotationTarget = -(new Vector2(direction.x, direction.z)).Angle();

            //Срез границ максимального значения
            if (RotationTarget < 0)
                RotationTarget += (float) (2*Math.PI * (int)(RotationTarget / (2*Math.PI)));
            else
                RotationTarget -= (float) (2*Math.PI * (int)(RotationTarget / (2*Math.PI)));
            
        }
    }

    ///Возвращает точку в глобальной системе координат, из которой будет произведён выстерл
    public Vector3 GetShotPoint() {
        return ShotPoint.GetGlobalTransform().origin;
    }

}
