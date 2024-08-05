using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearPassThrough : MonoBehaviour
{
    private GameObject player;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
    }

    public void SwitchGearPassThrough()
    {
        player.GetComponent<PlayerController>().SwitchGear();
    }
}
