using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToMenu : MonoBehaviour
{
    public void Init()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.ReturnToMenu();
        }
    }
}
