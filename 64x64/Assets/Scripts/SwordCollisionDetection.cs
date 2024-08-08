using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SwordCollisionDetection : MonoBehaviour
{
    private GameObject player;
    private List<GameObject> enemiesToAttack = new List<GameObject>();

    private bool shouldHitstop = false;

    [HideInInspector] public bool shouldAttack = false;

    public GameObject deadSwitch;

    public GameObject bloodParticle;

    private GameObject chestToOpen;

    [HideInInspector] public EnemySpawner currentEncounter;
    [HideInInspector] public int enemiesRemaining;

    public GameObject healthPickup;
    public GameObject staminaPickup;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            if (!enemiesToAttack.Contains(other.gameObject))
            {
                enemiesToAttack.Add(other.gameObject);
            }
        }
        else if(other.tag == "Chest")
        {
            chestToOpen = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            if (enemiesToAttack.Contains(other.gameObject))
            {
                enemiesToAttack.Remove(other.gameObject);
            }
        }
        else if (other.tag == "Chest")
        {
            chestToOpen = null;
        }
    }

    public void Attack()
    {
        float highestDotProduct = 0;
        GameObject enemyToSnap = null;

        foreach (GameObject enemy in enemiesToAttack)
        {
            if (enemy != null)
            {
                Vector3 directionToEnemy = (enemy.transform.parent.position - player.transform.position).normalized;

                float dotProduct = Vector3.Dot(player.transform.forward, directionToEnemy);

                if (dotProduct > 0.7f)
                {
                    if (highestDotProduct < dotProduct)
                    {
                        highestDotProduct = dotProduct;
                        enemyToSnap = enemy;
                    }
                }
            }
        }

        if(enemyToSnap != null)
        {
            Vector3 snapPosition = new Vector3(enemyToSnap.transform.parent.position.x, enemyToSnap.transform.parent.position.y, player.transform.position.z + player.transform.forward.z * 1.5f);
            enemyToSnap.transform.parent.position = snapPosition;
        }

        foreach (GameObject enemy in enemiesToAttack)
        {
            if (enemy != null)
            {
                Vector3 directionToEnemy = (enemy.transform.parent.position - player.transform.position).normalized;

                float dotProduct = Vector3.Dot(player.transform.forward, directionToEnemy);

                if (dotProduct > 0.7f)
                {
                    StartCoroutine(player.GetComponent<PlayerController>().SwordAttackCameraShake(0.2f, 0.1f));

                    if (player.GetComponent<PlayerController>().isSprinting)
                    {
                        enemy.GetComponentInParent<EnemyAI>().enemyHealth -= 2;
                    }
                    else
                    {
                        enemy.GetComponentInParent<EnemyAI>().enemyHealth--;
                    }

                    enemy.GetComponentInParent<EnemyAI>().hasBeenAttacked = true;

                    if (enemy.GetComponentInParent<EnemyAI>().enemyHealth <= 0)
                    {
                        shouldHitstop = true;
                        StartCoroutine(enemyHurtIndicator(enemy, 0.15f));
                    }
                    else
                    {
                        enemy.GetComponentInParent<EnemyAI>().attackCount++;

                        if (enemy.GetComponentInParent<EnemyAI>().attackCount >= 2 && shouldAttack)
                        {
                            shouldHitstop = true;
                            enemy.GetComponentInParent<EnemyAI>().attackCount = 0;

                            NavMeshAgent enemyAgent = enemy.GetComponentInParent<NavMeshAgent>();
                            if (enemyAgent != null)
                            {
                                enemyAgent.enabled = false;
                                enemy.transform.parent.position += player.transform.forward * 0.75f;
                                enemyAgent.enabled = true;
                            }
                        }

                        shouldAttack = false;
                        StartCoroutine(enemyHurtIndicator(enemy, 0.15f));
                    }

                    if (!shouldHitstop)
                    {
                        StartCoroutine(enemyHurtIndicator(enemy, 0.1f));
                    }
                }
            }
        }

        if (chestToOpen != null)
        {
            Vector3 directionToChest = (chestToOpen.transform.position - player.transform.position).normalized;

            float dotProduct = Vector3.Dot(player.transform.forward, directionToChest);

            if (dotProduct > 0.7f)
            {
                StartCoroutine(chestToOpen.GetComponent<Chest>().Open());
                chestToOpen = null;
            }
        }
    }

    private IEnumerator StopAgentForShortDuration(NavMeshAgent agent)
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(0.5f);
        agent.isStopped = false;
    }

    private IEnumerator enemyHurtIndicator(GameObject enemy, float stopTime)
    {
        Vector3 forwardDirection = enemy.transform.forward;

        float distanceInFront = 1.5f;

        Vector3 positionInFront = player.gameObject.transform.position - forwardDirection * distanceInFront;

        Instantiate(bloodParticle, positionInFront, enemy.transform.rotation);

        enemy.GetComponent<SpriteRenderer>().material.color = Color.red;

        if(shouldHitstop)
        {
            Time.timeScale = 0;
        }

        yield return new WaitForSecondsRealtime(stopTime);
        
        if(shouldHitstop)
        {
            Time.timeScale = 1;
        }

        shouldHitstop = false;
        enemy.GetComponentInParent<EnemyAI>().hasBeenAttacked = false;
        enemy.GetComponent<SpriteRenderer>().material.color = Color.white;

        if (enemy.GetComponentInParent<EnemyAI>().enemyHealth <= 0)
        {
            enemiesRemaining--;
            Instantiate(deadSwitch, enemy.gameObject.transform.position, enemy.gameObject.transform.rotation);

            int pickupType = Random.Range(0, 7);

            switch (pickupType)
            {
                case 0:
                    Instantiate(healthPickup, enemy.transform.position, Quaternion.identity);
                    break;
                case 1:
                    Instantiate(staminaPickup, enemy.transform.position, Quaternion.identity);
                    break;
                case 2:
                    Vector3 offset = Random.onUnitSphere;
                    offset.y = 0;
                    offset.Normalize();
                    offset *= 0.5f;

                    Instantiate(healthPickup, enemy.transform.position + offset, Quaternion.identity);
                    Instantiate(staminaPickup, enemy.transform.position - offset, Quaternion.identity);
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
            }

            Destroy(enemy.transform.parent.gameObject);
        }
    }

    public void ReactivateSwordSwing()
    {
        player.GetComponent<PlayerController>().canSwingSword = true;


        if (player.GetComponent<PlayerController>().hasQueuedInput)
        {
            player.GetComponent<PlayerController>().hasQueuedInput = false;
            player.GetComponent<PlayerController>().Action();
        }
    }

    public void DeactivateSwordTrail()
    {
        player.GetComponent<PlayerController>().SwordTrailDeactivate();
    }
}
