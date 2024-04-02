using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingWire : MonoBehaviour
{

    public AudioClip ziplineLoop;
    AudioSource audioSource;
    
    [SerializeField] int segments = 20;
    // [SerializeField] float drop = 3;

    [SerializeField] float tension = 0.5f;
    [SerializeField] float gravity = 1;
    [SerializeField] float dampener = 0.8f;

    [Space]

    //[SerializeField] Transform ATarget;
    //[SerializeField] Transform BTarget;

    LineRenderer wire;

    float[] velocities;

    [SerializeField] float width = 0.1f;

    public bool IsZipline = false;
    public float zipLineGravity = 5;

    Vector3 grappleVel;
    
    // Start is called before the first frame update
    void Start()
    {
        wire = GetComponent<LineRenderer>();

        // wire.useWorldSpace = ATarget && BTarget;

        for (int i = 0; i < 1000; i++) StepWire();

        wire.startWidth = width;
        wire.endWidth = width;

        if (!IsZipline) return;
        
        AddColliders();

        
    }

    private void Update()
    {
        if(IsZipline) CheckGrappled();
    }

    void CheckGrappled()
    {
        Transform grapple = null;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).childCount == 0) continue;
            grapple = transform.GetChild(i);
            break;
        }

        if (grapple && transform.childCount > 1)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).childCount == 0) Destroy(transform.GetChild(i).gameObject);
            }

            grappleVel = sweatersController.instance.velocity;
            SetupAudio(grapple.gameObject);
        }
        
        if (grapple && transform.childCount == 1)
        {
            DoPhysics(grapple);
        }

        if(!grapple && transform.childCount == 1)
        {
            Destroy(transform.GetChild(0).gameObject);
            AddColliders();
        }
    }

    void SetupAudio(GameObject target)
    {
        audioSource = target.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = GlobalAudioManager.instance.globalMixer.FindMatchingGroups("SFX")[0];
        audioSource.spatialBlend = 1;
        audioSource.clip = ziplineLoop;
        audioSource.volume = 1;
        audioSource.maxDistance = 20;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
        audioSource.dopplerLevel = 1;
        audioSource.loop = true;

        audioSource.Play();
    }

    // Zip line should fall from root (index == wires.positionCount-1) to tail (index == 0)
    // aka each line segment is i > i-1

    void DoPhysics(Transform grapple)
    {
        int currentSegment = FindClosestLine(grapple.localPosition);
        Vector3 line = wire.GetPosition(currentSegment - 1) - wire.GetPosition(currentSegment);
        Vector3 d = line.normalized;

        // Vector2 normal2D = Get2DNormal(d);
        // Vector3 normal = new Vector3(normal2D.x * d.x, d.y, normal2D.x * d.z);

        // grappleVel.y -= d.y * zipLineGravity * Time.deltaTime;
        // grappleVel = d * grappleVel.magnitude;

        grappleVel.y -= zipLineGravity * Time.deltaTime;

        grappleVel = Vector3.Project(grappleVel, d);

        // sweatersController.instance.velocity += grappleVel/2 * Time.deltaTime;

        Debug.DrawLine(grapple.localPosition + transform.position, wire.GetPosition(currentSegment) + transform.position);

        grapple.localPosition += grappleVel * Time.deltaTime;

        if(Vector3.Distance(wire.GetPosition(currentSegment), grapple.localPosition) > line.magnitude)
        {
            MoveHook hook = grapple.GetComponentInChildren<MoveHook>();

            if (hook)
            {
                hook.PullbackWithForce(0, 1);
            }

        }
    }

    Vector2 Get2DNormal(Vector3 d)
    {
        float y = d.y;
        float x = Mathf.Sqrt(d.x*d.x + d.z*d.z);

        return new Vector2(-y, x);
    }

    int FindClosestLine(Vector3 pos)
    {
        int closest = 1;
        
        // skip first index so you can always use i-1
        for (int i = 1; i < wire.positionCount; i++)
        {
            if (Vector3.Distance(wire.GetPosition(i), pos)
                < Vector3.Distance(wire.GetPosition(closest), pos)) closest = i;
        }

        return closest;
    }

    void AddColliders()
    {
        for (int i = 1; i < wire.positionCount - 1; i++)
        {
            float d = Vector3.Distance(wire.GetPosition(i), wire.GetPosition(i - 1));
            
            GameObject grapplePoint = new GameObject("Grapple Point");
            grapplePoint.transform.parent = transform;
            grapplePoint.layer = LayerMask.NameToLayer("HookTarget");

            grapplePoint.transform.localPosition = wire.GetPosition(i);

            SphereCollider sc = grapplePoint.AddComponent<SphereCollider>();
            sc.radius = d / 2;
            sc.isTrigger = true;

            HookTarget ht = grapplePoint.AddComponent<HookTarget>();
            ht.hasView = false;
            ht.tether = true;
            ht.swing = true;
            
        }
    }

    void StepWire()
    {
        int count = wire.positionCount;

        while (wire.positionCount < segments) wire.positionCount++;
        while (wire.positionCount > segments && segments >= 2) wire.positionCount--;

        if (count != wire.positionCount)
        {
            velocities = new float[wire.positionCount];
            for (int i = 0; i < wire.positionCount; i++) velocities[i] = 0;
        }

        //if (ATarget) wire.SetPosition(wire.positionCount - 1, ATarget.position);
        // else 
        wire.SetPosition(wire.positionCount - 1, new Vector3());

        // if (BTarget) wire.SetPosition(0, BTarget.position);

        for (int i = 1; i < wire.positionCount - 1; i++)
        {
            float y = wire.GetPosition(i).y;
            float nHeights = (wire.GetPosition(i - 1).y - y) + (wire.GetPosition(i + 1).y - y);

            velocities[i] += nHeights * tension;
            velocities[i] -= gravity;

            velocities[i] *= dampener;

            Vector3 start = wire.GetPosition(0);
            Vector3 v = wire.GetPosition(wire.positionCount - 1) - start;

            Vector3 newP = start + v.normalized * v.magnitude / (wire.positionCount - 1) * i;
            newP.y = y + velocities[i] * 0.02f;

            // Vector3 p = new Vector3(0, velocities[i] * Time.fixedDeltaTime, 0);

            wire.SetPosition(i, newP);
        }
    }
}
