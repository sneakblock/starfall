using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

[CreateAssetMenu()]
public class BehaviorTree : ScriptableObject
{
    public Node rootNode;
    public Node.State treeState = Node.State.Running;

    public Node.State Update()
    {
        if (rootNode.state == Node.State.Running)
        {
            treeState = rootNode.Update();
        }

        return treeState;
    }
    
    //TODO: Can add a reset function in here to re-run the tree if it has completed.
}
