using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StateMachine;

public class DemoEnemyStateMachine : MonoBehaviour
{

    public StarfallAIController controller;

    /// <summary>
    /// This struct is the "type" of the state machine, meaning that each state will have access to this data at runtime.
    /// This is useful as functionality expands-- for now, the states will have access to the entity they are controlling and the player, later they could
    /// have access to team blackboards, etc.
    /// </summary>
    struct DemoEnemyFsmData
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
        
        

    }
    
}
