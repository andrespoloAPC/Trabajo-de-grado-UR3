using UnityEngine;
using UnityEngine.UI; // Para usar Image

public class ParoDeEmergencia : MonoBehaviour
{
    [Header("Interfaz y Estado")]
    [Tooltip("Arrastra aquí el Canvas exclusivo de Paro de Emergencia")]
    public GameObject canvasParoEmergencia;

    [Tooltip("Arrastra aquí el objeto visual de la luz de estado")]
    public Image luzEstado;

    [Header("Control de Trayectorias")]
    [Tooltip("Arrastra aquí el objeto que tiene el script URTeachManager")]
    public URTeachManager gestorTrayectorias; // <-- AÑADIMOS ESTO

    public void ActivarEStop()
    {
        Debug.Log("<color=red>¡BOTÓN DE PARO DE EMERGENCIA PRESIONADO!</color>");

        // 1. Comando de freno de emergencia al URScript (frenado articular rápido)
        string comandoFreno = "stopj(10.0)\n";
        ur_data_processing.UR_Control_Data.comando_personalizado = comandoFreno;
        ur_data_processing.UR_Control_Data.enviar_comando_personalizado = true;

        // 2. Apagamos el joystick
        ur_data_processing.UR_Control_Data.joystick_button_pressed = false;

        // 3. DETENER LA TRAYECTORIA EN CURSO (NUEVO)
        if (gestorTrayectorias != null)
        {
            // Le decimos al script de trayectorias que deje de avanzar al siguiente punto
            gestorTrayectorias.CancelarLogicaTrayectoria();
        }

        // 4. Encendemos el Canvas exclusivo y cambiamos la luz
        if (canvasParoEmergencia != null)
        {
            canvasParoEmergencia.SetActive(true);
        }

        if (luzEstado != null)
        {
            luzEstado.color = Color.red;
        }

        // 5. Corte de red
        Invoke("DesconectarSockets", 0.2f);
    }

    private void DesconectarSockets()
    {
        ur_data_processing.GlobalVariables_Main_Control.connect = false;
        ur_data_processing.GlobalVariables_Main_Control.disconnect = true;
        Debug.Log("Sockets cerrados por Paro de Emergencia.");
    }

    // Vincula esta función al botón "OK" de tu Canvas de Paro de Emergencia
    public void PresionarBotonOK()
    {
        if (canvasParoEmergencia != null)
        {
            canvasParoEmergencia.SetActive(false);
        }
        ur_data_processing.GlobalVariables_Main_Control.disconnect = false;

        // NUEVO: Ponemos la luz en amarillo indicando "Standby / Esperando Conexión"
        if (luzEstado != null)
        {
            luzEstado.color = Color.yellow;
        }

        Debug.Log("Sistema liberado tras E-Stop. Listo para reconectar.");
    }
}