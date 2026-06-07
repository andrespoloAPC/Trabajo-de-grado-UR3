using UnityEngine;

public class InteraccionPinza : MonoBehaviour
{
    [Header("Configuración de Agarre")]
    [Tooltip("El Tag que deben tener los objetos para poder ser agarrados")]
    public string tagAgarrable = "Agarrable";

    private bool estadoAnteriorPinza = false;
    private GameObject objetoEnZona = null; // Lo que estamos tocando
    private GameObject objetoAgarrado = null; // Lo que tenemos en la mano

    void Update()
    {
        // Leemos constantemente la variable global que creamos antes
        bool estadoActualPinza = ur_data_processing.UR_Control_Data.pinza_esta_cerrada;

        // Detectamos si hubo un CAMBIO en el estado (flanco de subida o bajada)
        if (estadoActualPinza != estadoAnteriorPinza)
        {
            if (estadoActualPinza == true)
            {
                AgarrarObjeto();
            }
            else
            {
                SoltarObjeto();
            }

            // Actualizamos la memoria
            estadoAnteriorPinza = estadoActualPinza;
        }
    }

    // Cuando un objeto entra en la zona invisible de la pinza
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagAgarrable))
        {
            objetoEnZona = other.gameObject;
        }
    }

    // Cuando el objeto sale de la zona invisible
    private void OnTriggerExit(Collider other)
    {
        if (objetoEnZona != null && other.gameObject == objetoEnZona)
        {
            objetoEnZona = null;
        }
    }

    private void AgarrarObjeto()
    {
        // Solo agarramos si hay algo en la zona y tenemos la mano vacía
        if (objetoEnZona != null && objetoAgarrado == null)
        {
            objetoAgarrado = objetoEnZona;

            // 1. Apagamos la física temporalmente para que Unity no pelee con el robot
            Rigidbody rb = objetoAgarrado.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            // 2. Hacemos que el objeto sea "hijo" de la pinza (se moverá pegado a ella)
            objetoAgarrado.transform.SetParent(this.transform);

            Debug.Log("Objeto Agarrado: " + objetoAgarrado.name);
        }
    }

    private void SoltarObjeto()
    {
        if (objetoAgarrado != null)
        {
            // 1. Reactivamos las físicas
            Rigidbody rb = objetoAgarrado.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            // 2. Rompemos el vínculo (ya no es hijo de la pinza)
            objetoAgarrado.transform.SetParent(null);

            Debug.Log("Objeto Soltado: " + objetoAgarrado.name);
            objetoAgarrado = null;
        }
    }
}