using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float groundY = 1f;

    [Header("Mode Control")]
    public bool freezeDuringTraining = true;
    public bool demoMode = false;

    bool isCaptured = false;

    float minX, maxX, minZ, maxZ;

    Terrain terrain;
    Vector3 terrainPos, terrainSize;

    Animator anim;
    Vector3 lastPos;

    void Start()
    {
        DetectTerrainBounds();
        anim = GetComponentInChildren<Animator>();
        lastPos = transform.position;
    }

    void Update()
    {
        if (freezeDuringTraining && !demoMode) return;
        if (isCaptured) return;

        if (demoMode)
            HandleKeyboardMovement();

        ApplyConstraints();
        UpdateAnimation();
    }

    void HandleKeyboardMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, 0f, v);

        if (move.sqrMagnitude > 0.001f)
        {
            move.Normalize();
            transform.Translate(
                move * speed * Time.unscaledDeltaTime, // 🔥 FIX
                Space.World
            );
        }
    }

    void UpdateAnimation()
    {
        if (anim == null) return;

        float speedValue =
            (transform.position - lastPos).magnitude / Mathf.Max(Time.unscaledDeltaTime, 0.001f);

        anim.SetFloat("Speed", speedValue);
        lastPos = transform.position;
    }

    void DetectTerrainBounds()
    {
        terrain = Terrain.activeTerrain;

        if (terrain != null)
        {
            terrainPos = terrain.transform.position;
            terrainSize = terrain.terrainData.size;

            minX = terrainPos.x;
            maxX = terrainPos.x + terrainSize.x;
            minZ = terrainPos.z;
            maxZ = terrainPos.z + terrainSize.z;
        }
        else
        {
            minX = -50f;
            maxX = 50f;
            minZ = -50f;
            maxZ = 50f;
        }
    }

    void ApplyConstraints()
    {
        Vector3 pos = transform.position;
        pos.y = groundY;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    public void OnCaptured()
    {
        isCaptured = true;
    }

    public void ResetTarget(Vector3 startPos)
    {
        transform.position = startPos;
        lastPos = startPos;
        isCaptured = false;
    }
}
