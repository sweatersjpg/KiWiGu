using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShield : MonoBehaviour
{
    private void Update()
    {
        transform.LookAt(sweatersController.instance.transform.position + Vector3.up * 1.5f);
        transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
    }
}
