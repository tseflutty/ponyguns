using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

//2019 © Даниил Белов
//Создано 17.07.2019

///Более продвинутый ИИ для сущности. Может быть как враждебным так и нет.
///Умеет следовать за игроком, придерживаясь минимальной дистанции
public class ImprovedMobAI : BaseMobAI
{   
    [Export]
    ///Фильтр по неотображаемому названию типа для поиска цели
    public string[] TypeNamesTarget = new string[0];

    //TODO: Фикс, вылета при удалении цели
    ///Цель, за которой следует сущность в данный момент
    protected Entity CurrentTarget;

    protected List<Entity> TargetsToCompare = new List<Entity>();

    [Export]
    ///Может ли этот AI искать цель и следовать за ней
    public bool CanFollowing = true;

    protected MobAIState _state = MobAIState.Patrol;

    static bool AllowUsingNavigation = true;

    protected MobAIState state {
        get { return _state; }
        set {
            _state = value;
            if (_state == MobAIState.LookAround) {
                LookAroundAngle = (float) Math.PI * -1; //Vector2(cos(angle), sin(angle));
                DebugConsole.Shared.Output("[AI] LOOK AROUND");
            }
            if (_state == MobAIState.Following) {
                DebugConsole.Shared.Output("[AI] FOLLOWING");
                FindWait = 0;
            }
            if (_state == MobAIState.Patrol) {
                DebugConsole.Shared.Output("[AI] PATROL");
            }
                
        }
    }

    protected float LookAroundAngle = (float) Math.PI * -1;

    [Export]
    ///Минимальная дистанция, которую будет держать моб от цели
    public float MinDistance = 1;

    ///Текущаяя стадия моба (Патруль, следование...).
    ///Изменение допускается только из самого ИИ используя state
    public MobAIState State {
        get { return _state; }
    }

    //Время ожидание нахождения цели рейкастингом
    protected float FindWait = 0;

    public void PatrolProcess() {
        if (CurrentTarget != null)
            CurrentTarget = null;
        
        foreach (Spatial obj in SigthAreaObjects) {
            if (obj is Entity && obj != Slave) {
                Entity ent = obj as Entity;
                foreach (string typeName in TypeNamesTarget) {
                    //Если название типа удавлетворяет фильтру, то он отправляется в список для проверки
                    if (ent.TypeName == typeName && !TargetsToCompare.Contains(ent)) {
                        TargetsToCompare.Add(ent);
                    }
                }
            }
        }
        if (TargetsToCompare.Count > 0) {
            CompareTargets(TargetsToCompare);
            TargetsToCompare.Clear();
        }
    }

    protected void FollowingProcess(float delta) {
        //Если рейкастинг не видит цель меньше 2 секунд то мледовать в её сторону, иначе двигаться в последнем направлении
        if (FindWait < 2) {
            if (AllowUsingNavigation) {
                //Позиция управляемой сущность
                Vector3 SlavePos = Slave.GetGlobalTransform().origin;
                //Позиция цели
                Vector3 TargetPos = CurrentTarget.GetGlobalTransform().origin;

                //Порверка арены
                if (Arena.GetCurrent() == null) {
                    OS.Alert("Арена не найдена", "Ошибка");
                    return;
                }

                //Путь к цели по навигации
                Vector3[] Path = Arena.GetCurrent().GetNavigation().GetSimplePath(SlavePos, TargetPos, false);
                //Первая точка, к которой будет следовать сущность
                Vector3 FirstPos = (Path.Length > 1) ? Path[1] : TargetPos;
                //Направление к первой точке
                Vector2 dir = new Vector2(FirstPos.x - SlavePos.x, FirstPos.z - SlavePos.z);

                //Расстояние до цели
                float distance = (float)Math.Sqrt(Math.Pow((SlavePos.x - TargetPos.x), 2) + Math.Pow((SlavePos.z - TargetPos.z), 2));

                //Запуск движения к первой точке и перевод взгляда в сторону цели
                //Ограничение передвижения по минимальной дистанции
                if (distance > MinDistance)
                    Slave.StartMovement(dir);
                else
                    Slave.StopMovement();
                Slave.SetSightDirectionTo(TargetPos);
            } else {
                //Позиция управляемой сущность
                Vector3 SlavePos = Slave.GetGlobalTransform().origin;
                //Позиция цели
                Vector3 TargetPos = CurrentTarget.GetGlobalTransform().origin;

                //Направление к цели
                Vector2 dir = new Vector2(TargetPos.x - SlavePos.x, TargetPos.z - SlavePos.z);

                //Расстояние до цели
                float distance = (float)Math.Sqrt(Math.Pow((SlavePos.x - TargetPos.x), 2) + Math.Pow((SlavePos.z - TargetPos.z), 2));

                //Запуск движения к первой точке и перевод взгляда в сторону цели
                //Ограничение передвижения по минимальной дистанции
                if (distance > MinDistance)
                    Slave.StartMovement(dir);
                else
                    Slave.StopMovement();
                Slave.SetSightDirectionTo(TargetPos);
            }
        } else if (FindWait > 5) {
            //После 8 секунд ожидания, оглядывание вокруг
            Slave.StopMovement();
            state = MobAIState.LookAround;
            DebugConsole.Shared.Output("[AI] Look around");
        }

        //Проверка видимости цели и направление в его сторону
        Godot.Object sigthObj = SigthRayCast.GetCollider();
        if (sigthObj != CurrentTarget)
            FindWait += delta;
        else
            FindWait = 0;
        
    }

    protected void LookAroundProcess(float delta) {
        if (Slave == null) return;

        Slave.SetSightDirectionTo(Slave.GetGlobalTransform().origin + new Vector3((float)Math.Cos(LookAroundAngle), 0, (float)Math.Sin(LookAroundAngle)));

        foreach (Spatial obj in SigthAreaObjects) {
            if (obj is Entity && obj != Slave) {
                Entity ent = obj as Entity;
                foreach (string typeName in TypeNamesTarget) {
                    //Если название типа удавлетворяет фильтру, то он отправляется в список для проверки
                    if (ent.TypeName == typeName && !TargetsToCompare.Contains(ent)) {
                        TargetsToCompare.Add(ent);
                    }
                }
            }
        }
        if (TargetsToCompare.Count > 0) {
            CompareTargets(TargetsToCompare);
            TargetsToCompare.Clear();
        }

        LookAroundAngle += 5 * delta;
        if (LookAroundAngle > Math.PI) state = MobAIState.Patrol;
    }

    public override void _PhysicsProcess(float delta) {
        base._PhysicsProcess(delta);
        
        AllowRandomMovement = (State == MobAIState.Patrol);

        switch (State) {
            case MobAIState.Patrol:
                PatrolProcess();
                break;
            case MobAIState.Following:
                FollowingProcess(delta);
                break;
            case MobAIState.LookAround:
                LookAroundProcess(delta);
                break;
        }
    }

    ///Проверяет действительно ли сущность видит цели. Указывают первую видимую цель как текущую.
    ///В случае если найдена хоть одна видимая цель, переводит в режим следования
    protected void CompareTargets(List<Entity> targets) {
        if (Slave == null) return;

        PhysicsDirectSpaceState spaceState = Slave.GetWorld().DirectSpaceState;

        //Проверка рейкастом
        foreach (Entity target in targets) {
            Godot.Collections.Array arrExclude = new Godot.Collections.Array();
            arrExclude.Add(Slave);
            Dictionary resultObject = spaceState.IntersectRay(Slave.GetGlobalTransform().origin, target.GetGlobalTransform().origin, arrExclude);
            if (resultObject == null) continue;
            if (!resultObject.ContainsKey("collider")) continue;
            object obj = resultObject["collider"];
            //Проверка является пересикаемый рейкастом объект предпологаемой целью
            if (obj == null || !(obj is Entity)) continue;
            if (obj is Entity) {
                Entity comparingEntity = obj as Entity;
                if (obj == target) {
                    DebugConsole.Shared.Output("[AI] TARGET");
                    CurrentTarget = target;
                    state = MobAIState.Following;
                }
            }
        }
    }

}

public enum MobAIState {
    Patrol,
    Following,
    LookAround
}
