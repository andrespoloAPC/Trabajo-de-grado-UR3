using UnityEngine;
using UnityEngine.UI;
using System.Collections; // <-- OBLIGATORIO PARA USAR CORRUTINAS
using UnityEngine.SceneManagement;
public class ConectorFinalUR : MonoBehaviour
{
    public IPKeyboard teclado;
    public Image luzEstado;

    public void IniciarTodo()
    {
        // Validación de seguridad (Alertas activas)
        if (ur_data_processing.GlobalVariables_Main_Control.disconnect)
        {
            Debug.LogWarning("Conexión bloqueada: Primero presiona 'OK' en el panel de alerta para liberar el sistema.");
            return;
        }

        string ipEscrita = teclado.ObtenerIP().Trim();

        if (string.IsNullOrEmpty(ipEscrita))
        {
            Debug.LogError("¡La IP está vacía! Escribe la IP en el teclado VR.");
            return;
        }

        // --- EL CAMBIO ESTÁ AQUÍ ---
        // En lugar de prender todo de golpe, llamamos a la corrutina para hacer un reinicio limpio
        StartCoroutine(ReconectarLimpio(ipEscrita));
    }

    // Esta función nos permite pausar el tiempo en Unity
    private IEnumerator ReconectarLimpio(string nuevaIP)
    {
        // 1. APAGAMOS LOS MOTORES DE RED
        ur_data_processing.GlobalVariables_Main_Control.connect = false;

        // 2. LE DAMOS UN RESPIRO (0.5 segundos)
        // Esto le da tiempo al código base para cerrar los puertos y matar los hilos atascados de la IP vieja
        yield return new WaitForSeconds(0.5f);

        // 3. INYECTAMOS LA IP NUEVA Y CORREGIDA
        ur_data_processing.UR_Stream_Data.ip_address = nuevaIP;
        ur_data_processing.UR_Control_Data.ip_address = nuevaIP;

        // 4. VOLVEMOS A ENCENDER
        ur_data_processing.GlobalVariables_Main_Control.connect = true;

        // Feedback visual
        if (luzEstado != null) luzEstado.color = Color.yellow;
        Debug.Log(">>> Intentando conectar limpiamente con Robot UR en: " + nuevaIP);

        CancelInvoke("VerificarConexion");
        Invoke("VerificarConexion", 4.0f);
    }

    void VerificarConexion()
    {
        if (ur_data_processing.UR_Stream_Data.is_alive)
        {
            if (luzEstado != null) luzEstado.color = Color.green;
            Debug.Log(">>> [CONECTADO] Lectura y Control activos.");
        }
        else
        {
            if (luzEstado != null) luzEstado.color = Color.red;
            Debug.LogError(">>> [FALLO] No se pudo conectar. Revisa la IP o el Firewall.");

            // Si falla, lo dejamos apagado para que la próxima vez arranque limpio
            ur_data_processing.GlobalVariables_Main_Control.connect = false;
        }
    }
    // =========================================================
    // NUEVO: SISTEMA DE RESET GENERAL DE RED
    // =========================================================
    public void EjecutarResetGeneral()
    {
        Debug.Log("<color=red>INICIANDO RESET NUCLEAR: Destruyendo hilos de red y recargando entorno...</color>");

        if (luzEstado != null) luzEstado.color = Color.gray; // Feedback visual antes del reinicio

        // Usamos una pequeña corrutina para darle tiempo a Unity de procesar el color de la luz
        // antes de destruir la escena
        StartCoroutine(RecargarEscena());
    }

    private IEnumerator RecargarEscena()
    {
        // Pequeño parpadeo para que el usuario sepa que algo está pasando
        yield return new WaitForSeconds(0.2f);

        // OBTENEMOS EL NOMBRE DE LA ESCENA EN LA QUE ESTAMOS ACTUALMENTE
        string escenaActual = SceneManager.GetActiveScene().name;

        // RECARGAMOS LA ESCENA DESDE CERO
        // Esto aniquila cualquier variable, socket abierto o hilo bloqueado
        SceneManager.LoadScene(escenaActual);
    }
}