using UnityEngine;
using System.Collections;

public class AIZombieState_Patrol1 : AIZombieState
{
    // Inpsector Assigned 
    [SerializeField] float _turnOnSpotThreshold = 80.0f;
    [SerializeField] float _slerpSpeed = 5.0f;

    [SerializeField] [Range(0.0f, 3.0f)] float _speed = 1.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        if (_zombieStateMachine == null)
            return;
        //配置nav
        _zombieStateMachine.NavAgentControl(true,false);
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;

        _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(false));
        //重新-导航-路径
        _zombieStateMachine.Agent.Resume();
    }

    private void OnDrawGizmos()
    {
        if (_zombieStateMachine == null) return;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position,_zombieStateMachine.Agent.steeringTarget);

        Gizmos.DrawSphere(_zombieStateMachine.Agent.steeringTarget,0.2f);
    }

    public override AIStateType OnUpdate()
    {
        // Trigger-触发.
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }
        if (_zombieStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }
        if (_zombieStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            // If the distance to hunger ratio means we are hungry enough to stray off the path that far
            if ((1.0f - _zombieStateMachine.satisfaction) > (_zombieStateMachine.VisualThreat.distance / _zombieStateMachine.sensorRadius))
            {
                _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }
        }

        // 如果,路径还没计算完.
        if (_zombieStateMachine.Agent.pathPending)
        {
            _zombieStateMachine.speed = 0;
            return AIStateType.Patrol;
        }
        else
            _zombieStateMachine.speed = _speed;

        // 计算,两个向量夹角度数.
        float angle = Vector3.Angle(transform.forward, (_zombieStateMachine.Agent.steeringTarget - transform.position));

        // 如果,角度>转向阈值. 切换为警觉State.
        if (angle > _turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        // 如果,没有应用-[由Animator控制]-根旋转[rootRotation].
        if (!_zombieStateMachine.useRootRotation)
        {
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.Agent.desiredVelocity);
            
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
        }

        // 出现,导航路径丢失的情况.重新配置下一个节点.
        if (_zombieStateMachine.Agent.isPathStale ||
            !_zombieStateMachine.Agent.hasPath ||
            _zombieStateMachine.Agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(true));
        }

        return AIStateType.Patrol;
    }

    
    public override void OnDestinationReached(bool isReached)
    {
        if (_zombieStateMachine == null || isReached == false)
            return;

        //当zombie到达 目标点。
        if (_zombieStateMachine.currentTargetType == AITargetType.Waypoint)
            Debug.LogError("到达目的地!!!");
            _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(true));
    }
}
