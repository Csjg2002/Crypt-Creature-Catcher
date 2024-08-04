using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDead : MonoBehaviour
{
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        Vector3 lookDirection = player.transform.position - this.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        this.transform.localRotation = lookRotation;
    }
}
