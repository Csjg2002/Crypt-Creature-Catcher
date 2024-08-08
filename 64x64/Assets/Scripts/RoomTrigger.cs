using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public GameObject[] firewalls;
    public EnemySpawner enemySpawner;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            foreach (var firewall in firewalls)
            {
                firewall.GetComponent<Firewall>().ActivateFireWall();
            }

            enemySpawner.firewalls = firewalls;
            enemySpawner.SpawnEnemies();
            Destroy(this.gameObject);
        }
    }
}
