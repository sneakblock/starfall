using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeSpawn : MonoBehaviour
{
    private float throw_power = 5f;
    public GameObject Grenade;
    private GameObject characterParent;
    // Start is called before the first frame update
    void Start()
    { 
        characterParent = transform.parent.gameObject;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateGrenade(Vector3 direction) {
        Vector3 launchLocation = characterParent.transform.position + new Vector3(0.1f, 1.2f, 0);
        GameObject createdGrenade = Instantiate(Grenade, launchLocation, Quaternion.identity);
        Physics.IgnoreCollision(characterParent.GetComponent<Collider>(), createdGrenade.GetComponent<Collider>());
        createdGrenade.GetComponent<Rigidbody>().velocity = (direction * throw_power);
    }
}
