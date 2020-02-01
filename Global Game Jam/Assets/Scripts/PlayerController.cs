using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Camera cam;

    public float speed = 8f;
    public float lookingSpeed = 2f;

    float verticalRotation = 0f;
    public float yRange = 60f;

    public GameObject displayBox;
    readonly string[] keys = {"E", "R", "T" };
    private string keyToPress;
    private bool waitingForKey = false;

    public GameObject dogo;
    GameObject carying;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        Cursor.visible = false;
        dogo = GameObject.Find("Dogo");
        carying = null;
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate L & R
        float rotateX = Input.GetAxis("Mouse X") * lookingSpeed;
        transform.Rotate(0, rotateX, 0);

        // Rotate Up & Down
        verticalRotation -= Input.GetAxis("Mouse Y") * lookingSpeed;
        verticalRotation = Mathf.Clamp(verticalRotation, -yRange, yRange);
        cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

        // Movement
        float forwardSpeed = Input.GetAxis("Vertical") * speed;
        float sideSpeed = Input.GetAxis("Horizontal") * speed;
        Vector3 movement = new Vector3(sideSpeed, 0, forwardSpeed);
        movement = transform.rotation * movement;
        CharacterController cc = GetComponent<CharacterController>();
        cc.SimpleMove(movement);

        if (Input.GetButtonDown("Interact"))    Interact();
        if (Input.GetButtonDown("Fire1") && carying.tag == "Weapon") Attack();

        if (waitingForKey) CheckQTE();

    }

    private void Interact()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        Physics.Raycast(ray, out RaycastHit hitinfo);
        if (!hitinfo.collider) return;
        GameObject lookingAt = hitinfo.collider.gameObject;
        if (lookingAt == dogo)
        {
            Debug.Log("Looking at dogo");
            if (carying)
            {
                dogo.GetComponent<DogoController>().TakeObject(carying);
                
                carying = null;
                // place Item on Dogo
            }
            else if ( lookingAt.GetComponent<DogoController>().objectsCarrying.Count > 0 && lookingAt.name != "Dogo" )
            {
                carying = lookingAt;
                lookingAt.GetComponent<DogoController>().objectsCarrying.Remove(lookingAt);
            }
        }

        else if (lookingAt.CompareTag("Item") || lookingAt.CompareTag("Weapon") && !carying)
        {
            carying = lookingAt;
            lookingAt.transform.SetParent(this.transform);
            lookingAt.transform.localPosition = new Vector3(0, 0, 0.5f);
            lookingAt.transform.localRotation = Quaternion.Euler(45f, 0, 0);
            // Pick-Up animation
            //Destroy(lookingAt);
        }
    }

    private void Attack()
    {
        //int ammo = carying.GetComponent<WeaponScript>().ammonition;
        //if (ammo > 0)
        //{
        //    carying.GetComponent<WeaponScript>().ammonition--;

        //}
        //else
        //{
        //    // Play Click sound
        //}
    }

    public void StartQTE()
    {
        if (!waitingForKey)
        {
            waitingForKey = true;

            if (carying.GetComponent<ItemInfo>().isWeapon)
            {
                keyToPress = keys[Random.Range(1, 4)];
                displayBox.GetComponent<Text>().text = keyToPress;
                Debug.Log(keyToPress);
                Invoke("HandleQTEFail", 1.5f);
            }
            else
            {
                // GameOver
            }
        }
    }

    private void CheckQTE()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(keyToPress + "Key")) //right Key
            {
                CancelInvoke("HandleQTEFail");

            }
            else
            {
                HandleQTEFail();
            }
        }
    }

    private void HandleQTEFail()
    {
        Debug.Log("FAILED! You died!");
    }
}
