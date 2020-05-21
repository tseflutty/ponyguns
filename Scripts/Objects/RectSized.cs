using Godot;
using System;


///Объект, имеющий сведения о своих границах на плоскостях паралепипеда относительно нулевой точки объекта
public interface RectSized
{
    float LeftRectOffset {get; set;}

    float RightRectOffset {get; set;}

    float FrontRectOffset {get; set;}

    float BackRectOffset {get; set;}

    float TopRectOffset {get; set;}

    float BottomRectOffset {get; set;}

    Vector3 ZeroOffset {get; set;}

}
