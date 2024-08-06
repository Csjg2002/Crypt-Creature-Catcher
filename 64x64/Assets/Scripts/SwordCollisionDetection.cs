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
            if (!enemiesToAttack.Contains(other.gameObject))
            {
                enemiesToAttack.Add(other.gameObject);
            }
        }
        else if (other.tag == "Chest")
        {
            chestToOpen = null;
        }
    }

    public void Attack()
    {
        foreach (GameObject enemy in enemiesToAttack)
        {
            if(enemy != null)
            {
                Vector3 snapPosition = new Vector3(enemy.transform.position.x, enemy.transform.position.y, player.transform.position.z + player.transform.forward.z * 1.5f);
                enemy.transform.parent.position = snapPosition;

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

        if (chestToOpen != null)
        {
            StartCoroutine(chestToOpen.GetComponent<Chest>().Open());
            chestToOpen = null;
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
            Instantiate(deadSwitch, enemy.gameObject.transform.position, enemy.gameObject.transform.rotation);
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
