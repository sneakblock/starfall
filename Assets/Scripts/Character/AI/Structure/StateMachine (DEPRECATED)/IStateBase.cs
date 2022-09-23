using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public interface IStateBase<T>
    {
        string Name { get; }
        void Init(IFiniteStateMachine<T> fsm, T param);
        DeferredStateTransitionBase<T> Update();
        void Exit(bool globalTransition);
        void Exit();
    }
}

