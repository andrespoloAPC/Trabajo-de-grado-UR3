using UnityEngine;

public class MenuToggle : MonoBehaviour
{
    public GameObject menuTeclado;
    public Transform camaraJugador;

    public float distancia = 0.8f;
    public float ajusteAltura = 2.0f;
    public void AlternarMenu()
    {
        bool estaActivo = menuTeclado.activeSelf;
        menuTeclado.SetActive(!estaActivo);

        if (!estaActivo)
        {
            PosicionarFrente();
        }
    }

    void PosicionarFrente()
    {
        Vector3 direccionFrente = camaraJugador.forward;
        direccionFrente.y = 0;
        direccionFrente.Normalize();

        Vector3 posicionObjetivo = camaraJugador.position + (direccionFrente * distancia);
        posicionObjetivo.y = camaraJugador.position.y + ajusteAltura;
        menuTeclado.transform.position = posicionObjetivo;
        Vector3 puntoAMirar = new Vector3(camaraJugador.position.x, menuTeclado.transform.position.y, camaraJugador.position.z);
        menuTeclado.transform.LookAt(puntoAMirar);
        menuTeclado.transform.Rotate(0, 180, 0);
    }
}
