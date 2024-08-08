using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firewall : MonoBehaviour
{
    public GameObject roomTrigger;

    [HideInInspector] public bool firewallActive = false;

    private MeshRenderer mr;

    void Start()
    {
        mr = this.gameObject.transform.parent.GetComponent<MeshRenderer>();
        mr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateFireWall()
    {
        firewallActive = true;
        this.gameObject.transform.parent.gameObject.layer = LayerMask.NameToLayer("Default");
        mr.enabled = true;
    }
}
