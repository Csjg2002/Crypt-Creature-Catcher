using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SwordCollisionDetection : MonoBehaviour
{
    private GameObject player;
    private bool canAttack;
    private GameObject enemyToAttack;

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
        if(other.tag == "Enemy")
        {
            if (player.GetComponent<PlayerController>().isAttacking)
            {
                canAttack = true;
                enemyToAttack = other.gameObject;
            }
            else
            {
                canAttack = false;
                enemyToAttack = null;
            }
        }
        else
        {
            canAttack= false;
            enemyToAttack = null;
        }
    }

    public void Attack()
    {
        player.GetComponent<PlayerController>().SwordStamina();

        if (enemyToAttack != null && canAttack)
        {
            StartCoroutine(enemyHurtIndicator(enemyToAttack));

            Vector3 playerForwardDirection = player.transform.forward;

            Rigidbody enemyRigidbody = enemyToAttack.GetComponent<Rigidbody>();
            if (enemyRigidbody != null)
            {
                float knockbackForce = 5f;
                enemyRigidbody.AddForce(playerForwardDirection * knockbackForce, ForceMode.Impulse);
            }

            player.GetComponent<PlayerController>().isAttacking = false;
            canAttack = false;

            //StartCoroutine(resetEnemyRB(enemyRigidbody));
        }
    }

    private IEnumerator enemyHurtIndicator(GameObject enemy)
    {
        enemy.GetComponent<SpriteRenderer>().material.color = Color.red;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1;
        yield return new WaitForSeconds(0.1f);
        enemy.GetComponent<SpriteRenderer>().material.color = Color.white;
    }

    private IEnumerator resetEnemyRB(Rigidbody enemyRB)
    {
        enemyRB.isKinematic = false;
        yield return new WaitForSeconds(1);
        enemyRB.isKinematic = true;
        yield return new WaitForSeconds(0.1f);
        enemyRB.isKinematic = false;
    }
}
