using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class KnifeInventory : MonoBehaviour
{
    private APlayer _player;
    private List<Dagger> _knives;

    [SerializeField]
    [Tooltip("put in the dagger icons!")]
    public List<Image> _icons;


    void Start()
    {
        _player = GameManager.Instance.aPlayer;
        _knives = _player.GetComponent<DaggerAbility>()._daggers;

    }

    void Update()
    {
        UpdateInventory();
    }

    public void UpdateInventory()
    {
        int counter = 0;

        foreach (Dagger k in _knives)
        {
            if (k.IsHeld())
            {
                counter++;
            }
        }

        for (int i = 0; i < 7; i++)
        {
            if (i < counter)
            {
                Show(_icons[i]);
            }
            else
            {
                Hide(_icons[i]);
            }
        }
        Debug.Log($"isHeld counter: {counter}");
    }

    private void Show(Image i)
    {
        //Color c = i.color;
        //c.a = 1;
        //i.color = c;
        i.color = Color.white;
    }

    private void Hide(Image i)
    {
        //Color c = i.color;
        //c.a = (float) 0.1;
        //i.color = c;
        i.color = Color.black;
    }

}
