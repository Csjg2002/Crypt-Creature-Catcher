using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyPS : MonoBehaviour
{
    public float timeToDestroy;

    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        StartCoroutine(AutoDestroy());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(timeToDestroy);
        player.isHealing = false;
        player.isResting = false;
        Destroy(this.gameObject);
    }
}
