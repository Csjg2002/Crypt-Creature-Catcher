using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CreatureAI : MonoBehaviour
{
    private GameObject player;

    private NavMeshAgent creatureAgent;

    private Vector3 destination;
    private bool isMoving = false;
    private bool isFleeing = false;

    public float fleeDistance = 10f;
    public float fleeSpeed = 12f;
    public float normalSpeed = 6f;

    private Coroutine moveCoroutine;
    private Coroutine fleeCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
        creatureAgent = GetComponent<NavMeshAgent>();
        StartCoroutine(MoveToNewLocation());
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < fleeDistance)
        {
            if (!isFleeing)
            {
                isFleeing = true;
                creatureAgent.speed = fleeSpeed;
                if (moveCoroutine != null)
                {
                    StopCoroutine(moveCoroutine);
                    moveCoroutine = null;
                }
                fleeCoroutine = StartCoroutine(FleeFromPlayer());
            }
        }
        else
        {
            if (isFleeing)
            {
                isFleeing = false;
                creatureAgent.speed = normalSpeed;
                if (fleeCoroutine != null)
                {
                    StopCoroutine(fleeCoroutine);
                    fleeCoroutine = null;
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
            if (!isMoving && !isFleeing)
            {
                yield return StartCoroutine(FindNewLocation());
                isMoving = true;
                creatureAgent.SetDestination(destination);
                yield return new WaitForSeconds(Random.Range(3, 6));
            }
            yield return null;

            while (isMoving)
            {
                if (creatureAgent.pathPending || creatureAgent.pathStatus != NavMeshPathStatus.PathComplete)
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

    private IEnumerator FleeFromPlayer()
    {
        while (isFleeing)
        {
            Vector3 directionAwayFromPlayer = transform.position - player.transform.position;
            Vector3 potentialDestination = transform.position + directionAwayFromPlayer.normalized * fleeDistance;

            if (NavMesh.SamplePosition(potentialDestination, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            {
                destination = hit.position;
                creatureAgent.SetDestination(destination);
            }
            else
            {
                destination = transform.position;
                creatureAgent.SetDestination(destination);
                yield return new WaitForSeconds(2);
            }

            if (creatureAgent.pathPending || creatureAgent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            yield return null;
        }
    }
}
