using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Vector3 startPosition;
    public string neededKey = "";
    int openState = 0;


    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (openState == 1)
        {
            transform.position = new Vector3(startPosition.x, transform.position.y + 0.03f, startPosition.z);
            if (transform.position.y >= startPosition.y + 3f) openState = 0;
        } 
        else if (openState == 2)
        {
            transform.position = new Vector3(startPosition.x, transform.position.y - 0.03f, startPosition.z);
            if (transform.position.y <= startPosition.y) openState = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collider detected: " + other.name);
        if (neededKey == "")
        {
            if (other.CompareTag("Player") && openState == 0)
            {
                openState = 1;
                // play Sound
                Invoke("CloseDoor", 7);
            }
        }
        else 
        {
            if (other.CompareTag("Player") && openState == 0)
            {
                string[] keyCards = other.GetComponent<PlayerController>().doorKeys;
                foreach (string key in keyCards)
                {
                    if (key == neededKey)
                    {
                        openState = 1;
                        Invoke("CloseDoor", 7);
                    }
                }
            }
        }
    }

    private void CloseDoor()
    {
        openState = 2;
        // play Sound
    }
}
