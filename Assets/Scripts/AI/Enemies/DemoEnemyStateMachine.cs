using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine;
using Random = UnityEngine.Random;

public class DemoEnemyStateMachine : MonoBehaviour
{

    public StarfallAIController controller;
    private FiniteStateMachine<DemoEnemyFsmData> _demoFsm;

    public const string IdleStateName = "Idle";
    public const string ChaseStateName = "Chase";

    /// <summary>
    /// This struct is the "type" of the state machine, meaning that each state will have access to this data at runtime.
    /// This is useful as functionality expands-- for now, the states will have access to the entity they are controlling and the player, later they could
    /// have access to team blackboards, etc.
    /// </summary>
    public struct DemoEnemyFsmData
    {
        public DemoEnemyStateMachine demoFsm { get; private set; }
        public  StarfallPlayer player { get; private set; }
        public StarfallAIController controller { get; private set; }

        public DemoEnemyFsmData(DemoEnemyStateMachine demoFsm, StarfallAIController controller)
        {
            this.demoFsm = demoFsm;
            this.controller = controller;
            this.player = StarfallPlayer.Instance;
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
        protected StarfallPlayer Player;
        
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

        //TODO: Make this less stupid :)
        public Vector3 CalculateRandomPosInPlayerCircle(float radius)
        {
            var rand = Random.insideUnitCircle * radius;
            var pos = Player.character.transform.position;
            return new Vector3(pos.x + rand.x, pos.y, pos.z + rand.y);
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

            //Spin
            var inputs = Controller.InitInputs();
            inputs.LookVector = Quaternion.AngleAxis(50, Vector3.up) * Controller.character.transform.forward * Time.deltaTime;
            Controller.AssignInputsToCharacter(inputs);

            if (!IsWithinRangeOfPlayer(Controller.character.transform.position,Controller.leashRange))
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
        private Vector3 _targetLoc;

        public override void Init(IFiniteStateMachine<DemoEnemyFsmData> parentFsm, DemoEnemyFsmData demoFsmData)
        {
            base.Init(parentFsm, demoFsmData);
            _goToIdleStateTransition = parentFsm.CreateStateTransition(IdleStateName);
        }

        public override void Enter(StarfallCharacterController s)
        {
            base.Enter(s);
            _chaseTarget = s;
            bool b;
            do
            {
                _targetLoc = CalculateRandomPosInPlayerCircle(Controller.leashRange);
                b = Controller.SetPath(_targetLoc);
                Debug.Log("found new path");
            } while (!b);
        }

        public override void Exit(bool globalTransition)
        {
            
        }

        public override DeferredStateTransitionBase<DemoEnemyFsmData> Update()
        {
            DeferredStateTransition<DemoEnemyFsmData> ret = null;

            if (!IsWithinRangeOfPlayer(_targetLoc, Controller.leashRange))
            {
                bool b;
                do
                {
                    _targetLoc = CalculateRandomPosInPlayerCircle(Controller.leashRange);
                    b = Controller.SetPath(_targetLoc);
                    Debug.Log("found new path");
                } while (!b);
            }

            var inputs = Controller.InitInputs();
            inputs = Controller.FollowPath(inputs);
            Controller.AssignInputsToCharacter(inputs);
            

            if (IsWithinRangeOfPlayer(Controller.character.transform.position, Controller.leashRange) && !Controller.HasPath())
            {
                ret = _goToIdleStateTransition;
            }

            return ret;

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
        
        

    }

    private void Update()
    {
        _demoFsm.Update();
    }
}
