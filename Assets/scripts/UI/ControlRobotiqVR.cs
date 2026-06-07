using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ControlRobotiqVariable : MonoBehaviour
{
    public void AccionarGripper(bool cerrar)
    {
        // Recuperamos la IP de tu telemetría
        string ipRobot = ur_data_processing.UR_Stream_Data.ip_address;

        if (string.IsNullOrEmpty(ipRobot))
        {
            Debug.LogWarning("IP del robot no encontrada.");
            return;
        }

        // 1 será CERRAR, 2 será ABRIR
        int valorComando = cerrar ? 1 : 2;

        // Inyectamos el valor en la variable global del robot
        string comandoURScript = $"sec set_var():\n  global comando_vr = {valorComando}\nend\n";

        // Lo enviamos en segundo plano para no quitarle ni 1 FPS a las Meta Quest
        System.Threading.Tasks.Task.Run(() => EnviarComandoTCP(ipRobot, comandoURScript));
    }

    private void EnviarComandoTCP(string ip, string script)
    {
        try
        {
            // Disparamos por el 30003 (Canal silencioso que no interrumpe el movimiento)
            using (TcpClient client = new TcpClient(ip, 30003))
            {
                client.NoDelay = true; // Evitamos latencias de red
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.ASCII.GetBytes(script);
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
            }
            Debug.Log($"<color=green>Señal enviada a comando_vr</color>");
        }
        catch (Exception e)
        {
            Debug.LogError("Error comunicando con el robot: " + e.Message);
        }
    }
}