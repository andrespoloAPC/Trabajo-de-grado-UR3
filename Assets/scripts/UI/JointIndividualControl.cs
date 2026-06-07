using UnityEngine;
using UnityEngine.EventSystems;
using System.Text;
using System.Globalization;

public class ControlBotonFijo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum TipoControl { Articular, Cartesiano }
    public enum Eje { Eje1, Eje2, Eje3, Eje4, Eje5, Eje6 }
    public enum Direccion { Positivo, Negativo }

    [Header("Configuración de Movimiento")]
    public TipoControl tipo = TipoControl.Articular;
    public Eje seleccion = Eje.Eje1;
    public Direccion sentido = Direccion.Positivo;

    [Header("Ajustes de Velocidad")]
    public float velocidadFija = 0.1f; // Radianes/seg para Joints, Metros/seg para XYZ
    public int botonID; // ID único del 0 al 17

    private UTF8Encoding utf8 = new UTF8Encoding();

    public void OnPointerDown(PointerEventData eventData)
    {
        double[] valores = { 0, 0, 0, 0, 0, 0 };
        float velFinal = velocidadFija;

        if (sentido == Direccion.Negativo) velFinal *= -1;

        // Asignamos la velocidad solo al eje que queremos mover
        valores[(int)seleccion] = velFinal;

        // Determinamos el comando (speedj para articulaciones, speedl para X,Y,Z)
        string cmdType = (tipo == TipoControl.Articular) ? "speedj" : "speedl";

        // Formateamos el comando asegurando que use puntos decimales (importante para Android/Quest)
        string paramsStr = string.Format(CultureInfo.InvariantCulture,
            "[{0:F4},{1:F4},{2:F4},{3:F4},{4:F4},{5:F4}]",
            valores[0], valores[1], valores[2], valores[3], valores[4], valores[5]);

        // t=0.1 es un tiempo de respuesta sólido para Wi-Fi
        string command = cmdType + "(" + paramsStr + ", a=0.5, t=0.1)\n";

        // Pasamos los bytes al procesador de Roman Parak
        ur_data_processing.UR_Control_Data.command = utf8.GetBytes(command);
        ur_data_processing.UR_Control_Data.button_pressed[botonID] = true;

        Debug.Log("Enviando a UR3: " + command);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Al soltar, indicamos al procesador que deje de enviar el comando
        ur_data_processing.UR_Control_Data.button_pressed[botonID] = false;
    }
}