using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 09.05.2019

public class InventoryGUI : Control
{
    protected Inventory _slave;
    ///<summary>
    ///<para>Инвентарь, который был взят в рабство этим графическим интерфейсом</para>
    ///</summary>   
    public virtual Inventory Slave {
        get { return _slave; }
        set {
            _slave = value;
            // GD.Print("Connect");
            value.Connect(nameof(Inventory.ItemRemoved), this, nameof(ItemRemoved));
            value.Connect(nameof(Inventory.ItemAdded), this, nameof(ItemAdded));
            UpdateAllGUISlots();
        }
    }

    ///Возвращает инвентарь который будет использоваться в этом интерфейск
    public virtual Inventory _InventoryToUse {
        get {
            // GD.Print("OLD!");
            return Slave;
        }
    }

    ///Массив визуальных слотов инвентаря  
    protected List<List<InventoryGUISlot>> GUISlots = new List<List<InventoryGUISlot>>();

    ///Визуальные заголовки групп
    private PonyLabel[] GroupTitles;

    private PackedScene PackedGUISlot;

    private PackedScene PackedPonyLabel;

    protected ContextMenu Context;

    protected TextureRect MovableItemTexture;
    
    //Блокировка контекстного меню до следующей попытки открыть его.
    //На данный момент фиксить баг открывания контекстного меню после перемещения предмета
    protected bool OneBlockContext = false;

    //Выбранные игроком слоты для контекстного меню
    protected int SelectedGroup = 0, SelectetSlot = 0;


    protected Item MovableItem; //Передвигаемый предмет
    protected int MovableFromGroup; //Из какой группы проиcходит передвижение
    protected int MovableFromSlot; //Из какого слота проиcходит передвижение

    private const float padding = 12;
    private const float space = 6; //Отступы между слотами

    protected int _maxSlotsColumns = 5;
    [Export]
    ///Максимальное количество столбцов слотов одной группы
    public int MaxSlotsColumns {
        get { return _maxSlotsColumns; }
        set {
            _maxSlotsColumns = value;
            if (value <= 0) _maxSlotsColumns = 1;
            if (PackedGUISlot != null)
                UpdateAllGUISlots();
        }
    }


    public override void _Ready() {
        PackedGUISlot = ResourceLoader.Load("res://GUI/Inventory/InventoryGUISlot.tscn") as PackedScene;
        PackedPonyLabel = ResourceLoader.Load("res://GUI/PonyLabel.tscn") as PackedScene;
        Context = GetNode("ContextMenuLayer/ContextMenu") as ContextMenu;
        MovableItemTexture = GetNode("MovableItemTexture") as TextureRect;
    }

    public override void _Draw() {
        //Подгонка под размеры заголовков групп
        if (GroupTitles == null) return;
        foreach (PonyLabel label in GroupTitles) {
            float panelX = RectSize.x;
            float labelX = label.Size.x;
            if (labelX + padding*2 > RectSize.x) panelX = labelX + padding*1.5f;
            RectSize = new Vector2(panelX, RectSize.y);
        }
    }

    public override void _Input(InputEvent e) {
        //Следование пиктограммы перемещаемого предмета если какой либо предмет перемещается
        if (e is InputEventMouseMotion && isItemMoveMode) {
            MovableItemTexture.SetGlobalPosition(GetGlobalMousePosition() - MovableItemTexture.RectSize/2);
        }
    }

    ///<summary>
    ///<para>Показать инвентарь</para>
    ///</summary>   
    public void ShowGUI(bool animated) {
        UpdateAllGUISlots();
        Show();
    }
    

    protected bool _isItemMoveMode = false;
    //Информация для режима перемещения предмета по инвентарю игроком
    protected bool isItemMoveMode { //Активен ли режим
        get { return _isItemMoveMode; }
        set {
            _isItemMoveMode = value;
            if (value) {
                //Включает слушание у слотов
                foreach (List<InventoryGUISlot> g in GUISlots)
                    foreach (InventoryGUISlot s in g)
                        s.ListenAdding = true;
            } else {
                //Выключает слушание у слотов
                foreach (List<InventoryGUISlot> g in GUISlots)
                    foreach (InventoryGUISlot s in g)
                        s.ListenAdding = false;
            }
        }
    }

    protected void UpdateAllGUISlots() {
        isItemMoveMode = false;
        MovableItemTexture.Hide();
        
        GD.Print("[InventoryGUI] Trying start update");
        //Не обновлять если инвентаря нет
        if (_InventoryToUse == null) return;
        GD.Print("[InventoryGUI] UpdateSterted");
        
        RemoveChild(MovableItemTexture);

        //Удаление предыдущих слотов
        foreach (List<InventoryGUISlot> group in GUISlots)
            foreach (InventoryGUISlot slot in group)  slot.QueueFree();

        //Пересоздание массива
        GUISlots = new List<List<InventoryGUISlot>>();

        //Удаление предыдущих заголовков груп
        if (GroupTitles != null)
            foreach (PonyLabel l in GroupTitles) l.QueueFree();
        
        //Пересоздание массива заголовков
        GroupTitles = new PonyLabel[_InventoryToUse.GetGroupsCount()];

        //Размер панели после облновления
        Vector2 panelSize = new Vector2(0, padding*1.5f);

        int groupCount = _InventoryToUse.GetGroupsCount();

        //Цикл обновления
        for (int group = 0; group < groupCount; ++group) {
            // GD.Print("[InventoryGUI] Group created");

            //Создание массива для группы
            GUISlots.Add(new List<InventoryGUISlot>());

            //Создание нового заголовка
            PonyLabel newTitle = PackedPonyLabel.Instance() as PonyLabel;
            newTitle.Text = "<h2>"+_InventoryToUse.GetGroupName(group)+"</h2>";
            newTitle.FontColor = new Color(0.949019f, 0.949019f, 0.949019f, 1);
            // GD.Print("Name: ", _InventoryToUse.GetGroupName(group));
            //По Y: (Высота слота + кегль шрифта) * индекс группы - верхних выносных элемнтов шрифта + padding * 1.5
            newTitle.RectPosition = new Vector2(
                padding, 
                panelSize.y - 6
            );
            newTitle.RectSize = new Vector2(panelSize.x - padding*2 , 0);
            AddChild(newTitle);
            GroupTitles[group] = newTitle;

            //Кол-во слотов после создания в этой групп
            int countSlotsInGroup = 0;
            for (int slot = 0; slot < _InventoryToUse.GetSlotCountOfGroup(group); ++slot) {
                // GD.Print("[InventoryGUI] Slot created");
                //Создание нового слота
                InventoryGUISlot newSlot = PackedGUISlot.Instance() as InventoryGUISlot;
                //По Y: (Высота слота + кегль шрифта + padding) * индекс группы + кегль шрифта + padding * 1.5
                newSlot.RectPosition = new Vector2(
                    (newSlot.RectSize.x + space) * (slot % MaxSlotsColumns) + padding,
                    panelSize.y + 36 + (newSlot.RectSize.y + space) * (countSlotsInGroup / MaxSlotsColumns)
                );
                newSlot.ItemOfSlot = _InventoryToUse.GetItemAt(group, slot);
                newSlot.SetInventory(_InventoryToUse);
                newSlot.Group = group;
                newSlot.Slot = slot;
                newSlot.Connect(nameof(InventoryGUISlot.ItemStartMove), this, nameof(_ItemStartMove));
                newSlot.Connect(nameof(InventoryGUISlot.AddingListened), this, nameof(_AddingListened));
                newSlot.Connect(nameof(InventoryGUISlot.SlotClicked), this, nameof(SlotClicked));
                AddChild(newSlot);
                GUISlots[group].Add(newSlot);

                //Обновление кол-ва слотов
                countSlotsInGroup++;
            }

            //Обновление ширины
            float newPanelWidht = (51 + space) * ((countSlotsInGroup < MaxSlotsColumns) ? countSlotsInGroup : MaxSlotsColumns) 
                + padding + padding - space;
            // GD.Print(newPanelWidht, " ", countSlotsInGroup);
            if (panelSize.x < newPanelWidht) panelSize.x = newPanelWidht;

            //Обновление высоты (Высота слота + кегль шрифта + padding)
            panelSize.y += 36 + padding + (51 + space) * ( ((countSlotsInGroup-1) / MaxSlotsColumns) + 1 );
        }
        
        //Создание новых заголовков
        // for (int group = 0; group < groupCount; ++group) {
        //     PonyLabel newTitle = PackedPonyLabel.Instance() as PonyLabel;
        //     newTitle.Text = "<h2>"+_InventoryToUse.GetGroupName(group)+"</h2>";
        //     newTitle.FontColor = new Color(0.949019f, 0.949019f, 0.949019f, 1);
        //     // GD.Print("Name: ", _InventoryToUse.GetGroupName(group));
        //     //По Y: (Высота слота + кегль шрифта) * индекс группы - верхних выносных элемнтов шрифта + padding * 1.5
        //     newTitle.RectPosition = new Vector2(
        //         padding, 
        //         (51 + 36 + padding) * group - 6 + padding*1.5f
        //     );
        //     newTitle.RectSize = new Vector2(panelSize.x - padding*2 , 0);
        //     AddChild(newTitle);
        //     GroupTitles[group] = newTitle;
        // }

        panelSize.y += padding * 0.5f - space;

        AddChild(MovableItemTexture);

        //Обновление размера в панеле
        RectSize = panelSize; //После обновления размра движок сам вызывает _Draw(), который подравнивает ширину с учётом ширины заголовков названий групп
    }

    ///<summary>
    ///<para>Скрыть инвентарь</para>
    ///</summary>   
    public void HideGUI(bool animated) {
        

        Hide();
    }
    
    public void ItemRemoved(int group, int slot) {
        GD.Print("[GUI] ITEM RM ", group, "@", slot);
        (GUISlots[group])[slot].ItemOfSlot = null;
    }

    public void ItemAdded(int group, int slot) {
        (GUISlots[group])[slot].ItemOfSlot = _InventoryToUse.GetItemAt(group, slot);
    }

    //Когда пользователь нажимает кнопку "переместить", сюда подаётся сигнал
    public virtual void _ItemStartMove(int group, int slot) {
        isItemMoveMode = true;
        MovableItem = _InventoryToUse.GetItemAt(group, slot);
        MovableFromGroup = group;
        MovableFromSlot = slot;
        //Предмет удаляется в графическом интерфейсе, но не удаляется с самого инвентаря чтобы при сохранении игры во время перемещения
        //предмет не пропал в файле сохранения
        (GUISlots[group])[slot].ItemOfSlot = null;

        //Отображение пиктограммы перемещаемого предмета
        MovableItemTexture.Show();
        Texture ItemTexture = ResourceLoader.Load(MovableItem.PathToIcon).Duplicate() as Texture;
        ItemTexture.Flags = 0;
        MovableItemTexture.Texture = ItemTexture;
        MovableItemTexture.SetGlobalPosition(GetGlobalMousePosition() - MovableItemTexture.RectSize/2);
        OneBlockContext = true;
    }

    //Когда игрок нажимает на слот, куда хочет переместить предмет, сюда подаётся сигнал
    protected virtual void _AddingListened(int group, int slot) {
        if (!isItemMoveMode) return;

        MovableItemTexture.Hide();

        //Установлено True, если предмент был перемещён в тот же слот, откуда и начал перемещение
        bool isMoveCanceled = false;

        //Фикс проблемы перемещения туда же, откуда и начато перемещение
        if (group == MovableFromGroup && slot == MovableFromSlot && (GUISlots[group])[slot].ItemOfSlot == null) {
            _InventoryToUse.RemoveItemAt(group, slot);
            isMoveCanceled = true;
        }

        bool isMoved = _InventoryToUse.AddItemTo(group, slot, MovableItem);
        if (!isMoved) {
            //Случай, если перемещение в слот с другим предметом (Свап)
            if (_InventoryToUse.GetItemAt(group, slot) != null) {
                Item temp = _InventoryToUse.GetItemAt(group, slot);
                _InventoryToUse.RemoveItemAt(group, slot);
                _InventoryToUse.RemoveItemAt(MovableFromGroup, MovableFromSlot);
                _InventoryToUse.AddItemTo(group, slot, MovableItem);
                _InventoryToUse.AddItemTo(MovableFromGroup, MovableFromSlot, temp);
                isItemMoveMode = false;
                _ItemStartMove(MovableFromGroup, MovableFromSlot);
            }
            return;
        }
        //GD.Print("[Inventory GUI] Adding Сomplete");

        //Удаление предмета в инвентаре с предыдущего если предмет был перемещён в другой слот
        if (!isMoveCanceled)
            _InventoryToUse.RemoveItemAt(MovableFromGroup, MovableFromSlot);

        isItemMoveMode = false;
    }

    protected virtual void SlotClicked(int group, int slot) {
        if (_InventoryToUse.GetItemAt(group, slot) == null) return;
        if (isItemMoveMode) return;
        if (OneBlockContext) {
            OneBlockContext = false;
            return;
        }
        Context.Title = _InventoryToUse.GetItemAt(group, slot).ItemName;
        Context.ShowIn(GetGlobalMousePosition());
        SelectedGroup = group;
        SelectetSlot = slot;
    }

    private void ContextMenuItemSelected(int i) {
        switch (i) {
            //Переместить
            case 0:
                _ItemStartMove(SelectedGroup, SelectetSlot);
                break;
            //Информация
            case 1:
                break;
            //Удалить
            case 2:
                _InventoryToUse.RemoveItemAt(SelectedGroup, SelectetSlot);
                break;
        }
    }

    private void VisibilityChanged() {
        if (Visible) UpdateAllGUISlots();
    }
    

}
