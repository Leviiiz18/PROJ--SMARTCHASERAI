using UnityEngine;
using TMPro;
using System.Text;

public class KeyDisplay : MonoBehaviour
{
    public TMP_Text keyText;

    void Update()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("KEYS:");

        if (Input.anyKey)
        {
            if (Input.GetKey(KeyCode.W)) sb.AppendLine("W");
            if (Input.GetKey(KeyCode.A)) sb.AppendLine("A");
            if (Input.GetKey(KeyCode.S)) sb.AppendLine("S");
            if (Input.GetKey(KeyCode.D)) sb.AppendLine("D");

            if (Input.GetKey(KeyCode.Alpha1)) sb.AppendLine("1");
            if (Input.GetKey(KeyCode.Alpha2)) sb.AppendLine("2");
            if (Input.GetKey(KeyCode.Alpha3)) sb.AppendLine("3");
            if (Input.GetKey(KeyCode.Alpha4)) sb.AppendLine("4");

            if (Input.GetKey(KeyCode.Space)) sb.AppendLine("SPACE");
        }

        keyText.text = sb.ToString();
    }
}
