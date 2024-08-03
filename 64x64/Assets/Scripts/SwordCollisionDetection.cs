using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SwordCollisionDetection : MonoBehaviour
{
    private GameObject player;
    private GameObject enemyToAttack;

    private bool canAttack = false;

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
            enemyToAttack.GetComponent<EnemyAI>().enemyHealth--;

            if (enemyToAttack.GetComponent<EnemyAI>().enemyHealth <= 0)
            {
                StartCoroutine(enemyHurtIndicator(enemyToAttack, 0.1f));
            }
            else
            {
                StartCoroutine(enemyHurtIndicator(enemyToAttack, 0.05f));
            }

            player.GetComponent<PlayerController>().SwordStamina();

            NavMeshAgent enemyAgent = enemyToAttack.GetComponent<NavMeshAgent>();
            if (enemyAgent != null)
            {
                enemyAgent.enabled = false;

                enemyToAttack.transform.position += player.transform.forward * 0.75f;

                enemyAgent.enabled = true;
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
        enemy.GetComponent<SpriteRenderer>().material.color = Color.red;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(stopTime);
        Time.timeScale = 1;

        if (enemyToAttack.GetComponent<EnemyAI>().enemyHealth > 0)
        {
            enemy.GetComponent<SpriteRenderer>().material.color = Color.white;
        }
        else
        {
            Destroy(enemyToAttack);
        }
    }
}
