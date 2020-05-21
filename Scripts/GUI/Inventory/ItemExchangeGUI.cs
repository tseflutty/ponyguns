using Godot;
using System;

//2019 © Даниил Белов
//Создано 26.08.2019

public class ItemExchangeGUI : Control
{   
    protected ExchangeInventoryGUI InteractorGUI;
    protected ExchangeInventoryGUI PartnerGUI;

    public override void _Ready() {
        InteractorGUI = GetNode("InteractorGUI") as ExchangeInventoryGUI;
        PartnerGUI = GetNode("PartnerGUI") as ExchangeInventoryGUI;
        InteractorGUI.Init(new ExchangeInventoryGUI[1] {PartnerGUI});
        PartnerGUI.Init(new ExchangeInventoryGUI[1] {InteractorGUI});
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

}
