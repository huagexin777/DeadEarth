using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSceneManager : MonoBehaviour
{
    private static GameSceneManager _instance;
    public static GameSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));
            }
            return _instance;
        }
    }

    //private
    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();

    #region 属性
    
    public ParticleSystem BloodParticle
    {
        get
        {
            GameObject bloodGo = Resources.Load<GameObject>("Efx/BloodExf");
            bloodGo = Instantiate(bloodGo);
            ParticleSystem ps = bloodGo.GetComponentInChildren<ParticleSystem>();
            return ps;
        }
    }

    #endregion

    void Start()
    {

    }

    void Update()
    {

    }

    //public 
    //通过key,注册AIStateMachine
    public void RegisterAiStateMachine(int key, AIStateMachine aiMachine)
    {
        if (!_stateMachines.ContainsKey(key))
        {
            _stateMachines.Add(key,aiMachine);
        }
    }

    //通过key,来得到AIStateMachine
    public AIStateMachine GetAiStateMachine(int key)
    {
        AIStateMachine _aIStateMachine = null;
        if (_stateMachines.ContainsKey(key))
        {
            _stateMachines.TryGetValue(key,out _aIStateMachine);
        }
        return _aIStateMachine;
    }
}
