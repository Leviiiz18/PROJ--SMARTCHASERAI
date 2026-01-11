using UnityEngine;
using TMPro;

public class ChaserAgent : MonoBehaviour
{
    public Transform target;
    public float speed = 3f;

    [HideInInspector] public float moveX;
    [HideInInspector] public float moveZ;

    public float groundY = 1f;

    // 🔒 BOUNDARY LIMITS (AUTO FROM TERRAIN)
    float minX;
    float maxX;
    float minZ;
    float maxZ;

    // 🖥 UI
    public TextMeshProUGUI statusText;

    // 🌍 Terrain reference
    Terrain terrain;
    Vector3 terrainPos;
    Vector3 terrainSize;

    void Start()
    {
        // 🔥 AUTO-DETECT TERRAIN SIZE
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
            // Fallback (should never happen)
            minX = -50f;
            maxX = 50f;
            minZ = -50f;
            maxZ = 50f;
        }
    }

    void Update()
    {
        // 🔹 Apply movement from Python
        Vector3 move = new Vector3(moveX, 0f, moveZ);
        transform.Translate(move * speed * Time.deltaTime, Space.World);

        // 🔒 Lock Y and clamp X/Z to FULL terrain
        Vector3 pos = transform.position;
        pos.y = groundY;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    // 🔹 Called by PythonConnector
    public void SetMovement(float x, float z)
    {
        moveX = Mathf.Clamp(x, -1f, 1f);
        moveZ = Mathf.Clamp(z, -1f, 1f);
    }

    // 🔹 Inputs sent to Python (NEAT)
    public Vector2 GetInputs()
    {
        if (target == null) return Vector2.zero;

        float dx = target.position.x - transform.position.x;
        float dz = target.position.z - transform.position.z;

        return new Vector2(dx, dz);
    }

    // 🔥 VISUAL FEEDBACK ONLY
    void OnCollisionEnter(Collision col)
    {
        if (col.transform == target)
        {
            if (statusText != null)
                statusText.text = "CAUGHT";
        }
    }

    // 🔁 CALLED WHEN PYTHON SENDS "RESET"
    public void ResetAgent()
    {
        transform.position = new Vector3(
            Random.Range(minX, maxX),
            groundY,
            Random.Range(minZ, maxZ)
        );

        if (target != null)
        {
            target.position = new Vector3(
                Random.Range(minX, maxX),
                groundY,
                Random.Range(minZ, maxZ)
            );
        }

        moveX = 0f;
        moveZ = 0f;

        if (statusText != null)
            statusText.text = "RESET";
    }
}
