using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearPassThrough : MonoBehaviour
{
    private PlayerController player;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    public void SwitchGearPassThrough()
    {
        player.SwitchGear();
    }

    public void CheckForCreaturePassThrough()
    {
        player.CheckForCreature();
    }
}
