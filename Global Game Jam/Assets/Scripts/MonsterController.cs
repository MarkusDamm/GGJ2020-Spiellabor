using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
//using UnityEngine.SceneManagement; // Fürs neuladen der Scene

public class MonsterController : MonoBehaviour
{
    public NavMeshAgent agent;
    private static float MicLoudness;
    private string device;
    AudioClip clipRecord;
    readonly int sampleWindow = 128;
    bool isInitialized; // Microphone
    bool isActive;  //KI
    // possible targets
    public Transform player;
    //public Transform[] targets;
    private Transform current;
    public List<Transform> targets;
    // random number for targets
    bool isFollowingPlayer;
    bool isMonsterShouted = false;
    readonly float killRange = 1.5f;

    //add by tw
    AudioSource audioSource;
    public AudioClip shoutingClip;
    //add by tw end

    private void Awake()
    {
        // current = player;
        isActive = false;
        // Debug.Log(current.name);
        isFollowingPlayer = false;
        //for (int i = 0; i < targets.Count; i++)
        //{
        //    Debug.Log(targets[i].name + ":   " + targets[i].position);
        //}
    }

    private void Start()
    {
        //add by tw
        audioSource = GetComponent<AudioSource>();
        //add by tw end
        player = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            // for disctance between Monster and player
            var playerHeading = player.position - this.transform.position;
            var playerDistance = playerHeading.magnitude;

            if (!agent.CalculatePath(this.current.position, new NavMeshPath()))   // if there's no path to target -> get another target
            {
                SwitchFollowing();
            }
            else agent.SetDestination(current.position);

            // levelMax equals to the highest normalized value power 2, a small number because < 1
            // pass the value to a static var so we can access it from anywhere
            MicLoudness = LevelMax();

            // for distance between Monster and current target
            var distance = (current.position - transform.position).magnitude;

            // MouseInput for testing. 
            // Proplem: the same script on more than 1 Object only works for the first object
            // Seems like the 2nd object can't access the microphone
            //if (!isFollowingPlayer && (MicLoudness * 2 >= 0.01f || Input.GetMouseButtonDown(0)))
            //{
            //    isFollowingPlayer = true;
            //    current = player;
            //    Debug.Log(MicLoudness);
            //    // Move towards target position
            //    agent.SetDestination(current.position);
            //    agent.speed = 1.5f + MicLoudness * 50.0f;
            //    if (IsInvoking("SwitchFollowing"))
            //        CancelInvoke("SwitchFollowing");
            //    Invoke("SwitchFollowing", 5.0f);
            //}
            if (distance < 1.0f && !isFollowingPlayer)
            {
                new WaitForSeconds(2);
                ChooseTarget();
            }

            float playerAngle = Vector3.Angle(playerHeading, transform.forward);
            Debug.DrawRay(transform.position, 10 * playerHeading, Color.white);

            if (playerAngle <= 50.0f && playerDistance <= 8.0f)
            {
                //Debug.Log("Player in angle and range");

                Ray ray = new Ray(transform.position, playerHeading);
                Physics.Raycast(ray, out RaycastHit hitinfo);
                Debug.DrawRay(ray.origin, 10 * ray.direction, Color.magenta);
                int hitLayer = hitinfo.collider.gameObject.layer;
                //Debug.Log(hitinfo.collider.gameObject.layer);
                bool seen = (hitLayer == 9 && playerAngle < 30);    // Layer 9 == Player
                if (seen)
                {
                    //Debug.Log("I see you " + seen);
                    if (IsInvoking("SwitchFollowing")) CancelInvoke("SwitchFollowing");

                    if (!isFollowingPlayer) Invoke("SwitchFollowing", 5.0f);

                    isFollowingPlayer = true;
                    current = player;
                    // Move towards target position
                    agent.SetDestination(current.position);
                    agent.speed = 1.5f + MicLoudness * 50.0f;
                }
            }
            if (isFollowingPlayer && playerDistance <= killRange)
            {
                player.GetComponent<PlayerController>().StartQTE(this);
                // Start QTE
            }
        }
    }

    private void SwitchFollowing()
    {
        isFollowingPlayer = false;
        ChooseTarget();
        Debug.Log("Switch Following");
        agent.speed = 2;
    }

    private void ChooseTarget()
    {
        int num = Random.Range(0, targets.Count);
        current = targets[num];
        transform.LookAt(current);
        new WaitForSecondsRealtime(1);
        agent.SetDestination(current.position);
        Debug.Log(current.name);
        // Das letzt Element scheint verbuggt zu sein
    }

    public void RunAway(Transform attacker)
    {

        agent.SetDestination(new Vector3((transform.position.x - attacker.position.x) * 2f + transform.position.x, transform.position.y, (transform.position.z - attacker.position.z) * 3f + transform.position.z));
        isActive = false;
        Invoke("ActivateMonster", 4f);

    }

    private void InitMic()
    {
        if (device == null) device = Microphone.devices[0];
        clipRecord = Microphone.Start(device, true, 999, 44100);
    }

    void StopMicrophone()
    {
        Microphone.End(device);
    }

    float LevelMax()
    {
        float levelMax = 0;
        float[] waveData = new float[sampleWindow];
        int micPosition = Microphone.GetPosition(null) - (sampleWindow + 1);
        // null means the first microphone

        if (micPosition < 0) return 0;
        clipRecord.GetData(waveData, micPosition);
        // Getting a peak on the last 128 samples
        for (int i = 0; i < sampleWindow; i++)
        {
            float wavePeak = waveData[i] * waveData[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }
        return levelMax;
    }

    // start mic when scene starts
    void OnEnable()
    {
        InitMic();
        isInitialized = true;
    }

    //stop mic when loading a new level or quit application
    void OnDisable()
    {
        StopMicrophone();
    }

    void OnDestroy()
    {
        StopMicrophone();
    }

    // make sure the mic gets started & stopped when application gets focused
    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            //Debug.Log("Focus");
            if (!isInitialized)
            {
                //Debug.Log("Init Mic");
                InitMic();
                isInitialized = true;
            }
        }
        if (!focus)
        {
            //Debug.Log("Pause");
            StopMicrophone();
            //Debug.Log("Stop Mic");
            isInitialized = false;
        }
    }

    public void ActivateMonster()
    {
        isActive = true;
        SwitchFollowing();
        // Play Alien Sound? 
    }

}