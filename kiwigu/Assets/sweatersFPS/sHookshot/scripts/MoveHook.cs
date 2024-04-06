using UnityEngine;

public class MoveHook : MonoBehaviour
{
    // Start is called before the first frame update

    [HideInInspector] public ThrowHook home;

    public GameObject perfectHookFXprefab;
    public GameObject perfectHookCircleFXprefab;

    GameObject fx;

    public LineRenderer chain;

    //public float startingSpeed = 8;
    public float hookRange = 5;
    public float maxHookRange = 10;
    public float trackingAcceleration = 6;
    public float gravity = 10;
    public float deceleration = 32;

    public float catchDistance = 0.2f;

    // bool canCatch = false;

    GunInfo caughtGun;
    Ammunition caughtGunAmmo;

    [HideInInspector] public HookTarget hookTarget;
    public float playerDistance = 4;
    public float distToHook = 0;

    [SerializeField] float pullForce;

    bool headingBack = false;

    float speed;
    [HideInInspector] public Vector3 velocity;
    Vector3 G = new();

    float deltaTime = 0;

    Vector3 pPosition; // past position

    float chainPointTimer;
    public float chainSegmentSize = 0.5f;

    public MoveHook parentHook;
    Vector3 offsetFromOtherHook;

    public MoveHook childHook;

    bool hasKicked = false;

    void Start()
    {
        float startingSpeed = Mathf.Sqrt(2 * trackingAcceleration * hookRange / 2);

        velocity = transform.forward * startingSpeed;

        //Vector3 v = sweatersController.instance.velocity;
        //v.y = 0;
        //velocity += v;
        velocity += sweatersController.instance.GetRelativity();

        speed = -velocity.magnitude;

        // Collider[] hits = Physics.OverlapSphere(transform.position, 3, LayerMask.GetMask("HookShot"));

        // foreach (Collider hit in hits) Debug.Log(hit.name);

        gameObject.name = "HookShotNormal";

        GameObject[] objs = GameObject.FindGameObjectsWithTag("Hook");

        GameObject otherHook = null;

        foreach (GameObject obj in objs)
        {
            if (obj != gameObject 
                && Vector3.Distance(obj.transform.position, gameObject.transform.position) < 4
                && obj.name == "HookShotNormal") otherHook = obj;
        }

        if (otherHook != null)
        {
            gameObject.name = "HookShotFollow";
            // Debug.Log("I am " + gameObject.name + " and I found " + otherHook.name);
            parentHook = otherHook.transform.GetComponent<MoveHook>();

            sFishing s = GetComponent<sFishing>();
            if (s) Destroy(s);

            if (parentHook.headingBack)
            {
                parentHook = null;
                return;
            }

            offsetFromOtherHook = parentHook.transform.position - transform.position;
            parentHook.childHook = this;
            
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (PauseSystem.paused)
        {
            deltaTime = 0;
            return;
        }
        else deltaTime = Time.deltaTime;

        if(parentHook)
        {
            transform.position = parentHook.transform.position - offsetFromOtherHook/2;

            UpdateChain();

            headingBack = parentHook.headingBack;
            velocity = parentHook.velocity;
            speed = parentHook.speed;

            // if (parentHook.hookTarget) MeleLeg.instance.SetAttacking();

            return;
            // if(speed > 0) return;
        }

        if (isFishing)
        {
            UpdateChain();
            return;
        }

        pPosition = transform.position;

        Vector3 heading = home.transform.position - transform.position;

        if (childHook && hookTarget && (!hookTarget.tether || Mathf.Round(hookTarget.resistance) == 69) && heading.magnitude < 6 && !hasKicked)
        {
            MeleLeg.instance.Kick();
            hasKicked = true;
        }

        //if(canCatch && heading.magnitude < catchDistance)
        //{
        //    home.CatchHook(caughtGun);

        //    Destroy(gameObject);
        //    return;
        //} else if(heading.magnitude > catchDistance) {
        //    canCatch = true;
        //}
        if (headingBack && heading.magnitude < catchDistance && !hookTarget)
        {
            home.CatchHook(caughtGun, caughtGunAmmo);

            Destroy(gameObject);
            return;
        }

        if (speed >= 0)
        {
            velocity = heading.normalized;
            if (!headingBack)
            {
                headingBack = true;
                // home.PullBack();
                maxHookRange = heading.magnitude;
            }
        }

        if (hookTarget)
        {
            if (!hookTarget.tether) hookTarget.resistance -= deltaTime;

            if (hookTarget.resistance > 0)
            {
                UpdateChain();

                //if (hookTarget.tether) PullTowards(heading);
                //else Grapple(heading);
                
                if(!hookTarget.swing) PullTowards(heading, pullForce);
                else
                {
                    if (distToHook > 4) distToHook = Mathf.Lerp(distToHook, 4, deltaTime * 2);

                    //if (distToHook > 8)
                    //{
                    //    PullTowards(heading, pullForce / 4);
                    //    distToHook = Mathf.Min(heading.magnitude + 1, distToHook);
                    //}
                    //else

                    Grapple(heading);
                }

                return;
            }
            else if (!hookTarget.tether)
            {
                DamageEnemy(transform);
                transform.parent = null;

                TakeHookTarget();
                hookTarget = null;
                // sweatersController.instance.isEncombered = false;
            }
            else
            {
                ReturnHookTarget();
            }
        }

        if (headingBack) speed += trackingAcceleration * deltaTime * 0.5f;
        else if (heading.magnitude > hookRange)
        {
            if (speed < -0.5f) speed += deceleration * deltaTime * 0.5f;
            G += 0.5f * deltaTime * gravity * Vector3.down;
        }

        velocity = velocity.normalized * Mathf.Abs(speed);

        if (headingBack)
        {
            maxHookRange -= Mathf.Min(speed * deltaTime, maxHookRange);
        }

        // restrict distance
        if (heading.magnitude > maxHookRange)
        {
            transform.position = home.transform.position - heading.normalized * maxHookRange;

            Vector3 normal = heading.normalized;
            G -= Vector3.Project(G, normal);
        }

        Vector3 vel = G;
        if (!headingBack) vel += velocity;

        transform.position += vel * deltaTime;

        if (headingBack) speed += trackingAcceleration * deltaTime * 0.5f;
        else if (heading.magnitude > hookRange)
        {
            if (speed < -0.5f) speed += deceleration * deltaTime * 0.5f;
            G += 0.5f * deltaTime * gravity * Vector3.down;
        }

        DoPhysics();
        UpdateChain();
    }

    void DamageEnemy(Transform t)
    {
        EnemyHitBox e = t.GetComponentInParent<EnemyHitBox>();

        if (e)
        {
            Transform rootParent = GetRootParent(e.transform);

            if (rootParent != null)
            {
                var scriptType = System.Type.GetType(e.ReferenceScript);

                if (scriptType != null)
                {
                    var enemyComponent = rootParent.GetComponent(scriptType) as MonoBehaviour;

                    if (enemyComponent != null)
                    {
                        if (rootParent.gameObject.CompareTag("Drone"))
                        {
                            CallMethodSafely(enemyComponent, "TakeDamage", new object[] { 9999, false });
                        }
                        else if (rootParent.gameObject.CompareTag("Enemy"))
                        {
                            CallMethodSafely(enemyComponent, "TakeGun", null);
                        }
                    }
                }
            }
        }
    }

    void CallMethodSafely(MonoBehaviour component, string methodName, object[] parameters)
    {
        var method = component.GetType().GetMethod(methodName);

        if (method != null)
        {
            method.Invoke(component, parameters);
        }
    }

    private Transform GetRootParent(Transform child)
    {
        Transform parent = child.parent;

        while (parent != null)
        {
            child = parent;
            parent = child.parent;
        }

        return child;
    }

    void DoPhysics()
    {
        // raycast from ppos to pos

        if (caughtGun == null && !headingBack) HookGun();

        bool hasHit = Physics.Raycast(pPosition, transform.position - pPosition,
            out RaycastHit hit, (transform.position - pPosition).magnitude,
            ~LayerMask.GetMask("GunHand", "Player", "HookTarget", "TransparentFX", "HookShot"));

        if (hasHit)
        {
            if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                StartFishing();
                return;
            }
            ResolveCollision(hit);
            AddChainSegment(hit.point);
        }
    }

    [HideInInspector] public bool isFishing = false;

    void StartFishing()
    {

        sFishing f = GetComponent<sFishing>();
        if (f)
        {
            f.enabled = true;
            isFishing = true;
        }

        // gameObject.AddComponent<sFishing>();
    }

    void HookGun()
    {
        bool hasHit = Physics.SphereCast(pPosition, 1f, transform.position - pPosition,
            out RaycastHit hit, (transform.position - pPosition).magnitude, LayerMask.GetMask("HookTarget"));

        if (hasHit)
        {

            GameObject target = hit.transform.gameObject;
            HookTarget ht = target.transform.GetComponentInChildren<HookTarget>();

            if (ht && ht.blockSteal)
            {
                if (GetRootParent(target.transform).CompareTag("Enemy"))
                {
                    if (GetRootParent(target.transform).GetComponent<HellfireEnemy>())
                        GetRootParent(target.transform).GetComponent<HellfireEnemy>().TakeDamage(5, false);

                    if (GetRootParent(target.transform).GetComponent<PistolGrunt>())
                        GetRootParent(target.transform).GetComponent<PistolGrunt>().TakeDamage(5, false);
                }
                return;
            }

            if (!ht)
            {
                caughtGun = target.transform.GetComponent<ThrownGun>().info;
                caughtGunAmmo = target.transform.GetComponent<ThrownGun>().ammo; // transfer ammo info
            }
            else
            {
                ht.resistance -= deltaTime;
                if (ht.resistance > 0)
                {
                    GlobalAudioManager.instance.PlayHook(transform, "Hit");

                    hookTarget = ht;
                    Pullback(true);

                    Destroy(fx);
                    fx = Instantiate(perfectHookFXprefab, transform);

                    ht.gameObject.layer = LayerMask.NameToLayer("GunHand");

                    // sweatersController.instance.isEncombered = true;
                    distToHook = (sweatersController.instance.transform.position - ht.transform.position).magnitude;
                    transform.parent = ht.transform;
                    transform.localPosition = new();

                    return; // don't take gun
                }

                target = ht.gameObject;
                caughtGun = ht.info;
                caughtGunAmmo = new Ammunition(ht.info.capacity); // max capacity
            }

            target.transform.parent = transform;
            target.transform.localPosition = new();
            target.layer = LayerMask.NameToLayer("GunHand");

            if (target.transform.GetComponent<Rigidbody>() != null)
            {
                Destroy(target.transform.GetComponent<PhysicsHit>());
                Destroy(target.transform.GetComponent<Rigidbody>());
            }

            Pullback();
        }
    }

    public void TakeThrownGun(GameObject target)
    {
        caughtGun = target.transform.GetComponent<ThrownGun>().info;
        caughtGunAmmo = target.transform.GetComponent<ThrownGun>().ammo;

        target.transform.parent = transform;
        target.transform.localPosition = new();
        target.layer = LayerMask.NameToLayer("GunHand");

        if (target.transform.GetComponent<Rigidbody>() != null)
        {
            Destroy(target.transform.GetComponent<PhysicsHit>());
            Destroy(target.transform.GetComponent<Rigidbody>());
        }

        Pullback();
    }

    void TakeHookTarget()
    {
        if (!hookTarget.info) return;

        GlobalAudioManager.instance.PlayHook(transform, "Whip Back");

        GameObject target = hookTarget.gameObject;

        caughtGun = hookTarget.info;
        caughtGunAmmo = new Ammunition(caughtGun.capacity);

        target.transform.parent = transform;
        target.transform.localPosition = new();
        target.transform.gameObject.layer = LayerMask.NameToLayer("GunHand");

        if (target.transform.GetComponent<Rigidbody>() != null)
        {
            Destroy(target.transform.GetComponent<PhysicsHit>());
            Destroy(target.transform.GetComponent<Rigidbody>());
        }
    }

    void ResolveCollision(RaycastHit hit)
    {
        GlobalAudioManager.instance.PlayHook(transform, "Bounce");

        if (hit.transform.gameObject.CompareTag("Shield") && GetRootParent(hit.transform).GetComponent<HellfireEnemy>())
        {
            Transform parent = GetRootParent(hit.transform);
            parent.GetComponent<HellfireEnemy>().HookBlock();
        }

        if (hit.transform.gameObject.CompareTag("RigidTarget"))
        {
            hit.transform.gameObject.GetComponent<PhysicsHit>().Hit(hit.point, velocity);
        }

        if (headingBack) return;

        speed /= 2;
        G = new();
        Invoke(nameof(Pullback), 0.2f);

        transform.position = hit.point;

        Vector3 d = velocity;
        Vector3 n = hit.normal;

        Vector3 r = d - 2 * Vector3.Dot(d, n) * n;

        velocity = r;
    }

    void UpdateChain()
    {

        //chainPointTimer += deltaTime;
        //if(!headingBack && chainPointTimer > timeBetweenChainNodes)
        //{
        //    chainPointTimer = 0;
        //    AddChainSegment(transform.position + Random.insideUnitSphere * 0.1f - velocity.normalized * 0.1f);
        //}
        if (!headingBack && (chain.GetPosition(0) - chain.GetPosition(1)).magnitude > chainSegmentSize)
        {
            AddChainSegment(transform.position + Random.insideUnitSphere * 0.1f - velocity.normalized * 0.1f);
        }

        chain.SetPosition(chain.positionCount - 1, sweatersController.instance.playerCamera.transform.position - sweatersController.instance.playerCamera.transform.up);
        chain.SetPosition(chain.positionCount - 2, home.transform.position + home.transform.up * 0.12f);
        chain.SetPosition(0, transform.position);

        for (int i = 1; i < chain.positionCount - 2; i++)
        {
            Vector3 p = chain.GetPosition(i);

            Vector3 r = home.transform.position;
            Vector3 h = transform.position;

            Vector3 d = r - h;
            //Vector3 v = Vector3.Project(p - h, r - h) + h;
            Vector3 v = (d.magnitude / chain.positionCount) * i * d.normalized + h;

            p += 50 * deltaTime * ((v - p) / (headingBack ? 2 : 4));

            chain.SetPosition(i, p);
        }
    }

    void AddChainSegment(Vector3 pos)
    {
        chain.positionCount++;

        // shift positions down
        for (int i = chain.positionCount - 3; i >= 1; i--)
        {
            chain.SetPosition(i + 1, chain.GetPosition(i));
        }

        chain.SetPosition(1, pos);
    }

    void ReturnHookTarget()
    {
        transform.parent = null;
        hookTarget.resistance = hookTarget.maxResistance;
        hookTarget.gameObject.layer = LayerMask.NameToLayer("HookTarget");
        hookTarget = null;
    }

    public void Pullback() => Pullback(false);

    public void Pullback(bool withForce)
    {
        
        speed = 0;
        G = new();

        if (!withForce) return;

        sweatersController player = sweatersController.instance;

        if (!player.isGrounded)
        {
            if (!hookTarget.tether) player.velocity.y = 0;
            return;
        }

        Vector3 toPlayer = player.transform.position - transform.position;

        player.velocity.y = 0;
        player.velocity -= toPlayer.normalized * 8;
        // home.PullBack();
    }

    public void PullbackWithForce(float force, float vScale)
    {
        isFishing = false;
        sFishing s = GetComponent<sFishing>();
        if (s)
        {
            if(s.enabled) s.BeforeDestroy();
            Destroy(s);
        }

        if (hookTarget)
        {
            float t = hookTarget.maxResistance - hookTarget.resistance;

            if (t < 0.4) // perfect hook
            {
                hookTarget.resistance = 0;
                if (force > 0) LaunchPlayer(force, vScale);

                Destroy(fx);
                fx = Instantiate(perfectHookCircleFXprefab, transform);
            }
            else
            {
                hookTarget.resistance = 0;
            }

            if(fx) fx.transform.parent = null;

            sweatersController player = sweatersController.instance;

            if (hookTarget.tether) player.maxSpeed = player.airSpeed * 0.75f;
            else player.maxSpeed = player.airSpeed * 0.5f;
            player.velocity = Vector3.ClampMagnitude(player.velocity, player.maxSpeed);

        }
        speed = 0;
        G = new();

        if(parentHook)
        {
            if (parentHook.hookTarget && parentHook.hookTarget.tether) parentHook.ReturnHookTarget();
            parentHook.childHook = null;
            parentHook = null;
        }
        if(childHook)
        {
            
            childHook.parentHook = null;
            childHook = null;
        }

        

        home.PullBack();
    }

    void Grapple(Vector3 heading)
    {
        sweatersController player = sweatersController.instance;

        Vector3 toPlayer = player.transform.position - transform.position;

        player.isGrappling = true;


        //distToHook = Mathf.Min(toPlayer.magnitude, distToHook);

        //float distance = Mathf.Max(distToHook, playerDistance);
        //distance = Mathf.Min(distance, maxHookRange);

        //speed += trackingAcceleration * deltaTime * 0.5f;
        //maxHookRange -= Mathf.Min(speed * deltaTime, maxHookRange);
        //speed += trackingAcceleration * deltaTime * 0.5f;

        distToHook = Mathf.Min(heading.magnitude + 1, distToHook);

        // if (toPlayer.y > 1) return; // if above grapple, return


        //float distance = Mathf.Max(maxHookRange, playerDistance);
        float distance = Mathf.Max(distToHook, playerDistance);

        // grapple hook effect
        if (heading.magnitude > distance)
        {
            Vector3 target = transform.position + heading.normalized * distance;
            target += (player.transform.position - home.transform.position);

            player.transform.position = target;
            // Vector3 force = deltaTime * (target - player.transform.position);

            Vector3 normal = -toPlayer.normalized;
            player.velocity -= Vector3.Project(player.velocity, normal);

            // player.velocity += force;
        }
    }

    bool isGrapplnig = false;

    void PullTowards(Vector3 heading, float pullForce)
    {
        if (!hookTarget.tether && childHook == null) return;


        if(!isGrapplnig)
            GlobalAudioManager.instance.PlayHook(transform, "Launch");

        sweatersController player = sweatersController.instance;

        // Vector3 toPlayer = player.transform.position - transform.position;

        if (heading.magnitude < 0.5)
        {
            PullbackWithForce(0, 1);
            isGrapplnig = false;
            return;
        }

        if (isGrapplnig && Vector3.Dot(-heading.normalized, player.velocity.normalized) <= 0)
        {
            isGrapplnig = false;
            PullbackWithForce(0, 1);
            return;
        }

        if(!isGrapplnig)
        {
            player.velocity = -heading.normalized * player.velocity.magnitude;
        }


        isGrapplnig = true;

        // float t = hookTarget.maxResistance - hookTarget.resistance;

        //if (t < 0.4 && !hookTarget.tether) // perfect hook
        //{
        //    return;
        //}

        // -- direct approach --
        // player.velocity = -heading.normalized * (player.velocity.magnitude + Time.deltaTime * pullForce);

        // -- force approach --
        player.velocity -= Time.deltaTime * pullForce * heading.normalized;

        // player.velocity = Vector3.ClampMagnitude(player.velocity, player.maxSpeed);

        // player.maxSpeed = player.velocity.magnitude;
        // player.velocity += -heading.normalized * Time.deltaTime * pullForce;

        player.ignoreGravity = true;
        player.isGrappling = true;

        // player.velocity += -heading.normalized * Time.deltaTime * pullForce;
    }

    public void LaunchPlayer(float force, float vScale)
    {
        sweatersController player = sweatersController.instance;

        //Vector3 v = (player.transform.position - transform.position).normalized;

        //player.velocity.y = Mathf.Min(Mathf.Abs(v.y) + 0.5f, 1) * force;
        //player.maxSpeed += 5;

        Vector3 heading = home.transform.position - transform.position;

        if (heading.y > 0) return; // if higher than grapple dont give boost

        Vector3 normal = -(heading).normalized;

        Vector3 v = (player.velocity - Vector3.Project(player.velocity, normal)).normalized;

        float a = 0.5f;

        // v.y = Mathf.Abs(v.y);
        if (v.y < 0) v = normal;
        else if (v.y < a)
        {
            v.y = a * (a / v.y);
        }

        Vector3 forward = player.playerCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();

        //v.x = forward.x;
        //v.z = forward.z;

        v.Normalize();

        float product = Vector3.Dot(new Vector3(v.x, 0, v.z).normalized, forward);

        v *= (player.velocity.magnitude * vScale + force) * product;

        player.maxSpeed = v.magnitude;

        player.velocity = v;

    }
}
