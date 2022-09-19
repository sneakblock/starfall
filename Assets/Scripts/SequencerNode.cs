using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequencerNode : CompositeNode
{
    private int curr;
    protected override void OnStart()
    {
        curr = 0;
    }

    protected override void OnStop()
    {
        
    }

    protected override State OnUpdate()
    {
        var child = children[curr];
        switch (child.Update())
        {
            case State.Running:
                return State.Running;
            case State.Failure:
                return State.Failure;
            case State.Success:
                curr++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return curr == children.Count ? State.Success : State.Running;
    }
}
