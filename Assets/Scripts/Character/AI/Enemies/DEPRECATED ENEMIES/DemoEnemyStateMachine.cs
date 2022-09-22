using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using StateMachine;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Random = UnityEngine.Random;

public class DemoEnemyStateMachine : MonoBehaviour
{

    public AIController controller;
    private FiniteStateMachine<DemoEnemyFsmData> _demoFsm;

    public const string IdleStateName = "Idle";
    public const string ChaseStateName = "Chase";
    public const string StayStillAndFireStateName = "StayStillAndFire";

    /// <summary>
    /// This struct is the "type" of the state machine, meaning that each state will have access to this data at runtime.
    /// This is useful as functionality expands-- for now, the states will have access to the entity they are controlling and the player, later they could
    /// have access to team blackboards, etc.
    /// </summary>
    public struct DemoEnemyFsmData
    {
        public DemoEnemyStateMachine demoFsm { get; private set; }
        public  Player player { get; private set; }
        public AIController controller { get; private set; }

        public DemoEnemyFsmData(DemoEnemyStateMachine demoFsm, AIController controller)
        {
            this.demoFsm = demoFsm;
            this.controller = controller;
            this.player = GameManager.Instance.GetPlayer();
        }
    }
    
    //Here is where static methods can go, for example, if the enemy needs to do some special calculation for trajectory.
    // ****

    /// <summary>
    /// This is the base state information, meaning every state specified later in this class extends from this.
    /// It hold access to all the information that the DemoEnemyData stores, as well as Init() which is called by the StateMachine,
    /// and can hold methods that will be useful for the other states, e.g distance from the player, etc.
    /// </summary>
    abstract class DemoEnemyStateBase
    {
        //Will be overridden by the inheriting state. Each state has a name.
        public virtual string Name => throw new System.NotImplementedException();

        //Each state has access to these variables. They are set via the input struct, as shown below.
        protected IFiniteStateMachine<DemoEnemyFsmData> ParentFsm;
        protected DemoEnemyStateMachine DemoFsm;
        protected AIController Controller;
        protected Player Player;
        protected LayerMask LayerMask = LayerMask.GetMask("Walkable");
        
        //Init is called by the parent state machine, and sets up the states, their transitions, etc.
        public virtual void Init(IFiniteStateMachine<DemoEnemyFsmData> parentFsm,
            DemoEnemyFsmData demoFsmData)
        {
            ParentFsm = parentFsm;
            DemoFsm = demoFsmData.demoFsm;
            Controller = demoFsmData.controller;
            Player = demoFsmData.player;
        }
        
        //Methods that all states have access to go here.
        public bool IsWithinRangeOfPlayer(Vector3 loc, float range)
        {
            return (Player.GetCharacter().transform.position - loc).magnitude <= range;
        }
        
        public Vector3 CalculateRandomPosInPrefRange(float min, float max, Vector3 target)
        {
            var randX = Random.Range(-max, max);
            var randZ = Random.Range(-max, max);
            var pos = target;
            //This var contains the randomized x and z, but uses the player's current y position for the y variable.
            //This won't work, as we need to map this y to the ground to get a usable navigation point.
            var temp = new Vector3(pos.x + randX, pos.y, pos.z + randZ);
            //Cast a ray straight down from the point.
            Ray r = new Ray(temp, Vector3.down);
            return Physics.Raycast(r, out RaycastHit hitInfo, 1000f, LayerMask) ? hitInfo.point : temp;
        }

        public bool FindValidNavPos(Vector3 targetToSeek, out Vector3 navPos)
        {
            navPos = new Vector3(0, 0, 0);
            for (var i = 0; i < 3; i++)
            {
                Debug.Log("Calculating a position to seek.");
                navPos = CalculateRandomPosInPrefRange(Controller.enemyData.minEngagementRange, Controller.enemyData.maxEngagementRange, targetToSeek);
                if (Controller.SetPath(navPos))
                {
                    Debug.Log("Found a navPos");
                    return true;
                }
            }
            return false;
        }
        
        //Base functions for states
        protected void InternalEnter()
        {
            Debug.Log("Entered state " + Name);
        }
    
        public virtual void Exit(bool globalTransition) { }
        public virtual void Exit() { Exit(false); }
    
        public virtual DeferredStateTransitionBase<DemoEnemyFsmData> Update()
        {
            return null;
        }

    }
    
    abstract class DemoEnemyState : DemoEnemyStateBase, IState<DemoEnemyFsmData>
    {
        public virtual void Enter() { InternalEnter(); }
    }
    
    abstract class DemoEnemyState<S0> : DemoEnemyStateBase, IState<DemoEnemyFsmData, S0>
    {
        public virtual void Enter(S0 s) { InternalEnter(); }
    }
    
    abstract class DemoEnemyState<S0, S1> : DemoEnemyStateBase, IState<DemoEnemyFsmData, S0, S1>
    {
        public virtual void Enter(S0 s0, S1 s1) { InternalEnter(); }
    }

    //Can add more state types with different numbers of entry parameters with this pattern ^

    class IdleState : DemoEnemyState
    {
        public override string Name => IdleStateName;

        private DeferredStateTransition<DemoEnemyFsmData, SCharacterController> _goToChaseStateTransition;

        public override void Init(IFiniteStateMachine<DemoEnemyFsmData> parentFsm, DemoEnemyFsmData demoFsmData)
        {
            base.Init(parentFsm, demoFsmData);
            _goToChaseStateTransition = parentFsm.CreateStateTransition<SCharacterController>(ChaseStateName, null);
        }

        public override void Enter()
        {
            base.Enter();
            Controller.StopGoing();
        }

        public override void Exit(bool globalTransition)
        {
            
        }

        public override DeferredStateTransitionBase<DemoEnemyFsmData> Update()
        {
            //Announce return variable
            DeferredStateTransition<DemoEnemyFsmData, SCharacterController> ret = null;

            var inputs = Controller.InitInputs();
            Controller.AssignInputsToCharacter(inputs);
            
            //TODO: Give this the capability to transition to the retreat state as well, if the player is already too close for comfort.
            if (!IsWithinRangeOfPlayer(Controller.character.transform.position, Controller.enemyData.maxEngagementRange))
            {
                _goToChaseStateTransition.Arg0 = Player.GetCharacter();
                ret = _goToChaseStateTransition;
            }

            return ret;
        }
    }

    class ChaseState : DemoEnemyState<SCharacterController>
    {
        public override string Name => ChaseStateName;

        private SCharacterController _characterToChase;
        private DeferredStateTransition<DemoEnemyFsmData> _goToIdleStateTransition;
        private DeferredStateTransition<DemoEnemyFsmData> _goToStayStillAndFireStateTransition;
        private Vector3 _walkToLocation;

        public override void Init(IFiniteStateMachine<DemoEnemyFsmData> parentFsm, DemoEnemyFsmData demoFsmData)
        {
            base.Init(parentFsm, demoFsmData);
            _goToIdleStateTransition = parentFsm.CreateStateTransition(IdleStateName);
            _goToStayStillAndFireStateTransition = parentFsm.CreateStateTransition(StayStillAndFireStateName);
        }

        public override void Enter(SCharacterController s)
        {
            base.Enter(s);
            _characterToChase = s;
            Controller.SetLookAtPath();
        }

        public override void Exit(bool globalTransition)
        {
            
        }

        public override DeferredStateTransitionBase<DemoEnemyFsmData> Update()
        {
            DeferredStateTransitionBase<DemoEnemyFsmData> ret = null;

            //TODO: Account for min range
            if (!IsWithinRangeOfPlayer(_walkToLocation, Controller.enemyData.maxEngagementRange) || !Controller.HasPath())
            {
                if (FindValidNavPos(_characterToChase.transform.position, out var navPos))
                {
                    _walkToLocation = navPos;
                }
                else
                {
                    //TODO: This should lead to an error state, or otherwise be handled differently.
                    return _goToIdleStateTransition;
                }
            }

            var inputs = Controller.InitInputs();
            inputs = Controller.FollowPath(inputs);
            inputs = Controller.SetLookAtInputs(inputs);
            
            if (IsWithinRangeOfPlayer(Controller.character.transform.position, Controller.enemyData.maxEngagementRange))
            {

                Controller.SetLookAtCharacter(_characterToChase);
                inputs = Controller.SetTarget(inputs, _characterToChase.transform.position);
                inputs = Controller.Fire(inputs);
                
                if (!Controller.HasPath())
                {
                    ret = _goToStayStillAndFireStateTransition;
                }
            }
            else
            {
                Controller.SetLookAtPath();
            }

            Controller.AssignInputsToCharacter(inputs);
            return ret;

        }
    }

    class StayStillAndFireState : DemoEnemyState
    {
        public override string Name => StayStillAndFireStateName;

        private SCharacterController _targetCharacter;
        private DeferredStateTransition<DemoEnemyFsmData, SCharacterController> _goToChaseStateTransition;
        private DeferredStateTransition<DemoEnemyFsmData> _goToIdleStateTransition;
        private float _initialTimeStamp;
        private float _numSecondsToStayInState;

        public override void Init(IFiniteStateMachine<DemoEnemyFsmData> parentFsm, DemoEnemyFsmData demoFsmData)
        {
            base.Init(parentFsm, demoFsmData);
            _goToChaseStateTransition = parentFsm.CreateStateTransition<SCharacterController>(ChaseStateName, null);
            _goToIdleStateTransition = parentFsm.CreateStateTransition(IdleStateName);
        }

        public override void Enter()
        {
            base.Enter();
            Controller.StopGoing();
            _targetCharacter = Player.GetCharacter();
            Controller.SetLookAtCharacter(_targetCharacter);
            _numSecondsToStayInState = Random.Range(.5f, 5f);
            _initialTimeStamp = Time.time;
        }

        public override void Exit(bool globalTransition)
        {
            
        }

        public override DeferredStateTransitionBase<DemoEnemyFsmData> Update()
        {
            
            DeferredStateTransitionBase<DemoEnemyFsmData> ret = null;

            var inputs = Controller.InitInputs();
            inputs = Controller.SetLookAtInputs(inputs);
            inputs = Controller.SetTarget(inputs, _targetCharacter.transform.position);
            // THIS IS THE PROBLEM. AIMING A MILLION TIMES CRIPPLES YOUR MOVEMENT TO NOTHING.
            // inputs = Controller.Aim(inputs);
            inputs = Controller.Fire(inputs);

            if (Time.time > _initialTimeStamp + _numSecondsToStayInState)
            {
                _goToChaseStateTransition.Arg0 = Player.GetCharacter();
                ret = _goToChaseStateTransition;
                // ret = _goToIdleStateTransition;
            }
            
            Controller.AssignInputsToCharacter(inputs);
            
            return ret;

        }
        
    }

    private void Awake()
    {
        controller = GetComponent<AIController>();
    }

    private void Start()
    {

        //var demoEnemyFsmData = new DemoEnemyFsmData(this, controller);
        //_demoFsm = new FiniteStateMachine<DemoEnemyFsmData>(demoEnemyFsmData);
        
        //_demoFsm.AddState(new ChaseState());
        //_demoFsm.AddState(new StayStillAndFireState());
        //_demoFsm.AddState(new IdleState(), true);

    }

    private void Update()
    {
        //_demoFsm.Update();
    }
}
