using UnityEngine;
using System.Collections;

public abstract class AIState : MonoBehaviour
{
    //protected
    protected AIStateMachine _aIStateMachine;
    

    //abstract
    public abstract AIStateType GetStateType();
    public abstract AIStateType OnUpdate();

    //private 
    private AITargetType _curType;

    //public Porperty
    public AITargetType curType { get { return _curType; } set { _curType = value; } }

    //Default Handlers 
    public virtual void OnEnterState() { }
    public virtual void OnExitState() { }
    public virtual void OnAnimatorUpdated()
    {
        //得到根运动为这个更新所更新的米的数量，并除以deltaTime得到米每秒。  (速度=路程/时间)
        //然后我们把这个分配给nav代理的速度。
        if (_aIStateMachine.useRootPosition)
            _aIStateMachine.Agent.velocity = _aIStateMachine.Anim.deltaPosition / Time.deltaTime;

        // 从animator中获取根旋转并赋值为变换的旋转。
        if (_aIStateMachine.useRootRotation)
            _aIStateMachine.transform.rotation = _aIStateMachine.Anim.rootRotation;
    }
    public virtual void OnAnimatorIKUpdated() { }
    public virtual void OnTriggerEvent(AITriggerEventType aITriggerEventType, Collider other) { }
    public virtual void OnDestinationReached(bool isReached) { }

    //public
    public virtual void SetAIStateMachine(AIStateMachine aIStateMachine)
    {
        _aIStateMachine = aIStateMachine;
    }

    /// <summary>
    /// 转换球形碰撞器到世界坐标轴
    /// </summary>
    public static void ConvertSphereColliderToWorldSpace(SphereCollider sphere,out Vector3 soundPos,out float soundRadius)
    {
        float x = sphere.center.x * sphere.transform.lossyScale.x;
        float y = sphere.center.y * sphere.transform.lossyScale.y;
        float z = sphere.center.z * sphere.transform.lossyScale.z;

        soundPos = new Vector3(x,y,z);

        float temp1 = Mathf.Max(sphere.radius * sphere.transform.lossyScale.x, sphere.radius * sphere.transform.lossyScale.y);
        float temp2 = Mathf.Max(temp1, sphere.radius * sphere.transform.lossyScale.z);
        soundRadius = temp2;
    }

    /// <summary>
    /// 返回,带符号的角度.
    /// </summary>
    public static float FindSingleAngle(Vector3 from,Vector3 to)
    {
        float angle = Vector3.Angle(from.normalized,to.normalized);
        Vector3 temp = Vector3.Cross(from.normalized, to.normalized);
        angle = temp.y > 0 ? angle : -angle;
        return angle;
    }


}
