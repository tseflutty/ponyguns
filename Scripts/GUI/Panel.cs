using Godot;
using System;

public class Panel : Control
{
    
    public override void _Draw() {
        DrawRect(new Rect2(-6, -6, RectSize.x + 12, RectSize.y + 12), new Color(0.41176f, 0.34901f, 0.48235f, 1));
        DrawRect(new Rect2(0, 0, RectSize.x, RectSize.y), new Color(0, 0, 0, 1));
    }

}
