using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour

{
    private Animator animator;
    private PlayerController playerController;

    public GameObject healthPrefab; // Reference to the health prefab
    public GameObject staminaPrefab; // Reference to the stamina prefab

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = FindObjectOfType<PlayerController>();
    }

    public IEnumerator Open()
    {
        GetComponent<Collider>().enabled = false;

        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(0.05f);

        Time.timeScale = 1;

        StartCoroutine(playerController.ChestOpenShake());

        animator.Play("ChestOpen");

        // Spawn health and stamina items
        SpawnItems(healthPrefab, Random.Range(2,6));
        SpawnItems(staminaPrefab, Random.Range(2,6));
    }

    private void SpawnItems(GameObject prefab, int quantity)
    {
        if (prefab == null) return;

        for (int i = 0; i < quantity; i++)
        {
            // Instantiate the item
            GameObject item = Instantiate(prefab, transform.position, Quaternion.identity);

            // Calculate direction towards the player
            Vector3 direction = (playerController.transform.position - transform.position).normalized;

            // Set the upward and forward forces
            Vector3 upwardDirection = direction;
            upwardDirection.y = 1; // Adjust this value to control how high the items go

            // Add force to the item to move it towards the player with an upward arc
            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Apply a force that includes both upward and forward direction
                rb.AddForce(upwardDirection * 50, ForceMode.Impulse);
            }
        }
    }
}
