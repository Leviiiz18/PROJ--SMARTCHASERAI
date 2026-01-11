using UnityEngine;

public class CameraDirector : MonoBehaviour
{
    public GameObject camFollow;
    public GameObject camOrbit;
    public GameObject camWide;
    public GameObject camTop;   // optional (safe even if unused)

    void Start()
    {
        Activate(camFollow);
    }

    void Update()
    {
        // Normal input (works when Game view has focus)
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Activate(camFollow);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            Activate(camOrbit);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            Activate(camWide);

        if (camTop != null && Input.GetKeyDown(KeyCode.Alpha4))
            Activate(camTop);
    }

    // 🔥 FAILSAFE INPUT (works during Recorder / focus loss)
    void OnGUI()
    {
        if (Event.current.type != EventType.KeyDown) return;

        if (Event.current.keyCode == KeyCode.Alpha1)
            Activate(camFollow);

        if (Event.current.keyCode == KeyCode.Alpha2)
            Activate(camOrbit);

        if (Event.current.keyCode == KeyCode.Alpha3)
            Activate(camWide);

        if (camTop != null && Event.current.keyCode == KeyCode.Alpha4)
            Activate(camTop);
    }

    void Activate(GameObject cam)
    {
        if (camFollow != null) camFollow.SetActive(false);
        if (camOrbit != null) camOrbit.SetActive(false);
        if (camWide != null) camWide.SetActive(false);
        if (camTop != null) camTop.SetActive(false);

        cam.SetActive(true);
    }
}
