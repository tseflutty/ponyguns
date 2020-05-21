using Godot;
using System;

//2019 © Даниил Белов
//Создано 01.08.2019

///Графический пользовательский интерфейс сундука
public class ChestGUI : Control
{   

    protected Entity _user;
    ///Пользователь сундука
    public Entity User {
        get { return _user; }
        set {
            _user = value;
            UpdatePickupButton();
        }
    }

    protected Chest _interactingChest;
    ///Сундук, с которым взаимодействует пользователь
    public Chest InteractingChest {
        get { return _interactingChest; }
        set {
            if (_interactingChest != null) {
                _interactingChest.Disconnect(nameof(InteractiveObject.StopInteractFrom), this, nameof(Chest_StopInteracting));
                _interactingChest.Disconnect("tree_exited", this, nameof(Chest_Removed));
            }
            _interactingChest = value;

            if (_interactingChest != null) {
                //Получение информации о предмете, лежащим в сундуке, и её занесение
                Items.ItemData data = Items.GetItemData((int)_interactingChest.ItemInside);
                Title = data.Name;
                //TODO: Установка описания
                ItemTexture = ResourceLoader.Load<Texture>(data.PathToIcon);

                //Подключение сигналов удаления
                _interactingChest.Connect(nameof(InteractiveObject.StopInteractFrom), this, nameof(Chest_StopInteracting));
                _interactingChest.Connect("tree_exited", this, nameof(Chest_Removed));
                
                UpdatePickupButton();
            }
        }
    }

    ///Объект заголовка
    protected PonyLabel TitleLabel;

    ///Объект описания
    protected PonyLabel DescriptionLabel;

    ///Кнопка "Взять"
    protected Button PickUpButton;

    ///Кнопка "Закрыть"
    protected Button CloseButton;

    ///Объект текстуры предмета
    protected TextureRect ItemTextureRect;

    ///Основной плеер анимаций
    protected AnimationPlayer Animations;

    protected string _title;
    ///Заголовок (Название предмета)
    protected string Title {
        get { return _title; }
        set {
            _title = value;
            if (TitleLabel != null) {
                TitleLabel.Text = value;
            }
        }
    }

    protected string _description;
    ///Описание предмета
    protected string Description {
        get { return _description; }
        set {
            _description = value;
            if (DescriptionLabel != null)
                DescriptionLabel.Text = value;
        }
    }

    protected Texture _itemTexture;
    //Текстура предмета
    protected Texture ItemTexture {
        get { return _itemTexture; }
        set {
            _itemTexture = value.Duplicate() as Texture;
            _itemTexture.Flags = 0;
            if (ItemTextureRect != null)
                ItemTextureRect.Texture = value;
        }
    }

    public override void _Ready() {
        ItemTextureRect = GetNode("Panel/ItemTextureRect") as TextureRect;
        TitleLabel = GetNode("Panel/TitleLabel") as PonyLabel;
        DescriptionLabel = GetNode("Panel/DescriptionLabel") as PonyLabel;
        PickUpButton = GetNode("Panel/PickUpButton") as Button;
        CloseButton = GetNode("Panel/CloseButton") as Button;
        Animations = GetNode("Animations") as AnimationPlayer;

        //Установка заголовка, описания и текстуры предмета, если не удалось установить их при изменении переменных
        TitleLabel.Text = Title;
        DescriptionLabel.Text = Description;
        ItemTextureRect.Texture = ItemTexture;

        UpdatePickupButton();
    }


    public override void _PhysicsProcess(float delta) {
        ///Привязка позции к сундуку
        Arena arena = Arena.GetCurrent();
        Vector3 chestPos = InteractingChest.GetGlobalTransform().origin;
        if (InteractingChest != null && arena != null) {
            //ВРЕМЕННО от основной камеры
            SetPosition(arena.MainCamera.UnprojectPosition(chestPos) + new Vector2(0, -InteractingChest.Heigth));
        }
    }

    public void Setup(Chest chest, Entity user) {
        InteractingChest = chest;
        User = user;
    }

    ///Коректное удаление предмета
    public void Remove(bool animated = true) {
        if (!animated) QueueFree();

        
        if (PickUpButton != null && CloseButton != null) {
            PickUpButton.Disabled = true;
            CloseButton.Disabled = true;
        }

        if (Animations != null)
            Animations.Play("Remove");
        else
            QueueFree();
    }

    //Действия по нажатию кнопки "Взять"
    protected void PickUpButton_Click() {
        //Прерать при отсутствующем пользователе или сундуке
        if (User == null || InteractingChest == null) return;
        //Если пользователь взаимодействет не с сундуком. 
        //TODO Реализовать у Entity функцию для публичноного использования с проверкой этогот (Паблик аналоги для StopInteract и SendInteractAction)
        if (User.InteractingWith != InteractingChest) return;
        DebugConsole.Shared.Output("[ChestGUI] "+User);
        User.SendInteractAction(1);
    }

    //Действия по нажатию кнопки "Закрыть"
    protected void CloseButton_Click() {
        if (User != null && InteractingChest != null && User.InteractingWith == InteractingChest)
            User.StopInteract();
        Remove();
    }

    ///Обновление кнопки "Взять"
    protected void UpdatePickupButton() {
        if (PickUpButton == null || InteractingChest == null || User == null) return;
        if (InteractingChest.ItemInside == 0 || User.EntityInventory == null) return;
        Inventory inventory = User.EntityInventory;
        Item item = Items.Get((int)InteractingChest.ItemInside);

        bool result = inventory.HasSpaceFor(item);

        PickUpButton.Disabled = !result;
        PickUpButton.Text = (result) ? "Pick up" : "No space";

        item.QueueFree();
    }

    protected void Chest_StopInteracting(Entity user, InteractiveObject _) {
        Remove();
    }

    protected void Chest_Removed() {
        InteractingChest = null;
        Remove();
    }

    protected void Animations_AnimationFinished(string animName) {
        if (animName == "Remove")
            QueueFree();
    }
}
