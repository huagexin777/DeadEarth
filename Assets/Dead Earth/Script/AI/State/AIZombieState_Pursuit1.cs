using UnityEngine;
using System.Collections;

/// <summary>
/// 方案1: 追逐敌人,--在数秒后放弃
/// </summary>
public class AIZombieState_Pursuit1 : AIZombieState
{
    //计时器
    [SerializeField] float _timer = 0;
    [SerializeField] float _repathTimer = 0.0f; //重新计算路径.

    [SerializeField] [Range(1,20)] float _maxDuration = 10;
    [SerializeField] [Range(0, 10)] private float _speed = 1.0f;
    [SerializeField] private float _repathDistanceMultiplier = 0.035f;
    [SerializeField] private float _repathVisualMinDuration = 0.05f;
    [SerializeField] private float _repathVisualMaxDuration = 5.0f;



    public override AIStateType GetStateType()
    {
        return AIStateType.Pursuit;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        //配置 state Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;


        _zombieStateMachine.Agent.Resume();
        //_zombieStateMachine.Agent.Stop();

        _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.currentTarget.position);
    
    }


    public override AIStateType OnUpdate()
    {
        _repathTimer += Time.deltaTime; //路径重新计算.
        _timer += Time.deltaTime;       //处于状态时间内.
        if (_timer >= _maxDuration)
        {
            _timer = 0;
            _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(false));
            return AIStateType.Alerted;
        }

        //追到-目的地:
        //1.处于,近战状态.
        if (_zombieStateMachine.currentTargetType == AITargetType.Visual_Player && 
            _zombieStateMachine.inMeleeRange)
        {
            return AIStateType.Attack;
        }
        //2.处于: 光照、声音. 
        if (_zombieStateMachine.isTargetReached)
        {
            switch (_zombieStateMachine.currentTargetType)
            {
                case AITargetType.Audio:
                case AITargetType.Visual_Light:
                    _zombieStateMachine.ClearTarget();    
                    Debug.Log("光照---开始警觉!");
                    return AIStateType.Alerted;

                case AITargetType.Visual_Food:
                    Debug.Log("吃尸体! ");
                    return AIStateType.Feeding;
            }
        }

        //当我们,导航路径丢失.State重新切换为警觉
        if (_zombieStateMachine.Agent.isPathStale ||
            _zombieStateMachine.Agent.pathStatus == NavMeshPathStatus.PathInvalid ||
           (_zombieStateMachine.Agent.hasPath == false && _zombieStateMachine.Agent.pathPending == false))
        {
            return AIStateType.Alerted;
        }

        if (_zombieStateMachine.Agent.pathPending)
            _zombieStateMachine.speed = 0;
        else
            _zombieStateMachine.speed = _speed;


        #region   朝向问题:

        // 1. 追逐（目标-玩家）过程中:
        if (_zombieStateMachine.useRootRotation == false
            && _zombieStateMachine.currentTargetType == AITargetType.Visual_Player
            && _zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player
            && _zombieStateMachine.isTargetReached)
        {
            //需要,一直朝向player.
            Vector3 targetPos = _zombieStateMachine.targetPosition;
            targetPos.y = transform.position.y;
            Quaternion targetRoa = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRoa, Time.deltaTime);
        }

        //2. 追逐（目标-光照、音频）过程中:
        else if (_zombieStateMachine.isTargetReached)
        {
            return AIStateType.Alerted;
        }

        // 追逐中...  需要持续更新朝向 (Agent.desiredVelocity)
        else if (_zombieStateMachine.useRootRotation == false &&
                 _zombieStateMachine.isTargetReached == false)
        {
            //需要,一直朝向player.
            Quaternion targetRoa = Quaternion.LookRotation(_zombieStateMachine.Agent.desiredVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRoa, Time.deltaTime);
        }

     

        #endregion

        //!!! 需要持续更新...目标位置.

        //视觉威胁,
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            if (_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position)
            {
                //实现效果: 
                //距离越近,(位置)更新越频繁.
                if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier,_repathVisualMinDuration,_repathVisualMaxDuration) < _repathTimer)
                {
                    _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.targetPosition);
                    _repathTimer = 0;
                }
            }
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);

            return AIStateType.Pursuit;
        }
        //光照威胁.
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            //音频、食物 优先于!
            if (_zombieStateMachine.currentTargetType == AITargetType.Audio || 
                _zombieStateMachine.currentTargetType == AITargetType.Visual_Food)
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                return AIStateType.Alerted;
            }
            else if (_zombieStateMachine.currentTargetType == AITargetType.Visual_Light)
            {
                int currentID = _zombieStateMachine.currentID;
                if (currentID == _zombieStateMachine.VisualThreat.collider.GetInstanceID())
                {
                    if (_zombieStateMachine.targetPosition != _zombieStateMachine.VisualThreat.position)
                    {
                        if (Mathf.Clamp(_zombieStateMachine.VisualThreat.distance * _repathDistanceMultiplier,_repathVisualMinDuration,_repathVisualMaxDuration) < _repathTimer)
                        {
                            _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.targetPosition);
                            _repathTimer = 0;
                        }
                    }
                    _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                    return AIStateType.Pursuit;
                }
            }
        }
        
        return AIStateType.Pursuit;
    }
}
