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

    private MonsterController attackedBy;
    public Animator animator;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

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
        if (Input.GetButtonDown("Fire1") && carying.GetComponent<ItemInfo>().itemType.ToString() == "Weapon" ) Attack();

        if (waitingForKey) CheckQTE();

        // RMB for testing, delete later
        //if (Input.GetButtonDown("Fire2")) StartQTE();
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

        else if (lookingAt.CompareTag("Item") || lookingAt.CompareTag("Weapon") && !carying && (lookingAt.transform.position - transform.position).magnitude <= 1.5f)
        {
            carying = lookingAt;
            lookingAt.transform.SetParent(this.transform);
            lookingAt.transform.localPosition = new Vector3(0, 0, 0.5f);
            lookingAt.transform.localRotation = Quaternion.Euler(45f, 0, 0);
            lookingAt.transform.localScale = new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z);
            lookingAt.layer = 9;
            // Pick-Up animation
            //Destroy(lookingAt);
        }
    }

    private void Attack()
    {
        // Play attack animation (Unity parameter für die animation setzen und als Bedingung (Condition) festlegen)
        animator.SetTrigger("Attack");
        // Check for hit
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        // Alien react
        foreach(Collider enemy in hitEnemies)
        {
            Debug.Log("Enemy hit " + enemy);
            enemy.GetComponent<MonsterController>().RunAway(transform);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.DrawSphere(attackPoint.position, attackRange);
    }

    public void StartQTE(MonsterController monster)
    {
        attackedBy = monster;
        if (!waitingForKey)
        {
            waitingForKey = true;

            if (carying.GetComponent<ItemInfo>().itemType.ToString() == "Weapon")
            {
                int num = Random.Range(0, 3);
                keyToPress = keys[num];
                //displayBox.GetComponent<Text>().text = keyToPress;
                Debug.Log(num + "  Press " + keyToPress);
                Invoke("HandleQTEFail", 2f);
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
            waitingForKey = false;

            if (Input.GetButtonDown(keyToPress + "Key")) //right Key
            {
                Debug.Log("You win!");
                attackedBy.RunAway(transform);
                attackedBy = null;
                CancelInvoke("HandleQTEFail");
                // Play counter animation
                
            }
            else
            {
                CancelInvoke("HandleQTEFail");
                HandleQTEFail();
            }
        }
    }

    private void HandleQTEFail()
    {
        Debug.Log("FAILED! You died!");
    }
}
