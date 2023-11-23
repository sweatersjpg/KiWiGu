using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmIK : MonoBehaviour
{

    public Transform target;
    public Transform restPos;
    public Transform holdPoint;

    [Space]
    public Transform shoulder;
    public Transform elbow;
    public Transform wrist;

    [Space]
    public Transform head;

    [Space]
    public float maxShoulderDelta = 0.2f;

    Vector3 offset;

    void Start()
    {
        offset = head.localPosition - transform.localPosition;
    }

    private void LateUpdate()
    {
        transform.localPosition = head.localPosition - offset;

        shoulder.localPosition = new();
        shoulder.LookAt(restPos);
        elbow.localEulerAngles = new();

        wrist.eulerAngles = target.eulerAngles;
        Vector3 targetPos = target.position - (holdPoint.position - wrist.position);

        elbow.LookAt(targetPos);
        wrist.eulerAngles = target.eulerAngles;

        Vector3 t = targetPos - wrist.position;
        shoulder.LookAt(elbow.position + t);

        elbow.LookAt(targetPos);

        wrist.eulerAngles = target.eulerAngles;

        shoulder.position += Vector3.ClampMagnitude(target.position - holdPoint.position, maxShoulderDelta);

    }


}
