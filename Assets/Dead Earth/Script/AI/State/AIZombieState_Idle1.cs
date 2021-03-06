﻿using UnityEngine;
using System.Collections;

public class AIZombieState_Idle1 : AIZombieState
{
    [SerializeField] private float _idleTimer = 0;
    [SerializeField] private float _idleTime  = 10;
    [SerializeField] private Vector2 _idleTimeRange = new Vector2(1f,5f);


    public override AIStateType GetStateType()
    {
        return AIStateType.Idle;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_zombieStateMachine == null)
        {
            return;
        }
        //随机,idleTime持续时间.
        _idleTime = Random.Range(_idleTimeRange.x, _idleTimeRange.y);

        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        _zombieStateMachine.ClearTarget();
    }

    
   

    public override AIStateType OnUpdate()
    {
        _idleTimer += Time.deltaTime;
        if (_idleTimer >= _idleTime)
        {
            _idleTimer = 0;
            _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(false));
            return AIStateType.Patrol;
        }

        // Trigger-触发.
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }
        else if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }
        else if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }
        else if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        return AIStateType.Idle;
    }

}
