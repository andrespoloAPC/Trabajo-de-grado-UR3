using TMPro;
using UnityEngine;
public class IPKeyboard : MonoBehaviour
{
    public TextMeshProUGUI pantalla;
    private string textoActual = "";
    public void PresionarTecla(string valor)
    {
        if (textoActual.Length < 15)
        {
            textoActual += valor;
            ActualizarPantalla();
        }
    }
    public void Borrar()
    {
        if (textoActual.Length > 0)
        {
            textoActual = textoActual.Substring(0, textoActual.Length - 1);
            ActualizarPantalla();
        }
    }
    void ActualizarPantalla()
    {
        pantalla.text = textoActual;
    }
    public string ObtenerIP()
    {
        return textoActual;
    }

    public string GetIP() => textoActual;
}
