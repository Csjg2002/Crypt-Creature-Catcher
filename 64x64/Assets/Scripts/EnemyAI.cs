using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    private GameObject player;
    private GameObject UI;

    private NavMeshAgent enemyAgent;

    private Vector3 destination;
    private bool isMoving = false;
    private bool isChasing = false;

    public float chaseDistance = 10f;
    public float chaseSpeed = 12f;
    public float normalSpeed = 6f;

    private Coroutine moveCoroutine;
    private Coroutine chaseCoroutine;
    private Coroutine attackCoroutine;

    private float distanceToPlayer;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
        UI = FindObjectOfType<UI>().gameObject;
        enemyAgent = GetComponent<NavMeshAgent>();
        StartCoroutine(MoveToNewLocation());
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < chaseDistance)
        {
            if (!isChasing)
            {
                isChasing = true;
                enemyAgent.speed = chaseSpeed;
                if (moveCoroutine != null)
                {
                    StopCoroutine(moveCoroutine);
                    moveCoroutine = null;
                }
                chaseCoroutine = StartCoroutine(ChasePlayer());
            }
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                enemyAgent.speed = normalSpeed;
                if (chaseCoroutine != null)
                {
                    StopCoroutine(chaseCoroutine);
                    chaseCoroutine = null;
                }
                moveCoroutine = StartCoroutine(MoveToNewLocation());
            }
        }

        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        Vector3 Lookdirection = player.transform.position - this.gameObject.transform.position;
        Quaternion Lookrotation = Quaternion.LookRotation(Lookdirection);
        this.gameObject.transform.rotation = Lookrotation;
    }

    private IEnumerator MoveToNewLocation()
    {
        while (true)
        {
            if (!isMoving && !isChasing)
            {
                yield return StartCoroutine(FindNewLocation());
                isMoving = true;
                enemyAgent.SetDestination(destination);
                yield return new WaitForSeconds(Random.Range(3, 6));
            }
            yield return null;

            while (isMoving)
            {
                if (enemyAgent.pathPending || enemyAgent.pathStatus != NavMeshPathStatus.PathComplete)
                {
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                if (Vector3.Distance(transform.position, destination) < 1)
                {
                    isMoving = false;
                    yield return new WaitForSeconds(Random.Range(3, 6));
                }
                yield return null;
            }
        }
    }

    private IEnumerator FindNewLocation()
    {
        int retries = 0;
        bool foundValidLocation = false;

        while (retries < 10 && !foundValidLocation)
        {
            float randomX = Random.Range(-10, 10);
            float randomZ = Random.Range(-10, 10);
            destination = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                destination = hit.position;
                foundValidLocation = true;
            }
            else
            {
                retries++;
                yield return new WaitForSeconds(0.1f);
            }
        }

        if (!foundValidLocation)
        {
            destination = transform.position;
            yield return new WaitForSeconds(2);
        }
    }

    private IEnumerator ChasePlayer()
    {
        while (isChasing)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (distanceToPlayer <= chaseDistance)
            {
                if (distanceToPlayer > 1.5f)
                {
                    Vector3 directionToPlayer = player.transform.position - transform.position;
                    destination = transform.position + directionToPlayer.normalized * chaseDistance;

                    if (NavMesh.SamplePosition(destination, out NavMeshHit hit, chaseDistance, NavMesh.AllAreas))
                    {
                        destination = hit.position;
                    }
                    else
                    {
                        destination = transform.position;
                    }

                    enemyAgent.SetDestination(destination);
                }
                else
                {
                    destination = transform.position;
                    enemyAgent.SetDestination(destination);

                    if(distanceToPlayer <= 1.5f)
                    {
                        if(attackCoroutine == null)
                        {
                            attackCoroutine = StartCoroutine(Attack());
                        }
                    }
                }
            }
            else
            {
                destination = transform.position;
                enemyAgent.SetDestination(destination);
            }

            if (enemyAgent.pathPending || enemyAgent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            yield return null;
        }
    }

    private IEnumerator Attack()
    {
        yield return new WaitForSeconds(1.5f);

        if(distanceToPlayer <= 1.5f)
        {
            StartCoroutine(UI.GetComponent<UI>().DamageIndicator());
            player.GetComponent<PlayerController>().healthSlider.value--;
            attackCoroutine = null;
        }
        else
        {
            attackCoroutine = null;
            yield return null;
        }
    }
}
