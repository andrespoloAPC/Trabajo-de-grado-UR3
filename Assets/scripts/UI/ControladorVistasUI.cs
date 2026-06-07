using UnityEngine;
using TMPro;

public class ControladorVistasUI : MonoBehaviour
{
    [Header("Tus Lienzos (Canvases)")]
    public GameObject primerCanvas;
    public GameObject segundoCanvas;

    [Header("Tu Menú Desplegable")]
    public TMP_Dropdown menuDesplegable;

    [Header("Posicionamiento VR")]
    public Transform camaraJugador;
    public float distancia = 0.8f;
    public float ajusteAltura = 0.0f;

    void Start()
    {
        if (menuDesplegable != null)
        {
            ActualizarCanvases(menuDesplegable.value);
            menuDesplegable.onValueChanged.AddListener(ActualizarCanvases);
        }
    }

    public void ActualizarCanvases(int opcionSeleccionada)
    {
        // Opción 0: Ocultar todo
        if (opcionSeleccionada == 0)
        {
            if (primerCanvas != null) primerCanvas.SetActive(false);
            if (segundoCanvas != null) segundoCanvas.SetActive(false);
        }
        // Opción 1: Mostrar solo el primero y traerlo al frente
        else if (opcionSeleccionada == 1)
        {
            if (primerCanvas != null) primerCanvas.SetActive(true);
            if (segundoCanvas != null) segundoCanvas.SetActive(false);
            PosicionarFrente();
        }
        // Opción 2: Mostrar ambos y traerlos al frente
        else if (opcionSeleccionada == 2)
        {
            if (primerCanvas != null) primerCanvas.SetActive(true);
            if (segundoCanvas != null) segundoCanvas.SetActive(true);
            PosicionarFrente();
        }
    }

    public void PosicionarFrente()
    {
        if (camaraJugador == null || primerCanvas == null) return;

        Vector3 direccionFrente = camaraJugador.forward;
        direccionFrente.y = 0;
        direccionFrente.Normalize();

        Vector3 posicionObjetivo = camaraJugador.position + (direccionFrente * distancia);
        posicionObjetivo.y = camaraJugador.position.y + ajusteAltura;

        // Movemos directamente el Primer Canvas
        primerCanvas.transform.position = posicionObjetivo;

        Vector3 puntoAMirar = new Vector3(camaraJugador.position.x, primerCanvas.transform.position.y, camaraJugador.position.z);
        primerCanvas.transform.LookAt(puntoAMirar);
        primerCanvas.transform.Rotate(0, 180, 0);
    }
}