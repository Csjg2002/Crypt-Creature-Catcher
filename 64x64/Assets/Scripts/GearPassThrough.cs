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

    public void ResetCanSwitch()
    {
        player.canSwitch = true;
        player.canMove = true;
    }

    public IEnumerator CreatureHitStop()
    {
        if(player.hasFoundCreature)
        {
            Time.timeScale = 0;

            yield return new WaitForSecondsRealtime(0.05f);

            Time.timeScale = 1;
        }
    }
}
