using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleLeg : MonoBehaviour
{
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

        if ((Input.GetKeyDown(KeyCode.V) || Input.GetMouseButtonDown(2)) && canKick) Kick();

    }

    public void DealDamage()
    {
        GameObject attack = Instantiate(attackPrefab, transform.parent);
        attack.GetComponent<DirectionalAttack>().target = attackLocation;
    }

    public void Kick()
    {
        canKick = false;
        Invoke(nameof(SetCanKick), 0.5f);
        
        anim.SetTrigger("WholeKick");
        if (transform.parent.childCount == 2) DealDamage();
        // Invoke(nameof(DealDamage), 0.3f);
    }

    public void SetCanKick() => canKick = true;

    public void Attack()
    {
        anim.SetTrigger("Kick");

        // Invoke(nameof(Rest), 0.5f);
    }
    public void Rest()
    {
        anim.SetTrigger("Rest");
    }

}
