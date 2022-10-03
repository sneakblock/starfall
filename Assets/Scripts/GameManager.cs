using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    [Header("Player Character")] public APlayer aPlayer;
    public RangedWeapon playerWeapon { get; private set; }
    public Light dirLight;

    // TODO(mish): have SCharacter main player and set up a function to switch
    // between different players
    private SCharacter _currentPlayer;
    //private Kuze _kuze;
    
    // TODO(cameron): use more specific types if desired
    public static Action<APlayer> PlayerDeath;
    public static Action<GameObject> EnemyDeath;

    private void Awake()
    {
        //Initialize the instance of the Game Manager.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        
        if (!aPlayer) TryFindAPlayer();
        if (!dirLight) dirLight = TryFindDirLight();
        
        PlayerDeath += OnPlayerDeath;
        EnemyDeath += OnEnemyDeath;
    }

    void TryFindAPlayer()
    {
        aPlayer = GameObject.FindWithTag("Player").GetComponent<APlayer>();
    }

    Light TryFindDirLight()
    {
        foreach (var light in GameObject.FindObjectsOfType<Light>())
        {
            if (light.type == LightType.Directional) return light;
        }

        return null;
    }
    
    private void OnPlayerDeath(APlayer player)
    {
        Debug.Log("GAME OVER YEEEEEEEEEEEEEEAAAAAAAAAAAAAAAAAH");
        // Do whatever cleanup
        PlayerDeath -= OnPlayerDeath;
        EnemyDeath -= OnEnemyDeath;
        Invoke("LoadCharacterSelectScene", 3.0f);
    }
    
    private void OnEnemyDeath(GameObject enemy)
    {
        Debug.Log("bitchass mofo dead");
        Destroy(enemy);
    }

    private void LoadCharacterSelectScene()
    {
        //Replace the Testing scene with the name of the character select scene
        SceneManager.LoadScene("ElizabethTesting", LoadSceneMode.Single);
    }
}
