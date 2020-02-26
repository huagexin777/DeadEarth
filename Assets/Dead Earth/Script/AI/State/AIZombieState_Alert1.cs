using UnityEngine;
using System.Collections;

public class AIZombieState_Alert1 : AIZombieState
{
    [SerializeField] [Range(0, 360)] float _wayPointThreshold = 80;
    [SerializeField] [Range(0, 360)] float _viewThreshold = 10;
    [SerializeField] [Range(5, 60)]  float _maxDuration = 10.0f;
    [SerializeField] [Range(0f, 3f)]  float _viewMaxDuration = 10.0f;
    [SerializeField] float _timer = 0;              //状态(转变)时间.
    [SerializeField] float _viewChanageTimer = 0;   //视野(转变)时间.

    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_aIStateMachine == null)
        {
            return;
        }

        //配置 state Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        

        _zombieStateMachine.Agent.Resume();
        //_zombieStateMachine.Agent.Stop();

        _timer = _maxDuration;
        _viewChanageTimer = 0;
    }

    public override AIStateType OnUpdate()
    {
        _viewChanageTimer += Time.deltaTime;
        //状态内总时间-消耗完
        _timer -= Time.deltaTime;
        if (_timer < 0)
        {
            _timer = _maxDuration;
            _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(true));
            _zombieStateMachine.Agent.Resume();
        }

        // Trigger-触发.
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            _timer = _maxDuration;
        }
        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            _timer = _maxDuration;
        }
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food 
            && _zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            // If the distance to hunger ratio means we are hungry enough to stray off the path that far
            if ((1.0f - _zombieStateMachine.satisfaction) > (_zombieStateMachine.VisualThreat.distance / _zombieStateMachine.sensorRadius))
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }
        }

        // 威胁情况:
        // 1.targetType由威胁过渡而来.
        if ((_zombieStateMachine.currentTargetType == AITargetType.Audio || _zombieStateMachine.currentTargetType == AITargetType.Visual_Light) && _zombieStateMachine.isTargetReached)
        {
            //处于状态过渡中... 视野角度 < _viewThreshold.
            float angle = AIState.FindSingleAngle(transform.forward, _zombieStateMachine.Agent.steeringTarget - transform.position);

            if (_zombieStateMachine.currentTargetType == AITargetType.Audio && Mathf.Abs(angle) < _viewThreshold)
            {
                return AIStateType.Pursuit;
            }
            if (_viewChanageTimer >= _viewMaxDuration)
            {
                if (Random.value <= _zombieStateMachine.intelligence)
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1, 1));
                }
                _viewChanageTimer = 0;
            }
        }
        // 2.targetType是wayPoint类型.
        else if (_zombieStateMachine.currentTargetType == AITargetType.Waypoint && !_zombieStateMachine.Agent.pathPending)
        {
            //处于 当视野角度 < _wayPointThreshold
            float angle = AIState.FindSingleAngle(transform.forward, _zombieStateMachine.Agent.steeringTarget - transform.position);

            if (Mathf.Abs(angle) < _wayPointThreshold)
            {
                Debug.LogError("Patrol");
                //_zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(true));
                return AIStateType.Patrol;
            }

            _zombieStateMachine.seeking = 0;
            if (_viewChanageTimer >= _viewMaxDuration)
            {
                if (Random.value <= _zombieStateMachine.intelligence)
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1, 1));
                }
                _viewChanageTimer = 0;
            }
        }
        // 3.目标都消失. zombie警觉
        else
        {
            Debug.LogError("Patrol++2");
            if (_viewChanageTimer >= _viewMaxDuration)
            {
                _zombieStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1, 1));
                _viewChanageTimer = 0;
            }
        }

       

        

      


        return AIStateType.Alerted;
    }
}
