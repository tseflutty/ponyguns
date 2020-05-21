using Godot;
using System;

//2019 © Даниил Белов
//Создано 26.08.2019

public class InventoryBasedObject : InteractiveObject
{
    protected AdvancedInventory _content;
    ///Инвентарь этого объекта
    ///Не подключать инвентарь, используемый другим объектом
    public virtual AdvancedInventory Content {
        get { return _content; }
        set {
            if (_content != null) {
                _removeFromUpdateContentValue = true;
                RemoveChild(_content);
                _content.Disconnect("tree_exited", this, nameof(_Content_Removed));
            }

            _content = value;

            foreach (Entity user in Users.ToArray()) {
                StopInteract(user);
            }

            if (Content != null) {
                AddChild(Content);
                Content.Connect("tree_exited", this, nameof(_Content_Removed));
            }
        }
    }

    ///Фиксит краш при смене значения Content
    protected bool _removeFromUpdateContentValue = false;

    ///Свойства для инициализации инвентаря из инспектора
    [Export]
    int[] GroupSizes = new int[1] {5};

    [Export]
    string[] GroupNames = new string[1] {"Иневентарь"};

    [Export]
    string InventoryName = "Инвентарь";
    
    ///При начале взаимодействия
    protected void _InventoryBasedObject_StartInteract(Entity user, InteractiveObject _) {
        GD.Print("[InventoryBasedObject] START INTERACT");
        if (Content != null) {
            Content.GiveAccessToExchange(user.EntityInventory);
            GD.Print("[InventoryBasedObject] Acces OK");
        }
    }

    ///После начала взаимодействия
    protected void _InventoryBasedObject_StartedInteract(Entity user, InteractiveObject _) {
        Godot.Collections.Array binds = new Godot.Collections.Array();
        binds.Add(user);
        user.Connect(nameof(Entity.InventoryChanged), this, nameof(User_InventoryChanged), binds);
        if (user.EntityInventory == null) {
            StopInteract(user);
        }
    }

    ///При остановке взаимодействия
    protected void _InventoryBasedObject_StopInteract(Entity user, InteractiveObject _) {
        user.Disconnect(nameof(Entity.InventoryChanged), this, nameof(User_InventoryChanged));
        if (user.EntityInventory == null) {
            Content.CancelAccessToExchange(user.EntityInventory);
        }
    }

    public override void _Ready() {
        ///Инициализация инвентаря из инспектора
        if (GroupSizes.Length != GroupNames.Length) {
            OS.Alert("GroupSizes.Length not equals GroupsNames.Length", "InventoryBasedObject Error");
        } else {
            Content = new AdvancedInventory(InventoryName, GroupSizes, GroupNames);
        }
    }

    public void User_InventoryChanged(Entity user) {
        StopInteract(user);
    }

    protected void _Content_Removed() {
        if (_removeFromUpdateContentValue) {
            _removeFromUpdateContentValue = false;
            return;
        }
        Content = null;
    }
    

}
