using UnityEngine;

public class URIndicadorEsfera : MonoBehaviour
{
    [Header("Materiales Holográficos")]
    public Material materialAbiertoRojo;
    public Material materialActivoVerde;

    private Renderer miRenderer;
    private bool estadoAnteriorActivo;

    void Start()
    {
        miRenderer = GetComponent<Renderer>();

        // Asignamos el rojo (abierto) por defecto al iniciar
        if (miRenderer != null && materialAbiertoRojo != null)
        {
            miRenderer.material = materialAbiertoRojo;
            estadoAnteriorActivo = false;
        }
    }

    void Update()
    {
        // Leemos la variable global de tu script de datos
        bool pinzaEstaActiva = ur_data_processing.UR_Control_Data.pinza_esta_cerrada;

        // Solo cambiamos el material si hubo un cambio de estado
        if (pinzaEstaActiva != estadoAnteriorActivo)
        {
            if (pinzaEstaActiva == true) // Si la pinza está activa/cerrada
            {
                miRenderer.material = materialActivoVerde;
            }
            else // Si la pinza está abierta
            {
                miRenderer.material = materialAbiertoRojo;
            }

            // Guardamos el estado para no repetirlo infinitamente
            estadoAnteriorActivo = pinzaEstaActiva;
        }
    }
}