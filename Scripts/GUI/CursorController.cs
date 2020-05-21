using Godot;
using System;

//Отображение управление курсором
//Курсор
public class CursorController : Node2D
{
    //Получить объект используемого контроллера курсора
    public static CursorController Sharred;

    //Ссылки на текстуры курсоров
    [Export]
    public string[] Cursors = new string[0];

    private bool SystemCursorHidden = false;

    //Текущая текстура курсора
    private Texture CurrentCursor;
    public override void _Ready() {
        //Прятаем курсор
        Input.SetMouseMode(Input.MouseMode.Hidden);
        SystemCursorHidden = true;

        //Установка первого курсора в качестве основного
        if (Cursors.Length != 0) CurrentCursor = ResourceLoader.Load(Cursors[0]) as Texture;
        
        Sharred = this;
    }
    
    public override void _Draw() {
        //Рисуем круг в качесве курсора, если не установлена текущая текстура курсора
        if (CurrentCursor == null) {
            DrawCircle(GetGlobalMousePosition(), 15, new Color(0, 0, 0, 0.8f));
            DrawCircle(GetGlobalMousePosition(), 10, new Color(1, 1, 1, 0.8f));
            return;
        }
        //Рисуем курсор
        DrawTexture(CurrentCursor, GetGlobalMousePosition() - CurrentCursor.GetSize()/2);
    }

    public override void _PhysicsProcess(float delta) {
        //Каждый тик обновляем отрисовку
        Update();

        //Показываем системный курсор, если он за пределами окна игры
        // if (SystemCursorHidden) {
        //     Rect2 WindowRect = new Rect2(OS.GetWindowPosition(), OS.GetWindowSize());
        //     if (!WindowRect.HasPoint(GetGlobalMousePosition())) {
        //         Input.SetMouseMode(Input.MouseMode.Visible);
        //         SystemCursorHidden = false;
        //     }
        // } else {
        //     Rect2 WindowRect = new Rect2(OS.GetWindowPosition(), OS.GetWindowSize());
        //     if (WindowRect.HasPoint(GetGlobalMousePosition())) {
        //         Input.SetMouseMode(Input.MouseMode.Hidden);
        //         SystemCursorHidden = true;
        //     }
        // }

        // Rect2 WindowRect = new Rect2(OS.GetWindowPosition(), OS.GetWindowSize());
        // Vector2 GameResolution = GetViewportRect().Size;
        // float RatioAspect = GameResolution.y / GameResolution.x;
        // float WinScale = (WindowRect.Size.y > WindowRect.Size.x * RatioAspect) ? GameResolution.x / WindowRect.Size.x : GameResolution.y / WindowRect.Size.y;
        // if (SystemCursorHidden) {
        //     if (!WindowRect.HasPoint(GetGlobalMousePosition() * WinScale)) {
        //         Input.SetMouseMode(Input.MouseMode.Visible);
        //         SystemCursorHidden = false;
        //     }
        // } else {
        //     if (WindowRect.HasPoint(GetGlobalMousePosition() * WinScale)) {
        //         Input.SetMouseMode(Input.MouseMode.Hidden);
        //         SystemCursorHidden = true;
        //     }
        // }
    }

    //Установка текстуры курсора по индексу из массива ссылока на текстуры курсора
    public void SetCursorByIndex(int i) {
        if (Cursors.Length > i) CurrentCursor = ResourceLoader.Load(Cursors[i]) as Texture;
    }
}
