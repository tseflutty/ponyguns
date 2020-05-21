using Godot;
using System;

public class HealthBarController : Node
{
    protected PackedScene PackedHealthBar;

    public override void _Ready() {
        PackedHealthBar = ResourceLoader.Load<PackedScene>("res://GUI/Entity/PortableHealthBar.tscn");

        //Подключение сигналов о создании и удалении интерактивнов объектов
        Entity.Instances.Connect(nameof(PonyNodeList.ItemAdded), this, nameof(NewEntity));
        Entity.Instances.Connect(nameof(PonyNodeList.ItemRemoved), this, nameof(EntityRemoved));
        
        //Подключение уже созданных
        foreach (Node e in Entity.Instances.GetArr())
            NewEntity(e);
    }

    protected void NewEntity(Node node) {
        if (!(node is Entity)) return;
        Entity e = node as Entity;

        Godot.Collections.Array bindsForEntitySignal = new Godot.Collections.Array();
        bindsForEntitySignal.Add(e);

        e.Connect(nameof(Entity.EntityInjured), this, nameof(Enity_Injured), bindsForEntitySignal);
    }

    protected void EntityRemoved(Node node) {
        if (!(node is Entity)) return;
        Entity e = node as Entity;

        e.Disconnect(nameof(Entity.EntityInjured), this, nameof(Enity_Injured));
    }


    //Когда сушности был нанесён урон
    protected void Enity_Injured(int damage, Spatial damager, Entity damaged) {
        if (damager is Entity) {
            Entity eDamager = damager as Entity;
            if (eDamager.IsPlayer && !damaged.IsPlayer) {
                ShowHealthBar(eDamager.PlayerID, damaged, 3);
            }
        }
    }

    ///Показать хп бар сущности (from) для игрока под номером (playerID) с задержкеой removeDelay
    ///Если хп бар уже показан на большее время, чем (removeDelay), то время удаления не будет изменено
    protected void ShowHealthBar(byte playerID, Entity from, float removeDelay) {
        ArenaGUI gui = ArenaGUI.GetCurrent();
        if (gui == null) return;

        ObjectPanels obp = gui.GetObjectPanels(playerID);
        if (obp == null) return;

        //Получаем текущую панель сущности (или null)
        Control currPanel = obp.GetObjectPanel(from);

        //Если текущая панель это хп бар, обновляем счётчик ожидания удаления и прекращает выаполнение функции
        if (currPanel != null && currPanel is PortableHealthBar) {
            PortableHealthBar currBar = currPanel as PortableHealthBar;
            if (currBar.RemoveDelay < removeDelay)
                currBar.RemoveDelay = removeDelay;
            currBar.ResetRemoveWait();
            return;
        }

        PortableHealthBar newBar = PackedHealthBar.Instance() as PortableHealthBar;
        newBar.ResearchEntity = from;
        newBar.RemoveDelay = removeDelay;

        obp.ShowObjectPanel(from, new Vector2(0, -from.Height), newBar);
            
    }

    public override void _PhysicsProcess(float delta) {
        ///ВРЕМЕННО от основной камеры для первого игрока
        ///TODO Исправить и придумать что-нибудь лучше
        Arena arena = Arena.GetCurrent();
        if (arena != null) {
            Spatial curCollider = arena.MainCamera.CursorCollider;
            if (curCollider is Entity) {
                Entity curEntity = curCollider as Entity;
                if (curEntity.PlayerID != 1)
                    ShowHealthBar(1, curEntity, 0.1f);
            }
        }
    }

}
