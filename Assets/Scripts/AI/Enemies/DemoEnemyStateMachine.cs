using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine;
using TMPro;
using Random = UnityEngine.Random;

public class DemoEnemyStateMachine : MonoBehaviour
{

    public StarfallAIController controller;
    private FiniteStateMachine<DemoEnemyFsmData> _demoFsm;

    public const string IdleStateName = "Idle";
    public const string ChaseStateName = "Chase";
    public const string MoveAndFireStateName = "MoveAndFire";

    /// <summary>
    /// This struct is the "type" of the state machine, meaning that each state will have access to this data at runtime.
    /// This is useful as functionality expands-- for now, the states will have access to the entity they are controlling and the player, later they could
    /// have access to team blackboards, etc.
    /// </summary>
    public struct DemoEnemyFsmData
    {
        public DemoEnemyStateMachine demoFsm { get; private set; }
        public  Player player { get; private set; }
        public StarfallAIController controller { get; private set; }

        public DemoEnemyFsmData(DemoEnemyStateMachine demoFsm, StarfallAIController controller)
        {
            this.demoFsm = demoFsm;
            this.controller = controller;
            this.player = Player.Instance;
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
        protected StarfallAIController Controller;
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
            return (Player.character.transform.position - loc).magnitude <= range;
        }

        
        public Vector3 CalculateRandomPosInPrefRange(float min, float max, Vector3 target)
        {
            var randX = Random.Range(min, max);
            var randZ = Random.Range(min, max);
            var pos = target;
            //This var contains the randomized x and z, but uses the player's current y position for the y variable.
            //This won't work, as we need to map this y to the ground to get a usable navigation point.
            var temp = new Vector3(pos.x + randX, pos.y, pos.z + randZ);
            //Cast a ray straight down from the point.
            Ray r = new Ray(temp, Vector3.down);
            return Physics.Raycast(r, out RaycastHit hitInfo, 1000f, LayerMask) ? hitInfo.point : temp;
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

        private DeferredStateTransition<DemoEnemyFsmData, StarfallCharacterController> _goToChaseStateTransition;

        public override void Init(IFiniteStateMachine<DemoEnemyFsmData> parentFsm, DemoEnemyFsmData demoFsmData)
        {
            base.Init(parentFsm, demoFsmData);
            _goToChaseStateTransition = parentFsm.CreateStateTransition<StarfallCharacterController>(ChaseStateName, null);
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
            DeferredStateTransition<DemoEnemyFsmData, StarfallCharacterController> ret = null;

            var inputs = Controller.InitInputs();
            Controller.AssignInputsToCharacter(inputs);
            
            //TODO: Give this the capability to transition to the retreat state as well, if the player is already too close for comfort.
            if (!IsWithinRangeOfPlayer(Controller.character.transform.position, Controller.enemyData.maxEngagementRange))
            {
                _goToChaseStateTransition.Arg0 = Player.character;
                ret = _goToChaseStateTransition;
            }

            return ret;
        }
    }

    class ChaseState : DemoEnemyState<StarfallCharacterController>
    {
        public override string Name => ChaseStateName;

        private StarfallCharacterController _chaseTarget;
        private DeferredStateTransition<DemoEnemyFsmData> _goToIdleStateTransition;
        private DeferredStateTransition<DemoEnemyFsmData, bool, StarfallCharacterController> _goToMoveAndFireStateTransition;
        private Vector3 _targetLoc;

        public override void Init(IFiniteStateMachine<DemoEnemyFsmData> parentFsm, DemoEnemyFsmData demoFsmData)
        {
            base.Init(parentFsm, demoFsmData);
            _goToIdleStateTransition = parentFsm.CreateStateTransition(IdleStateName);
            _goToMoveAndFireStateTransition = parentFsm.CreateStateTransition<bool, StarfallCharacterController>(MoveAndFireStateName, false, null);
        }

        public override void Enter(StarfallCharacterController s)
        {
            base.Enter(s);
            _chaseTarget = s;
            Controller.SetLookAtPath();
            //Try three times to get a valid location to walk to.
            for (var i = 0; i < 3; i++)
            {
                Debug.Log("Calculating a position to seek.");
                _targetLoc = CalculateRandomPosInPrefRange(Controller.enemyData.minEngagementRange, Controller.enemyData.maxEngagementRange, _chaseTarget.transform.position);
                if (Controller.SetPath(_targetLoc))
                {
                    break;
                }
            }

            if (!Controller.SetPath(_targetLoc))
            {
                _targetLoc = s.transform.position;
            }
        }

        public override void Exit(bool globalTransition)
        {
            
        }

        public override DeferredStateTransitionBase<DemoEnemyFsmData> Update()
        {
            DeferredStateTransitionBase<DemoEnemyFsmData> ret = null;

            //TODO: Account for min range
            if (!IsWithinRangeOfPlayer(_targetLoc, Controller.enemyData.maxEngagementRange) || !Controller.HasPath())
            {
                //Try three times to get a valid location to walk to.
                for (var i = 0; i < 3; i++)
                {
                    Debug.Log("Calculating a position to seek.");
                    _targetLoc = CalculateRandomPosInPrefRange(Controller.enemyData.minEngagementRange, Controller.enemyData.maxEngagementRange, _chaseTarget.transform.position);
                    if (Controller.SetPath(_targetLoc))
                    {
                        break;
                    }
                }
            }

            var inputs = Controller.InitInputs();
            inputs = Controller.FollowPath(inputs);
            Controller.AssignInputsToCharacter(inputs);
            Debug.Log(inputs.MoveVector);

            if (IsWithinRangeOfPlayer(Controller.character.transform.position, Controller.enemyData.maxEngagementRange))
            {
                //Randomly choose between aiming and not aiming.
                var aim = Random.Range(0, 2) == 1;
                _goToMoveAndFireStateTransition.Arg0 = aim;
                _goToMoveAndFireStateTransition.Arg1 = Player.character;
                ret = _goToMoveAndFireStateTransition;
            }

            return ret;

        }
    }

    //The bool determines if the enemy should aim as it moves.
    class MoveAndFireState : DemoEnemyState<bool, StarfallCharacterController>
    {
        public override string Name => MoveAndFireStateName;

        private DeferredStateTransition<DemoEnemyFsmData, StarfallCharacterController> _goToChaseStateTransition;
        private DeferredStateTransition<DemoEnemyFsmData> _goToIdleStateTransition;

        private StarfallCharacterController _targetChar;
        private bool _isAiming;
        private Vector3 _targetLoc;

        public override void Init(IFiniteStateMachine<DemoEnemyFsmData> parentFsm, DemoEnemyFsmData demoFsmData)
        {
            base.Init(parentFsm, demoFsmData);
            _goToChaseStateTransition = parentFsm.CreateStateTransition<StarfallCharacterController>(ChaseStateName, null);
            _goToIdleStateTransition = parentFsm.CreateStateTransition(IdleStateName);
        }

        public override void Enter(bool s0, StarfallCharacterController s1)
        {
            base.Enter(s0, s1);
            _isAiming = s0;
            _targetChar = s1;
            Controller.SetLookAtCharacter(Player.character);
        }

        public override DeferredStateTransitionBase<DemoEnemyFsmData> Update()
        {

            DeferredStateTransitionBase<DemoEnemyFsmData> ret = null;

            //If the character has nowhere to go, assign it a place to go.
            if (!Controller.HasPath())
            {
                for (var i = 0; i < 3; i++)
                {
                    _targetLoc = CalculateRandomPosInPrefRange(Controller.enemyData.minEngagementRange, Controller.enemyData.maxEngagementRange, _targetChar.transform.position);
                    if (Controller.SetPath(_targetLoc))
                    {
                        break;
                    }
                }
            }

            var inputs = Controller.InitInputs();
            inputs = Controller.FollowPath(inputs);
            inputs = Controller.SetLookAtInputs(inputs);
            if (_isAiming)
            {
                inputs = Controller.Aim(inputs);
            }
            var aimPoint = _targetChar.transform.position +
                           (Random.insideUnitSphere * (1 - Controller.enemyData.accuracy));
            inputs.Target = aimPoint;
            inputs.Primary = true;
            Controller.AssignInputsToCharacter(inputs);

            if (!IsWithinRangeOfPlayer(Controller.character.transform.position,
                    Controller.enemyData.maxEngagementRange))
            {
                _goToChaseStateTransition.Arg0 = _targetChar;
                ret = _goToChaseStateTransition;
            }

            return ret;

        }

        public override void Exit(bool globalTransition)
        {
            
        }
    }

    private void Awake()
    {
        controller = GetComponent<StarfallAIController>();
    }

    private void Start()
    {

        var demoEnemyFsmData = new DemoEnemyFsmData(this, controller);
        _demoFsm = new FiniteStateMachine<DemoEnemyFsmData>(demoEnemyFsmData);
        
        _demoFsm.AddState(new ChaseState());
        _demoFsm.AddState(new IdleState(), true);
        _demoFsm.AddState(new MoveAndFireState());
        

    }

    private void Update()
    {
        _demoFsm.Update();
    }
}
