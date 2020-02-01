using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Camera cam;

    public float speed = 8f;
    public float lookingSpeed = 2f;

    float verticalRotation = 0f;
    public float yRange = 60f;

    GameObject dogo;
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
        if (Input.GetButtonDown("Fire1") && carying.tag == "Weapon") Shoot();
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

        else if (lookingAt.tag == "Item" || lookingAt.tag == "Weapon" && !carying)
        {
            carying = lookingAt;
            lookingAt.transform.SetParent(this.transform);
            lookingAt.transform.localPosition = new Vector3(0, 0, 0.5f);
            lookingAt.transform.localRotation = Quaternion.Euler(45f, 0, 0);
            // Pick-Up animation
            //Destroy(lookingAt);
        }
    }

    private void Shoot()
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
        if (carying)
        {

        }
        else
        {
            // GameOver
        }
    }
}
