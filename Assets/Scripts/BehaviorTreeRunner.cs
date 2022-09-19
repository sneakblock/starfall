using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTreeRunner : MonoBehaviour
{
    private BehaviorTree _tree;
    // Start is called before the first frame update
    void Start()
    {
        _tree = ScriptableObject.CreateInstance<BehaviorTree>();
        var logNode = ScriptableObject.CreateInstance<DebugLogNode>();
        logNode.message = "Testmsg";
        _tree.rootNode = logNode;
    }

    // Update is called once per frame
    void Update()
    {
        _tree.Update();
    }
}
