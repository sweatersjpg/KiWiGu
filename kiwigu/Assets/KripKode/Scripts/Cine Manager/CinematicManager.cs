using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicManager : MonoBehaviour
{
    //[HideInInspector] // for debug right now - change later
    public bool inCinemaMode;

    [Header("Cinema Menu")]
    [Range(1, 10)] public int amountOfCameras;
    [SerializeField] Camera cameraPrefab;
    [SerializeField] float timePerCamera;


    // Non-Editor Values
    public List<CinematicPoint> cinematicPoints = new List<CinematicPoint>();
    Camera[] cameraList;
    Camera mainCamera;
    sweatersController playerController;
    ScreenSystem pauseMenu;

    bool initialized;

    private void Awake()
    {
        pauseMenu = FindObjectOfType<ScreenSystem>();
        playerController = FindObjectOfType<sweatersController>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (inCinemaMode)
        {
            InitializeCinematic();
        }
    }
    void InitializeCinematic()
    {
        if (initialized)
            return;

        for (int i = 0; i < amountOfCameras; i++)
        {
            GameObject cinematicPointGO = new GameObject("CinematicPoint " + i);
            cinematicPointGO.transform.parent = transform;

            CinematicPoint cinematicPoint = cinematicPointGO.AddComponent<CinematicPoint>();
            cinematicPoints.Add(cinematicPoint);

            Vector3[] pointsArray = new Vector3[]
            {
                new Vector3(Random.Range(0, 5), Random.Range(0, 5), Random.Range(0, 5)),
                new Vector3(Random.Range(0, 5), Random.Range(0, 5), Random.Range(0, 5)),
            };

            cinematicPoint.points = pointsArray;
        }

        cameraList = new Camera[amountOfCameras];

        for (int i = 0; i < amountOfCameras; i++)
        {
            Camera newCamera = Instantiate(cameraPrefab, transform.position, transform.rotation);
            newCamera.transform.parent = transform;
            cameraList[i] = newCamera;
        }

        mainCamera.gameObject.GetComponent<AudioListener>().enabled = false;
        playerController.enabled = false;
        mainCamera.enabled = false;
        pauseMenu.gameObject.SetActive(false);
        CallCinematic(timePerCamera);
        initialized = true;
    }

    void DisableCinematic()
    {
        foreach (Camera cams in cameraList)
        {
            cams.enabled = false;
        }

        mainCamera.gameObject.GetComponent<AudioListener>().enabled = true;
        mainCamera.enabled = true;
        playerController.enabled = true;

        pauseMenu.gameObject.SetActive(true);

        initialized = false;
        inCinemaMode = false;
    }

    public void CallCinematic(float timeFirstCamera)
    {
        StartCoroutine(CinematicTransition(timeFirstCamera));
    }

    IEnumerator CinematicTransition(float timeFirstCamera)
    {
        int camIndex = 0; // Initialize the camera index

        foreach (Camera cineCam in cameraList)
        {
            cineCam.enabled = true;
            cineCam.gameObject.GetComponent<AudioListener>().enabled = true;

            if (camIndex < cinematicPoints.Count)
            {
                CinematicPoint cinematicPoint = cinematicPoints[camIndex];

                for (int i = 0; i < cinematicPoint.points.Length; i++)
                {
                    Vector3 targetPosition = cinematicPoint.points[i];
                    float elapsedTime = 0f;

                    while (Vector3.Distance(cineCam.transform.position, targetPosition) > 0.01f)
                    {
                        cineCam.transform.position = Vector3.Lerp(cineCam.transform.position, targetPosition, elapsedTime / timeFirstCamera * Time.deltaTime);
                        elapsedTime += Time.deltaTime;
                        yield return null;
                    }

                    cineCam.transform.position = targetPosition;

                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                Debug.LogError("no drip bozo");
            }

            cineCam.enabled = false;
            cineCam.gameObject.GetComponent<AudioListener>().enabled = false;

            camIndex++; // Increment the camera index for the next iteration
        }

        DisableCinematic();
    }
}
