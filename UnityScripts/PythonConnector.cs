using UnityEngine;
using System.Net.Sockets;
using System.Text;
using TMPro;
using System;

public class PythonConnector : MonoBehaviour
{
    public ChaserAgent chaser;
    public TMP_Text trainingInfoText;
    public NeuralNetworkVisualizer nnVisualizer;

    TcpClient client;
    NetworkStream stream;
    bool connected = false;

    enum TrainingState
    {
        Waiting,
        Training,
        Disconnected
    }

    TrainingState state = TrainingState.Waiting;

    string generation = "-";
    string genome = "-";
    string fitness = "-";
    string distance = "-";

    // 🔥 TCP RECEIVE BUFFER (CRITICAL)
    string receiveBuffer = "";

    void Start()
    {
        UpdateUI();
        ConnectToPython();
    }

    void Update()
    {
        UpdateUI();

        if (!connected || chaser == null)
            return;

        try
        {
            // ================= SEND INPUTS =================
            Vector2 inputs = chaser.GetInputs();

            // 🔥 ADD NEWLINE DELIMITER
            string message = inputs.x + "," + inputs.y + "\n";
            byte[] sendData = Encoding.ASCII.GetBytes(message);
            stream.Write(sendData, 0, sendData.Length);

            // ================= RECEIVE DATA (NON-BLOCKING SAFE) =================
            if (!stream.DataAvailable)
                return;

            byte[] buffer = new byte[1024];
            int bytes = stream.Read(buffer, 0, buffer.Length);

            if (bytes <= 0)
                throw new SocketException();

            receiveBuffer += Encoding.ASCII.GetString(buffer, 0, bytes);

            // ================= PROCESS COMPLETE LINES =================
            while (receiveBuffer.Contains("\n"))
            {
                string line;
                int index = receiveBuffer.IndexOf("\n");
                line = receiveBuffer.Substring(0, index).Trim();
                receiveBuffer = receiveBuffer.Substring(index + 1);

                // ================= RESET HANDLING =================
                if (line.Contains("RESET"))
                {
                    chaser.ResetAgent();
                    continue;
                }

                string[] split = line.Split('|');

                // ================= MOVEMENT =================
                if (split.Length >= 1)
                {
                    string[] move = split[0].Split(',');
                    float moveX = float.Parse(move[0]);
                    float moveZ = float.Parse(move[1]);
                    chaser.SetMovement(moveX, moveZ);

                    // ================= NN VISUALIZER =================
                    if (nnVisualizer != null && split.Length >= 2)
                    {
                        string[] nn = split[1].Split(',');
                        if (nn.Length >= 2)
                        {
                            nnVisualizer.UpdateNetwork(
                                float.Parse(nn[0]),
                                float.Parse(nn[1]),
                                moveX,
                                moveZ
                            );
                        }
                    }
                }

                // ================= TRAINING STATS =================
                if (split.Length >= 3)
                {
                    string[] stats = split[2].Split(',');
                    if (stats.Length >= 4)
                    {
                        generation = stats[0];
                        genome = stats[1];
                        fitness = stats[2];
                        distance = stats[3];
                    }
                }
            }
        }
        catch (Exception)
        {
            state = TrainingState.Disconnected;
            Disconnect();
            Invoke(nameof(ConnectToPython), 2f);
        }
    }

    void ConnectToPython()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 5005);
            stream = client.GetStream();

            // 🔥 PREVENT UNITY FREEZE
            stream.ReadTimeout = 50;

            connected = true;
            state = TrainingState.Training;
        }
        catch
        {
            connected = false;
            state = TrainingState.Waiting;
        }
    }

    void Disconnect()
    {
        connected = false;

        if (stream != null) stream.Close();
        if (client != null) client.Close();

        stream = null;
        client = null;
    }

    void UpdateUI()
    {
        if (trainingInfoText == null) return;

        trainingInfoText.text =
            "GENERATION: " + generation +
            "\nGENOME: " + genome +
            "\nFITNESS: " + fitness +
            "\nDISTANCE: " + distance +
            "\nSTATUS: " + state.ToString().ToUpper();
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
}
