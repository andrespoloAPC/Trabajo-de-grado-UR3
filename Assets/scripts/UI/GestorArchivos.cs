using UnityEngine;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class GestorArchivosDropdown : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown dropdownArchivos;

    void Start()
    {
        // Al iniciar la escena, buscamos los archivos y llenamos el menú
        ActualizarListaArchivos();
    }

    // Esta función puedes llamarla desde un botón de "Refrescar" si lo deseas
    public void ActualizarListaArchivos()
    {
        if (dropdownArchivos == null) return;

        // 1. Vaciamos las opciones viejas para no duplicar datos
        dropdownArchivos.ClearOptions();

        // 2. Buscamos todos los archivos .script en el almacenamiento de las gafas
        string[] rutasArchivos = Directory.GetFiles(Application.persistentDataPath, "*.script");

        // 3. Creamos una lista temporal para guardar solo los nombres limpios
        List<string> nombresArchivos = new List<string>();

        if (rutasArchivos.Length == 0)
        {
            // Si no hay archivos, mostramos un mensaje por defecto y bloqueamos el Dropdown
            nombresArchivos.Add("No hay archivos guardados");
            dropdownArchivos.interactable = false;
        }
        else
        {
            dropdownArchivos.interactable = true;

            // Recorremos las rutas y extraemos solo el nombre del archivo
            foreach (string ruta in rutasArchivos)
            {
                // Path.GetFileName convierte "/Android/data/.../archivo.script" en "archivo.script"
                nombresArchivos.Add(Path.GetFileName(ruta));
            }
        }

        // 4. Inyectamos nuestra lista en el Dropdown de Unity
        dropdownArchivos.AddOptions(nombresArchivos);
    }

    // Esta función te servirá para saber qué archivo eligió el usuario
    // (Puedes conectarla al evento "On Value Changed" del Dropdown o a un botón de "Cargar")
    public void ObtenerArchivoSeleccionado()
    {
        if (dropdownArchivos.options.Count > 0 && dropdownArchivos.interactable)
        {
            // Leemos el texto de la opción que está seleccionada actualmente
            string archivoElegido = dropdownArchivos.options[dropdownArchivos.value].text;

            Debug.Log("El archivo seleccionado para cargar es: " + archivoElegido);

            // AQUÍ PUEDES LLAMAR A TU FUNCIÓN DE CARGAR TRAYECTORIA
            // Ej: FindObjectOfType<URTeachManager>().CargarArchivoSeleccionado(archivoElegido);
        }
    }
}