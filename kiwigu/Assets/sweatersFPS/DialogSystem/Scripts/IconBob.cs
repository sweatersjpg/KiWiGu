using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconBob : MonoBehaviour
{
    [SerializeField] float delta = 1;
    [SerializeField] float timeScale = 1;

    Vector3 startPos;
    float t = 0;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.localPosition;
    }

    private void OnEnable()
    {
        t = 0;
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime * timeScale;

        float y = Mathf.Abs(Mathf.Sin(t) * delta);

        transform.localPosition = startPos + new Vector3(0, y, 0);
    }
}
