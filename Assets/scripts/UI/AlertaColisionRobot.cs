using UnityEngine;
using TMPro;
using UnityEngine.UI; // Importante si la luz es una imagen de UI

public class AlertaColisionRobot : MonoBehaviour
{
    [Header("Interfaz de Usuario (Canvas)")]
    public GameObject canvasAlertaUI;

    [Header("Conexión Externa")]
    [Tooltip("Arrastra aquí el objeto que tiene el script ConectorFinalUR")]
    public ConectorFinalUR scriptConector; // <-- AÑADIMOS ESTO

    [Header("Configuración")]
    public string tagObstaculo = "Obstaculo";

    private void Start()
    {
        if (canvasAlertaUI != null) canvasAlertaUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagObstaculo))
        {
            Debug.Log("<color=red>¡ZONA DE SEGURIDAD VULNERADA! Desconectando robot...</color>");

            if (canvasAlertaUI != null) canvasAlertaUI.SetActive(true);

            ur_data_processing.GlobalVariables_Main_Control.connect = false;
            ur_data_processing.GlobalVariables_Main_Control.disconnect = true;

            // Usamos la referencia que creamos para cambiar el color
            if (scriptConector != null && scriptConector.luzEstado != null)
            {
                scriptConector.luzEstado.color = Color.red;
            }
        }
    }

    public void PresionarBotonOK()
    {
        if (canvasAlertaUI != null) canvasAlertaUI.SetActive(false);
        ur_data_processing.GlobalVariables_Main_Control.disconnect = false;

        // NUEVO: Ponemos la luz en amarillo
        if (scriptConector != null && scriptConector.luzEstado != null)
        {
            scriptConector.luzEstado.color = Color.yellow;
        }

        Debug.Log("Alerta aceptada por el usuario. Listo para reconectar.");
    }
}