using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SwordCollisionDetection : MonoBehaviour
{
    private GameObject player;
    private GameObject enemyToAttack;

    private bool canAttack = false;
    private bool shouldHitstop = false;

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
            canAttack = true;
            enemyToAttack = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            canAttack = false;
            enemyToAttack = null;
        }
    }

    public void Attack()
    {
        if (enemyToAttack != null)
        {
            enemyToAttack.GetComponentInParent<EnemyAI>().enemyHealth--;

            if (enemyToAttack.GetComponentInParent<EnemyAI>().enemyHealth <= 0)
            {
                shouldHitstop = true;
                StartCoroutine(enemyHurtIndicator(enemyToAttack, 0.1f));
            }
            else
            {
                enemyToAttack.GetComponentInParent<EnemyAI>().attackCount++;

                if (enemyToAttack.GetComponentInParent<EnemyAI>().attackCount >= 2)
                {
                    shouldHitstop = true;
                    enemyToAttack.GetComponentInParent<EnemyAI>().attackCount = 0;

                    NavMeshAgent enemyAgent = enemyToAttack.GetComponentInParent<NavMeshAgent>();
                    if (enemyAgent != null)
                    {
                        enemyAgent.enabled = false;

                        enemyToAttack.transform.parent.position += player.transform.forward * 0.75f;

                        enemyAgent.enabled = true;
                    }
                }

                StartCoroutine(enemyHurtIndicator(enemyToAttack, 0.05f));
            }

            player.GetComponent<PlayerController>().SwordStamina();
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

        if (enemyToAttack.GetComponentInParent<EnemyAI>().enemyHealth > 0)
        {
            enemy.GetComponent<SpriteRenderer>().material.color = Color.white;
        }
        else
        {
            Destroy(enemyToAttack.transform.parent.gameObject);
        }
    }
}
