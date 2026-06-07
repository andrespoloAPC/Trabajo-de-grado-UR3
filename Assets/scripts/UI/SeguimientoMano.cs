using UnityEngine;
using System.Text;
using System.Globalization;

public class SeguimientoMano : MonoBehaviour
{
    public Transform targetBolita;
    public Transform baseRobot; // También arrastras el objeto "UR3" aquí
    public float velocidadSeguimiento = 1.5f;

    void Update()
    {
        if (ur_data_processing.UR_Stream_Data.is_alive)
        {
            EnviarComandoAlReal();
        }
    }

    void EnviarComandoAlReal()
    {
        // Convertimos la posición de la bola a coordenadas locales del objeto UR3
        Vector3 posRel = baseRobot.InverseTransformPoint(targetBolita.position);

        // Ajuste de ejes para UR3: Unity(Z) -> Robot(X), Unity(-X) -> Robot(Y), Unity(Y) -> Robot(Z)
        double tx = posRel.z;
        double ty = -posRel.x;
        double tz = posRel.y;

        // Calculamos la velocidad necesaria para llegar al punto
        double vx = (tx - ur_data_processing.UR_Stream_Data.C_Position[0]) * velocidadSeguimiento;
        double vy = (ty - ur_data_processing.UR_Stream_Data.C_Position[1]) * velocidadSeguimiento;
        double vz = (tz - ur_data_processing.UR_Stream_Data.C_Position[2]) * velocidadSeguimiento;

        // Comando speedl (velocidad lineal)
        string comando = string.Format(CultureInfo.InvariantCulture,
            "speedl([{0:F4},{1:F4},{2:F4},0,0,0], a=1.2, t=0.1)\n",
            vx, vy, vz);

        ur_data_processing.UR_Control_Data.command = Encoding.UTF8.GetBytes(comando);
        ur_data_processing.UR_Control_Data.joystick_button_pressed = true;
    }
}