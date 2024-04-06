using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleLeg : MonoBehaviour
{
    // I have to stop making everything public static but its so convinient :-)
    public static MeleLeg instance;
    
    Quaternion attackRotation;
    Vector3 attackPosition;

    Quaternion restRotation;
    Vector3 restPosition;

    public float attackSpeed = 1;
    public float restSpeed = 1;

    public Transform attackLocation;
    public GameObject attackPrefab;

    Animator anim;

    bool attacking = false;

    public bool triggerDamage = false;
    bool canKick = true;

    [SerializeField] float maxRecoil = 45;
    float recoil = 0;
    [SerializeField] float recoilSpeed = 2;

    public WeaponCameraFX camFX;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        attackRotation = transform.localRotation;
        attackPosition = transform.localPosition;

        restPosition = new(0,0,-1);
        restRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, 0.5f);

        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (attacking) AnimateToAttack();
        //else AnimateToRest();

        if ((Input.GetButtonDown("Kick")) && canKick && transform.parent.parent.childCount == 2) Kick();

        if (canKick) recoil = Mathf.Lerp(recoil, 0, Time.deltaTime * recoilSpeed);
        else recoil = Mathf.Lerp(recoil, maxRecoil, Time.deltaTime * recoilSpeed);

        if (recoil > 0) camFX.RequestRecoil(recoil);
    }

    public void DealDamage()
    {
        GameObject attack = Instantiate(attackPrefab, transform.parent);
        attack.GetComponent<DirectionalAttack>().target = attackLocation;
    }

    public void Kick()
    {
        canKick = false;
        Invoke(nameof(SetCanKick), 1f);
        
        anim.SetTrigger("WholeKick");
        Invoke(nameof(DealDamage), 0.2f);
        // Invoke(nameof(DealDamage), 0.3f);
    }

    public void DelayKick(float delay)
    {
        Invoke(nameof(Kick), delay);
    }

    public void SetCanKick() => canKick = true;

    public void SetAttacking()
    {
        anim.SetBool("IsKicking", true);
        CancelInvoke(nameof(SetResting));
        Invoke(nameof(SetResting), 0.5f);
    }
    public void SetResting()
    {
        anim.SetBool("IsKicking", false);
    }

}
