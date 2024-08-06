using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour

{
    private Animator animator;
    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Open()
    {
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(0.05f);

        Time.timeScale = 1;

        StartCoroutine(playerController.ChestOpenShake());

        animator.Play("ChestOpen");
        GetComponent<Collider>().enabled = false;
    }
}
