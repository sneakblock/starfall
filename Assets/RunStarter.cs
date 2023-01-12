using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunStarter : MonoBehaviour
{
    public void Init()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.StartRun();
        }
    }
}
