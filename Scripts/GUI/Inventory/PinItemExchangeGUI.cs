using Godot;
using System;

//2019 © Даниил Белов
//Создано 03.09.2019

public class PinItemExchangeGUI : PointerMenuCouple
{
    protected ExchangeInventoryGUI InteractorGUI;
    protected ExchangeInventoryGUI PartnerGUI;

    public override void _Ready() {
        base._Ready();
        InteractorGUI = GetNode("PointerMenuContainer1/InventoryGUI") as ExchangeInventoryGUI;
        PartnerGUI = GetNode("PointerMenuContainer2/InventoryGUI") as ExchangeInventoryGUI;

        InteractorGUI.Init(new ExchangeInventoryGUI[1] {PartnerGUI});
        PartnerGUI.Init(new ExchangeInventoryGUI[1] {InteractorGUI});
        
        Godot.Collections.Array interactorSignalBinds = new Godot.Collections.Array();
        interactorSignalBinds.Add(0);
        InteractorGUI.Connect(nameof(ExchangeInventoryGUI.OrderedToFront), this, nameof(_InventoryGUI_OrderedToFroin), interactorSignalBinds);

        Godot.Collections.Array partnerSignalBinds = new Godot.Collections.Array();
        partnerSignalBinds.Add(1);
        PartnerGUI.Connect(nameof(ExchangeInventoryGUI.OrderedToFront), this, nameof(_InventoryGUI_OrderedToFroin), partnerSignalBinds);

        _UpdateInteractor();
        _UpdatePartner();
    }

    protected AdvancedInventory _interactor;
    ///Инвентарь, который взаимодействует
    public AdvancedInventory Interactor {
        get { return _interactor; }
        set { 
            _interactor = value;
            _UpdateInteractor();
        }
    }

    protected AdvancedInventory _partner;
    ///Инветнтарь, с которым взаимодействуют
    public AdvancedInventory Partner {
        get { return _partner; }
        set {
            _partner = value;
            _UpdatePartner();
        }
    }

    protected void _UpdateInteractor() {
        if (InteractorGUI != null && Interactor != null && PartnerGUI != null) {
            InteractorGUI.Slave = Interactor;
            PartnerGUI.Slave = Interactor;
        }
    }

    protected void _UpdatePartner() {
        if (Partner != null && PartnerGUI != null) {
            PartnerGUI.SlavePartner = Partner;
        }
    }

    protected void _InventoryGUI_OrderedToFroin(int num) => MoveChild((num == 0) ? FirstMenu : SecondMenu, GetChildCount()-1);
}
