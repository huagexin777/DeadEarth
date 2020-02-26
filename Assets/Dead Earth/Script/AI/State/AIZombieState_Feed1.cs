using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AIZombieState_Feed1 : AIZombieState
{
    [SerializeField] float _timer;
    [SerializeField] [Range(1,10)] float _maxTime = 10;
    [SerializeField] float _bloodTimer;
    [SerializeField] [Range(0.1f,2f)] float _maxBloodTime;
    [SerializeField] AnimationCurve animationCurve;
    [SerializeField] [Range(1,3)] float eatSpeed;
    [Header("-------------血液==粒子系统-------------")]
    [SerializeField] Transform _bloodParticlesMount = null;
    [SerializeField] [Range(0.01f, 1.0f)] float _bloodParticlesBurstTime = 0.1f;
    [SerializeField] [Range(1, 100)] int _bloodParticlesBurstAmount = 10;

    [SerializeField] int eatingType;                //进食动画类型.


    public override AIStateType GetStateType()
    {
        return AIStateType.Feeding;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();

        //配置 state Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = 0;
        _zombieStateMachine.feeding = true;
        _zombieStateMachine.feedingType = Random.Range(1, 4); //值: 1、2、3
        _zombieStateMachine.attackType = 0;
        _zombieStateMachine.seeking = 0;


        _zombieStateMachine.Agent.Resume();

        _timer = _maxTime;
        _bloodTimer = 0;
    }

    public override AIStateType OnUpdate()
    {
       
        //食用 最多时常.
        _timer -= Time.deltaTime;
        _bloodTimer += Time.deltaTime;
        if (_timer <= 0)
        {
            _zombieStateMachine.satisfaction = 1;
            _timer = _maxTime;
        }
        else
        {
            //处理,吃饱的时候.
            _zombieStateMachine.satisfaction += animationCurve.Evaluate(_timer / _maxTime) * Time.deltaTime;
            if (_zombieStateMachine.satisfaction >= 0.9f)
            {
                //处理,进食的时候.
                _zombieStateMachine.feeding = false;

                //方案一: 警觉一段时间后,再寻路
                return AIStateType.Alerted;
                //方案二: 直接再寻路
                _zombieStateMachine.Agent.SetDestination(_zombieStateMachine.GetWayPointPosition(false));
                return AIStateType.Alerted;
            }
            else
            {
                if (_bloodTimer >= _maxBloodTime)
                {
                    _bloodTimer = 0;
                    ParticleSystem ps = GameSceneManager.Instance.BloodParticle;
                    ps.transform.position = _bloodParticlesMount.position;
                    ps.transform.rotation = _bloodParticlesMount.rotation;
                    ps.Emit(_bloodParticlesBurstAmount);
                }
            }
        }

        //更新 旋转
        if (!_zombieStateMachine.useRootRotation)
        {
            Vector3 targetDir = _zombieStateMachine.targetPosition - transform.position;
            Quaternion targetRoa = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation,targetRoa,Time.deltaTime * 3);
        }

        return AIStateType.Feeding;
    }
    
}
