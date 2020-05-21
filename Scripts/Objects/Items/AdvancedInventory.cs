using Godot;
using System;
using System.Collections.Generic;

//2019 © Даниил Белов
//Создано 19.08.2019

///!!! Для нормальной работоспособности должен добавлен в дерево с помощью AddChild(...) !!!
///Инвентарь с расширенным функционалом:
///Поддержка обмена между инвентарями, а так же запросов получения/дарения предмета
///Для обмена через запросы обмена не нужно дававть разрешение обмена.
///В случае свободного обмена для одной из стороны надо дать этой стороне разрешение
public class AdvancedInventory : Inventory
{
    [Signal]
    public delegate void ExchangeRequestAccepted(Item ItemOfRequest, AdvancedInventory From, ExchangeRequestType Type);

    [Signal]
    public delegate void ExchangeRequestReceived(Item ItemOfRequest, AdvancedInventory From, ExchangeRequestType Type);

    [Signal]
    public delegate void ExchangeRequestCanceled(Item ItemOfRequest, AdvancedInventory From, ExchangeRequestType Type);

    ///Список запросов, ожидающтих проверки на существования прдмета(объекта запроса) в ближней обработке PhysicsProcess
    protected List<ExchangeRequest> _ExchangeRequestToCompareItems = new List<ExchangeRequest>();

    protected List<ExchangeRequest> _CurrentExchangeRequests = new List<ExchangeRequest>();

    ///Словарь в формате Инветарь (От кого), количество запросов
    protected Dictionary<AdvancedInventory, int> _ConnectedInventories = new Dictionary<AdvancedInventory, int>();
    
    public List<AdvancedInventory> ExchangeAcceses = new List<AdvancedInventory>();

    ///Максимальное количество запросов обмена, которые можно запросить у этого инвентаря
    public int MaxExchangeRequests = 6;

    ///Время отмены игнорируемого запроса в секундах
    public float RequestCancelTime = 10;

    public override void _Ready() {
        base._Ready();
        //Подключение своих сигналов к себе
        Connect(nameof(Inventory.ItemRemoving), this, nameof(_SomeInventory_ItemRemoving));
    }

    ///Отпрваить запрос типа (type) по координатам (fromGroup, fromSlot) к инвентарю (inventory)
    public bool SendExchangeRequest(ExchangeRequestType type, int fromGroup, int fromSlot, Inventory inventory) {
        Item requestObject = (type == ExchangeRequestType.Give) ? GetItemAt(fromGroup, fromSlot) : inventory.GetItemAt(fromGroup, fromSlot);
        if (requestObject == null) return false;
        
        return SendExchangeRequest(type, requestObject, inventory);
    }

    ///Отпрваить запрос типа (type) на предмет (requestObject) к инвентарю (inventory)
    public bool SendExchangeRequest(ExchangeRequestType type, Item requestObject, Inventory inventory) {
        if (inventory == null) return false;
        if (!(inventory is AdvancedInventory)) return false;
        AdvancedInventory to = inventory as AdvancedInventory;
        
        return to._ReceiveExchangeRequest(this, requestObject, type);
    }

    protected bool _ReceiveExchangeRequest(AdvancedInventory from, Item requestItem, ExchangeRequestType type) {
        if (from == null) return false;
        if (type == ExchangeRequestType.Give)
            if (!from.HasItem(requestItem)) return false;
        else if (type == ExchangeRequestType.Get)
            if (!HasItem(requestItem)) return false;
        if (_CurrentExchangeRequests.Count >= MaxExchangeRequests) return false;
        
        ExchangeRequest request = new ExchangeRequest(requestItem, from, type, RequestCancelTime);

        ///Отклонить если такой запрос уже есть
        foreach (ExchangeRequest req in _CurrentExchangeRequests) {
            if (req.From == request.From && req.ItemOfRequest == request.ItemOfRequest) {
                return false;
            }
        }

        _CurrentExchangeRequests.Add(request);
        EmitSignal(nameof(ExchangeRequestReceived), request.ItemOfRequest, request.From, request.Type);

        //Подключение сигнала и обновление кол-ва активных запросов
        if (!_ConnectedInventories.ContainsKey(from)) {
            _ConnectedInventories.Add(from, 0);

            Godot.Collections.Array bindsForInventorySignal = new Godot.Collections.Array();
            bindsForInventorySignal.Add(from);
            from.Connect("tree_exited", this, nameof(_SenderInventoryRemoved), bindsForInventorySignal);

            from.Connect(nameof(Inventory.ItemRemoving), this, nameof(_SomeInventory_ItemRemoving));
        }
        _ConnectedInventories[from]++;

        GD.Print("[AdvancedInventory] Request received");

        return true;
    }

    ///Плолучить ожидающие в данный момент запросы обмена к этому инвентарю
    public ExchangeRequest[] GetExchangeRequests() { return _CurrentExchangeRequests.ToArray(); }

    public bool AcceptExchangeRequest(int requestIndex) {
        if (_CurrentExchangeRequests.Count <= requestIndex) return false;

        ExchangeRequest request = _CurrentExchangeRequests[requestIndex];
        
        if (request.Type == ExchangeRequestType.Give) {
            if (!request.From.HasItem(request.ItemOfRequest)) return false;
            if (!AddItem(request.ItemOfRequest)) return false;
            request.From.RemoveItem(request.ItemOfRequest);
        } else {
            if (!HasItem(request.ItemOfRequest)) return false;
            if (!request.From.AddItem(request.ItemOfRequest)) return false;
            RemoveItem(request.ItemOfRequest);
        }

        _ExchangeRequestToCompareItems.Remove(request);
        _CurrentExchangeRequests.Remove(request);
        EmitSignal(nameof(ExchangeRequestAccepted), request.ItemOfRequest, request.From, request.Type);

        return true;
    }

    

    ///Дать инвентарю (inventory) доступ к взаимодействию с этим инвентарём
    public bool GiveAccessToExchange(Inventory inventory) {
        if (!(inventory is AdvancedInventory)) return false;
        AdvancedInventory inv = inventory as AdvancedInventory;

        if (!ExchangeAcceses.Contains(inv))
            ExchangeAcceses.Add(inv);
        return true;
    }

    ///Забрать у инвентаря (inventory) доступ к взаимодействию с этим инвентарём
    public bool CancelAccessToExchange(Inventory inventory) {
        if (!(inventory is AdvancedInventory)) return false;
        AdvancedInventory inv = inventory as AdvancedInventory;

        return ExchangeAcceses.Remove(inv);
    }

    public bool HasAccesToExchange(Inventory inventory) {
        if (!(inventory is AdvancedInventory)) return false;
        AdvancedInventory inv = inventory as AdvancedInventory;
        return ExchangeAcceses.Contains(inv) || inventory == this;
    }

    ///Забрать предмет у инвентаря (from) по коардинатам (fromGoup, fromSlot) в этот инвентарь по координатам (toGroup, toSlot)
    ///если инвентарь (from) дал разрешение этому инвентарю на взаимодействие
    ///Возращает True, если успешно, иначе False
    ///Инвентарь должен быть типа AdvancedInventory
    public bool ExchangeGetItem(Inventory from, int fromGroup, int fromSlot, int toGroup, int toSlot) {
        if (from == null) return false;
        if (!(from is AdvancedInventory)) return false;
        if (!(from as AdvancedInventory).HasAccesToExchange(this)) return false;

        Item itemForExchange = from.GetItemAt(fromGroup, fromSlot);
        if (itemForExchange == null) return false;
        
        if (AddItemTo(toGroup, toSlot, itemForExchange)) {
            from.RemoveItem(itemForExchange);
        } else {
            return false;
        }

        return true;
    }

    ///Забрать предмет у инвентаря (from) по коардинатам (fromGoup, fromSlot) в этот инвентарь
    ///если инвентарь (from) дал разрешение этому инвентарю на взаимодействие
    ///Возращает True, если успешно, иначе False
    ///Инвентарь должен быть типа AdvancedInventory
    public bool ExchangeGetItem(Inventory from, int fromGroup, int fromSlot) {
        if (from == null) return false;
        if (!(from is AdvancedInventory)) return false;
        if (!(from as AdvancedInventory).HasAccesToExchange(this)) return false;

        Item itemForExchange = from.GetItemAt(fromGroup, fromSlot);
        if (itemForExchange == null) return false;
        
        if (AddItem(itemForExchange)) {
            from.RemoveItem(itemForExchange);
        } else {
            return false;
        }

        return true;
    }

    ///Дать предмет инвентарю (from) в координаты (toGroup, toSlot) из этого инвентаря по координатам (fromGroup, fromSlot)
    ///если инвентарь (from) дал разрешение этому инвентарю на взаимодействие
    ///Возращает True, если успешно, иначе False
    ///Инвентарь должен быть типа AdvancedInventory
    public bool ExchangeGiveItem(Inventory to, int toSlot, int toGroup, int fromGroup, int fromSlot) {
        if (to == null) return false;
        if (!(to is AdvancedInventory)) return false;
        if (!(to as AdvancedInventory).HasAccesToExchange(this)) return false;

        Item itemForExchange = GetItemAt(fromGroup, fromSlot);
        if (itemForExchange == null) return false;
        
        if (to.AddItemTo(toGroup, toSlot, itemForExchange)) {
            RemoveItem(itemForExchange);
        } else {
            return false;
        }

        return true;
    }

    ///Дать предмет инвентарю (from) из этого инвентаря по координатам (fromGroup, fromSlot)
    ///если инвентарь (from) дал разрешение этому инвентарю на взаимодействие
    ///Возращает True, если успешно, иначе False
    ///Инвентарь должен быть типа AdvancedInventory
    public bool ExchangeGiveItem(Inventory to, int fromGroup, int fromSlot) {
        if (to == null) return false;
        if (!(to is AdvancedInventory)) return false;
        if (!(to as AdvancedInventory).HasAccesToExchange(this)) return false;

        Item itemForExchange = GetItemAt(fromGroup, fromSlot);
        if (itemForExchange == null) return false;
        
        if (to.AddItem(itemForExchange)) {
            RemoveItem(itemForExchange);
        } else {
            return false;
        }

        return true;
    }
    
    ///Переместить предмет в инвентаре (from) из координат (fromGroup, fromSlot) в инвентарь (to) в координаты (toGroup, toSlot)
    ///Если слот, куда надо переместиьи предмет занят, то по возможно сти происходит обмен местами
    ///Возращает True, если успешно, иначе False
    ///Инвентарь должен быть типа AdvancedInventory
    public bool ExchangeMoveItem(Inventory from, int fromGroup, int fromSlot, Inventory to, int toGroup, int toSlot) {
        if (to == null || from == null) return false;
        if (!(to is AdvancedInventory) || !(from is AdvancedInventory)) return false;
        if (!(to as AdvancedInventory).HasAccesToExchange(this) || !(from as AdvancedInventory).HasAccesToExchange(this)) return false;

        Item itemForExchange = from.GetItemAt(fromGroup, fromSlot);
        if (itemForExchange == null) return false;
        
        if (to.AddItemTo(toGroup, toSlot, itemForExchange)) {
            from.RemoveItemAt(fromGroup, fromSlot);
        } else {
            //Если слот, куда переместить, существует и занят – то делать свап
            if (GetGroupsCount() > toGroup && to.GetSlotCountOfGroup(toGroup) > toSlot) {
                Item itemInTo = to.GetItemAt(toGroup, toSlot);

                if (itemInTo != null) {
                    from.RemoveItem(itemForExchange);
                    to.RemoveItem(itemInTo);
                    if (from.AddItemTo(fromGroup, fromSlot, itemInTo) && to.AddItemTo(toGroup, toSlot, itemForExchange)) {
                        return true;
                    } else {
                        //Если один из предметов не удалось положить в слот-коллегу (напрмер: по причине, 
                        //что слот не может содержать этот предмет), то отменить свап
                        from.AddItemTo(fromGroup, fromSlot, itemForExchange);
                        to.AddItemTo(toGroup, toSlot, itemInTo);
                    }
                }
            }
            
            return false;
        }

        return true;
    }

    protected void CancelExchangeRequest(ExchangeRequest request) {
        GD.Print("[AdvancedInventory] Trying CancelExchangeRequest");
        int i = 0;
        foreach (ExchangeRequest req in _CurrentExchangeRequests) {
            if (req.From == request.From && req.ItemOfRequest == request.ItemOfRequest) {
                //Убрать из списка запросов, если указанного запроса нет в спике, отмена будет прервана
                _CurrentExchangeRequests.RemoveAt(i);

                //Обновление кол-ва активных запросов с инвентарём-отправителем
                if (!_ConnectedInventories.ContainsKey(request.From)) {
                    OS.Alert("Error with request count", "Error");
                    return;
                }
                _ConnectedInventories[request.From]--;
                //Отключение сигналов инвентаря-отправителя если не осталось активных запросом с ним
                if (_ConnectedInventories[request.From] == 0) {
                    request.From.Disconnect("tree_exited", this, nameof(_SenderInventoryRemoved));
                    request.From.Disconnect(nameof(Inventory.ItemRemoving), this, nameof(_SomeInventory_ItemRemoving));
                    _ConnectedInventories.Remove(request.From);
                }

                //Испустить сигнал о отмене
                EmitSignal(nameof(ExchangeRequestCanceled), request.ItemOfRequest, request.From, request.Type);

                //Очищение из списка ожидающих на проверку существования предмета если он там есть
                _ExchangeRequestToCompareItems.Remove(request);

                GD.Print("[AdvancedInventory] CancelExchangeRequest OK");
                break;
            }
            ++i;
        }
    }

    protected void CancelExchangeRequests(ExchangeRequest[] requests) {
        foreach (ExchangeRequest request in requests) {
            CancelExchangeRequest(request);
        }
    }

    public override void _PhysicsProcess(float delta) {
        base._PhysicsProcess(delta);

        //Отсчёт до удаления игнорируемых запросов
        List<ExchangeRequest> requestsToCancel = new List<ExchangeRequest>();
        for (int i = 0; i < _CurrentExchangeRequests.Count; ++i) {
            ExchangeRequest request = _CurrentExchangeRequests[i];
            request.CancelWait -= delta;
            _CurrentExchangeRequests[i] = request;
            if (_CurrentExchangeRequests[i].CancelWait <= 0) {
                requestsToCancel.Add(_CurrentExchangeRequests[i]);
                GD.Print("[AdvancedInventory] New time cancel (", request.From.InventoryName, ", " + request.ItemOfRequest.ItemName + ", " + request.Type.ToString() + ")");
            }
        }

        //Проверка на существование предметов и удаление запросов при пропаже предметов запроса
        foreach (ExchangeRequest request in _ExchangeRequestToCompareItems) {
            if (!request.From.HasItem(request.ItemOfRequest) && !requestsToCancel.Contains(request)) {
                requestsToCancel.Add(request);
            }
        }
        _ExchangeRequestToCompareItems.Clear();

        CancelExchangeRequests(requestsToCancel.ToArray());
    }

    protected void _SenderInventoryRemoved(Inventory inventory) {
        List<ExchangeRequest> requestsToCancel = new List<ExchangeRequest>();
        
        for (int i = 0; i < _CurrentExchangeRequests.Count; ++i) {
            ExchangeRequest request = _CurrentExchangeRequests[i];
            if (request.CancelWait <= 0)
                requestsToCancel.Add(request);
        }

        CancelExchangeRequests(requestsToCancel.ToArray());
    }

    protected void _SomeInventory_ItemRemoving(int _, int __, Item item) {
        GD.Print("[AdvancedInventory] Item Removed: ", item.ItemName);
        foreach (ExchangeRequest request in _CurrentExchangeRequests) {
            //Добавление запроса на проверку существования предмата
            //Если проверка в близжайщей обработке PhysicsProcess выявит отсутствие предмета, то запрос будет отменён
            if (request.ItemOfRequest == item && !_ExchangeRequestToCompareItems.Contains(request)) {
                _ExchangeRequestToCompareItems.Add(request);
            }
        }
    }




    ///Тип запроса передачи предмета – Дать (Give), Получить (Get)
    public enum ExchangeRequestType {
        Give,
        Get
    }

    ///Информация о запросе передачи предмета 
    public struct ExchangeRequest {
        public Item ItemOfRequest;
        public AdvancedInventory From;
        public ExchangeRequestType Type;
        public float CancelWait;

        public ExchangeRequest(Item itemOfRequest, AdvancedInventory from, ExchangeRequestType type, float cancelWait) {
            ItemOfRequest = itemOfRequest;
            From = from;
            Type = type;
            CancelWait = cancelWait;
        }
    }


    public AdvancedInventory() : base() {}
    public AdvancedInventory(string inventoryName, int[] sizes, string[] names) : base(inventoryName, sizes, names) {}
    public AdvancedInventory(string inventoryName, int[] sizes, string[] names, int mainSlotGroup, int mainSlot) : base(inventoryName, sizes, names, mainSlotGroup, mainSlot) {}
}

