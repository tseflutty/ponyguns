using Godot;
using System;

//2019 © Даниил Белов
//Создано 31.07.2019

public class Chest : InteractiveObject
{


    protected AnimationPlayer Animations;
    
    [Export]
    ///При создании сундука рандомно выберается id предмета из указанных здесь, который будет положен в сундук
    public int[] ItemIDs = new int[0];

    protected uint _itemInside = 0;

    [Export]
    ///ID предмета, лежащий внутри сундука в данный момент. Если равен 0, то в сундуке пусто
    public uint ItemInside {
        get { return _itemInside; }
        set {
            _itemInside = value;
            if (value != 0 && IsOpen && Users.Count == 0) {
                PlayAnimation("Close");
            }
            if (value == 0 && !IsOpen && Users.Count == 0) {
                PlayAnimation("Open");
            }
            //TODO: Подавать сигнал о изменении предмета
        }
    }

    ///Открыт ли сундук в данный момент
    protected bool IsOpen = false;

    protected bool _readyToUse = false;
    protected bool ReadyToUse {
        get { return _readyToUse; }
        set {
            _readyToUse = value;
            if (value) {
                ///Разослать сообщение всем пользователям о готовности использования
                Entity[] users = new Entity[Users.Count];
                for (int i = 0; i < Users.Count; ++i)
                    users[i] = Users[i];
                foreach (Entity user in users)
                    SendInteractMessage(InteractMessage.ReadyToUse, user);
            }
        }
    }

    public override void _Ready() {
        Animations = GetNode("Animations") as AnimationPlayer;
        //Выбор рандомного предмета из ItemIDs для ItemInside
        if (ItemIDs.Length != 0)
            ItemInside = (uint)ItemIDs[PonyRandom.Rnd.Next(0, ItemIDs.Length)];
        ReadyToUse = false;
    }

    ///Коректно запустить анимацию
    protected void PlayAnimation(string animName) {
        if (Animations.CurrentAnimation != animName) {
            if (animName == "Open") {
                IsOpen = true;
            } else if (animName == "Close") {
                IsOpen = false;
            }
        }
        Animations.Play(animName);
    }

    //Во время начала взаимодействия
    protected void ChestInteractStartFrom(Entity user, InteractiveObject _) {
        //Запуск анимации
        PlayAnimation("Open");
    }


    //При окончании взаимодействия
    protected void ChestInteractStopFrom(Entity user, InteractiveObject _) {
        if (ItemInside != 0)
            PlayAnimation("Close");
    }

    
    public override int _GetInteractActionFromEntity(Entity from, int interactAction) {
        if (!Users.Contains(from)) return 0;
        if (!ReadyToUse) return 0;
        base._GetInteractActionFromEntity(from, interactAction);

        //Выдать предмет если пользователь совершил действие взаимодействия 1
        if (interactAction == 1) {
            Item itemToAdd = Items.Get((int)ItemInside);
            DebugConsole.Shared.Output("[Chest] "+ItemInside);
            //Если удалось выдать предмет, удалить предмет из сундука
            if (from.EntityInventory.AddItem(itemToAdd)) {
                ItemInside = 0;
                SendInteractMessage(InteractMessage.OK, from);
            } else {
                itemToAdd.QueueFree();
                SendInteractMessage(InteractMessage.No, from);
            }
        }

        return 0;
    }

    public override void _PhysicsProcess(float delta) {
        Enable = ItemInside != 0;
        base._PhysicsProcess(delta);
    }

    protected void Animations_AnimationFinished(string animName) {
        if (animName == "Open")
            ReadyToUse = true;
        if (animName == "Close")
            ReadyToUse = false;
    }

}
