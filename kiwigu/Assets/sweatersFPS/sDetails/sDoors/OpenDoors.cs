using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoors : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] float openWidth;
    float closedWidth;

    [SerializeField] float speed = 1;

    [Space]
    [SerializeField] Transform left;
    [SerializeField] Transform right;

    [Space]
    [SerializeField] GameObject trigger;

    void Start()
    {
        closedWidth = left.localPosition.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(trigger.activeSelf)
        {
            left.localPosition = Vector3.Lerp(left.localPosition, left.localPosition.normalized * openWidth, 
                Time.deltaTime * speed);
            right.localPosition = Vector3.Lerp(right.localPosition, right.localPosition.normalized * openWidth,
                Time.deltaTime * speed);
        } else
        {
            left.localPosition = Vector3.Lerp(left.localPosition, left.localPosition.normalized * closedWidth, 
                Time.deltaTime * speed);
            right.localPosition = Vector3.Lerp(right.localPosition, right.localPosition.normalized * closedWidth,
                Time.deltaTime * speed);
        }

    }
}
