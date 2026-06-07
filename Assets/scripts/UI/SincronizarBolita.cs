using UnityEngine;
using System.Text;
using System.Globalization;

public class ControlPorPuntos : MonoBehaviour
{
    public Transform targetBolita;
    public Transform baseRobot; // Tu objeto "UR3"

    [Header("Parámetros de Movimiento")]
    public float aceleracion = 1.2f;
    public float velocidad = 0.25f;

    // ESTA FUNCIÓN SE LLAMA AL PRESIONAR TU NUEVO BOTÓN "ACTUAR"
    public void EnviarRobotAlPunto()
    {
        if (!ur_data_processing.UR_Stream_Data.is_alive)
        {
            Debug.LogWarning("Robot no conectado");
            return;
        }

        // 1. Obtener posición de la bolita respecto a la base UR3
        Vector3 posRelativa = baseRobot.InverseTransformPoint(targetBolita.position);

        // 2. Traducción de coordenadas Unity -> Robot
        double tx = posRelativa.z;
        double ty = -posRelativa.x;
        double tz = posRelativa.y;

        // 3. Mantener la rotación actual para que no dé error de cinemática
        double rx = ur_data_processing.UR_Stream_Data.C_Orientation[0];
        double ry = ur_data_processing.UR_Stream_Data.C_Orientation[1];
        double rz = ur_data_processing.UR_Stream_Data.C_Orientation[2];

        // 4. Construir comando MOVEL (p[x, y, z, rx, ry, rz], a, v)
        // La "p" antes de los corchetes es vital, indica que es una POSE.
        string comando = string.Format(CultureInfo.InvariantCulture,
            "movel(p[{0:F4}, {1:F4}, {2:F4}, {3:F4}, {4:F4}, {5:F4}], a={6:F2}, v={7:F2})\n",
            tx, ty, tz, rx, ry, rz, aceleracion, velocidad);

        // 5. Enviar el comando
        byte[] payload = Encoding.UTF8.GetBytes(comando);
        ur_data_processing.UR_Control_Data.command = payload;

        // Activamos el envío un momento para que el buffer lo tome
        StartCoroutine(EnviarPulsoControl());
    }

    System.Collections.IEnumerator EnviarPulsoControl()
    {
        ur_data_processing.UR_Control_Data.joystick_button_pressed = true;
        yield return new WaitForSeconds(0.2f);
        ur_data_processing.UR_Control_Data.joystick_button_pressed = false;
        Debug.Log("Comando movel enviado al UR3");
    }
}