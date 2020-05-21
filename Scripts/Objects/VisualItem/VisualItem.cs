using Godot;
using System;

//2019 © Даниил Белов
//Создано 14.05.2019

///<summary>
///<para>Визуальное представление предмета. Объект предмента, который отображается непосредственно на сцене</para>
///</summary>
public class VisualItem : Spatial
{

    protected Item ItemOfVisual;

    //Теги. Обычно является тегами слота, в которым лежит предмет отображения
    protected int[] Tags;

    protected Sprite3D ItemTexture {
        get { return GetNode("ItemTexture") as Sprite3D; }
    }

    ///Запустить анимацию под названием (name). Защищает от ошибки несуществующей анимации
    public void RunAnimation(string name) {
        AnimationPlayer animations = GetNode("ItemAnimations") as AnimationPlayer;
        animations.Stop();
        if (animations.HasAnimation(name))
            animations.Play(name);
    }

    //Обработка тегов слота
    protected virtual void UpdateTags() {}
    
    ///<summary>
    ///<para>Настроить это визуальное представление для предмета (item)</para>
    ///</summary>
    public virtual void Setup(Item item, int[] tags) {
        ItemOfVisual = item;
        Tags = tags;
        ItemTexture.Texture = ResourceLoader.Load(item.PathToIcon) as StreamTexture;
        if (Tags != null) UpdateTags();
    }
    
    ///Возвращает предмет в инвентаре, соответствующий этому визуальному представлению
    public Item GetItem() {
        return ItemOfVisual;
    }

    public virtual void SetGunRotation(Vector3 direction) {}
}
