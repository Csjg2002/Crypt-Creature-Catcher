using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIPass : MonoBehaviour
{
    public void DeathPass()
    {
        this.gameObject.transform.parent.GetComponent<EnemyAI>().Death();
    }


}
