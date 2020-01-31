using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MonsterController : MonoBehaviour
{
    public NavMeshAgent agent;
    private static float MicLoudness;
    private string device;
    AudioClip clipRecord;
    readonly int sampleWindow = 128;
    bool isInitialized;
    bool isActive;
    // possible targets
    public Transform player;
    //public Transform[] targets;
    private Transform current;
    public List<Transform> targets;
    // random number for targets
    bool isFollowingPlayer;
    bool isMonsterShouted = false;
    readonly float killRange = 1f;

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
        palyer = GameObject.Find("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        // for disctance between Monster and player
        var playerHeading = player.position - this.transform.position;
        var playerDistance = playerHeading.magnitude;

        if (isActive == false)
        {
            Ray ray = new Ray(this.transform.position, playerHeading);
            Physics.Raycast(ray, out RaycastHit hitinfo);
            Debug.DrawRay(ray.origin, 10 * ray.direction, Color.magenta);
            string name = hitinfo.collider.tag;
            //Debug.Log(name + " " + System.DateTime.Now);
            bool seen = (name == "Player");

            if (playerDistance <= 8.0f && seen == true)
            {
                Debug.Log("Initalize Monster AI");
                //waiting for shouting
                if (isMonsterShouted == false)
                {
                    audioSource.PlayOneShot(shoutingClip);
                    isMonsterShouted = true;
                }
            }

            if (isMonsterShouted == true && audioSource.isPlaying == false)
            {
                isActive = true;
                isFollowingPlayer = true;
                agent.SetDestination(current.position);
            }
            return;
        }

        if (isActive == true)
        {
            if (agent.CalculatePath(this.current.position, new NavMeshPath()))   // if there's no path to target -> get another target
            {
                SwitchFollowing();
            }
            else agent.SetDestination(current.position);
            // levelMax equals to the highest normalized value power 2, a small number because < 1
            // pass the value to a static var so we can access it from anywhere
            MicLoudness = LevelMax();

            // for distance between Monster and current target
            var distance = (current.position - this.transform.position).magnitude;


            // MouseInput for testing. 
            // Proplem: the same script on more than 1 Object only works for the first object
            // Seems like the 2nd object can't access the microphone
            if (!isFollowingPlayer && (MicLoudness * 2 >= 0.01f || Input.GetMouseButtonDown(0)))
            {
                isFollowingPlayer = true;
                current = player;
                Debug.Log(MicLoudness);
                // Move towards target position
                agent.SetDestination(current.position);
                agent.speed = 1.5f + MicLoudness * 50.0f;
                if (IsInvoking("SwitchFollowing"))
                    CancelInvoke("SwitchFollowing");
                Invoke("SwitchFollowing", 5.0f);
            }
            else if (distance < 1.0f && !isFollowingPlayer)
            {
                new WaitForSecondsRealtime(2);
                ChooseTarget();
            }
            //Debug.Log(playerAngle);

            float playerAngle = Vector3.Angle(playerHeading, this.transform.forward);
            if (playerAngle <= 35.0f && playerDistance <= 8.0f)
            {
                //Debug.Log("Player in angle and range");

                Ray ray = new Ray(this.transform.position, playerHeading);
                Physics.Raycast(ray, out RaycastHit hitinfo);
                Debug.DrawRay(ray.origin, 10 * ray.direction, Color.magenta);
                string name = hitinfo.collider.gameObject.tag;
                bool seen = (name == "Player" && playerAngle < 30);
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
                Debug.Log("Too close, end please!");
                // new WaitForSecondsRealtime(3);
                // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
            }
        }
    }

    private void SwitchFollowing()
    {
        isFollowingPlayer = false;
        ChooseTarget();
        agent.speed = 2;
    }

    private void ChooseTarget()
    {
        int num = Random.Range(0, targets.Count - 1);
        current = targets[num];
        transform.LookAt(target);
        new WaitForSecondsRealtime(1);
        agent.SetDestination(current.position);
        Debug.Log(current.name);
        // Das letzt Element scheint verbuggt zu sein
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

    private void OnCollisionEnter(Collision col)
    {

    }

    // private void MonsterHit()
    // {
    //     CancelInvoke("SwitchFollowing");
    //     enabled = false;    //stops update function from beeing called
    //     new WaitForSecondsRealtime(3);  //wait 3 seconds
    //     enabled = true;
    // }

}