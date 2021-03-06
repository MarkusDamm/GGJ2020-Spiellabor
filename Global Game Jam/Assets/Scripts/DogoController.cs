﻿using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class DogoController : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject player;
    float playerDistance;
    public List<GameObject> objectsCarrying;
    readonly float distanceForFollow = 3.0f;
    private void Awake()
    {
        agent.stoppingDistance = distanceForFollow;
    }

    private void Start()
    {
        NavMeshHit closestHit;

        if (NavMesh.SamplePosition(gameObject.transform.position, out closestHit, 500f, NavMesh.AllAreas))
            gameObject.transform.position = closestHit.position;
        else
            Debug.LogError("Could not find position on NavMesh!");
    }

    private void Update()
    {
        //Debug.Log(gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("walking"));
        // for distance between Girl and PlayerCharacter
        var heading = player.transform.position - this.transform.position;
        playerDistance = heading.magnitude;

        // zu Spielerposition schicken (Höhe nicht beachten für schleichen durch Lüftungsschächte)
        agent.SetDestination(new Vector3(player.transform.position.x, this.transform.position.y, player.transform.position.z));
        if (!gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("walking") && !gameObject.GetComponent<Animator>().IsInTransition(0))
            gameObject.GetComponent<Animator>().CrossFade("walking", 0.2f);

        else if (playerDistance <= distanceForFollow)
        {
            Vector3 target = new Vector3(player.transform.position.x, 0, player.transform.position.z);
            transform.LookAt(target);
            if (!gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("idle") && !gameObject.GetComponent<Animator>().IsInTransition(0))
            {
                // When standing and not playing idle animation and not already transitioning -> change to idle
                gameObject.GetComponent<Animator>().CrossFade("idle", 0.2f);
            }
        }
        //Debug.Log(playerDistance);
    }

    public void TakeObject(GameObject takenObject)
    {
        objectsCarrying.Add(takenObject);

        takenObject.transform.SetParent(transform);

        takenObject.transform.localPosition = new Vector3( Mathf.Pow(-1f, objectsCarrying.Count) * 1 , 0.5f, 0);
        takenObject.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        takenObject.transform.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
        takenObject.layer = 10;
    }
    

}
