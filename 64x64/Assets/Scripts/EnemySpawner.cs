using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public int numberOfEnemies = 2;

    [HideInInspector] public GameObject[] firewalls;

    private SwordCollisionDetection playerAttack;

    void Update()
    {
        if(playerAttack != null)
        {
            if (playerAttack.enemiesRemaining <= 0)
            {
                foreach (var firewall in firewalls)
                {
                    Destroy(firewall.transform.parent.gameObject);
                }

                Destroy(this.gameObject);
            }
        }
    }

    public void SpawnEnemies()
    {
        playerAttack = FindObjectOfType<SwordCollisionDetection>();

        Vector3 startPosition = transform.position;

        for (int i = 0; i < numberOfEnemies; i++)
        {
            float positionOffset = i * 2;

            GameObject enemyPrefab = GetEnemyPrefab(i);

            Vector3 spawnPosition;
            if (this.gameObject.transform.rotation.y == 0 || this.gameObject.transform.rotation.y == 180)
            {
                spawnPosition = startPosition + new Vector3(0, 0, positionOffset);
            }
            else
            {
                spawnPosition = startPosition + new Vector3(positionOffset, 0, 0);
            }

            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private GameObject GetEnemyPrefab(int index)
    {
        if (enemyPrefabs.Length == 0 || index >= enemyPrefabs.Length)
        {
            return null;
        }

        if(enemyPrefabs[index] != null)
        {
            playerAttack.enemiesRemaining = enemyPrefabs.Length;
        }

        return enemyPrefabs[index % enemyPrefabs.Length];
    }
}
