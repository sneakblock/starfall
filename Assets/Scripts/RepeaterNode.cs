using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeaterNode : DecoratorNode
{
    //As of now, this just infinitely loops it's child node.
    //TODO: Can add number of repetitions, and conditional repeats, e.g only continue to loop until the child fails or succeeds, and so on.
    
    protected override void OnStart()
    {
        
    }

    protected override void OnStop()
    {
        
    }

    protected override State OnUpdate()
    {
        child.Update();
        return State.Running;
    }
}
