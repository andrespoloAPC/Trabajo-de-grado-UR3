using UnityEngine;
using TMPro; // Usando TextMeshPro para mejor legibilidad en VR
using System;

public class ActualizarDatosUI : MonoBehaviour
{
    [Header("Textos de Posición Cartesiana")]
    public TextMeshProUGUI txtX;
    public TextMeshProUGUI txtY;
    public TextMeshProUGUI txtZ;
    public TextMeshProUGUI txtRX;
    public TextMeshProUGUI txtRY;
    public TextMeshProUGUI txtRZ;

    [Header("Textos de Posición Articular")]
    public TextMeshProUGUI txtJ1;
    public TextMeshProUGUI txtJ2;
    public TextMeshProUGUI txtJ3;
    public TextMeshProUGUI txtJ4;
    public TextMeshProUGUI txtJ5;
    public TextMeshProUGUI txtJ6;

    void Update()
    {
        // Solo intentamos actualizar si el hilo de datos está vivo y tiene información
        try
        {
            // --- ACTUALIZAR ARTICULACIONES (J1 - J6) ---
            // Convertimos de Radianes a Grados para que sea más fácil de leer
            txtJ1.text = RadToDeg(ur_data_processing.UR_Stream_Data.J_Orientation[0]).ToString("F2") + "°";
            txtJ2.text = RadToDeg(ur_data_processing.UR_Stream_Data.J_Orientation[1]).ToString("F2") + "°";
            txtJ3.text = RadToDeg(ur_data_processing.UR_Stream_Data.J_Orientation[2]).ToString("F2") + "°";
            txtJ4.text = RadToDeg(ur_data_processing.UR_Stream_Data.J_Orientation[3]).ToString("F2") + "°";
            txtJ5.text = RadToDeg(ur_data_processing.UR_Stream_Data.J_Orientation[4]).ToString("F2") + "°";
            txtJ6.text = RadToDeg(ur_data_processing.UR_Stream_Data.J_Orientation[5]).ToString("F2") + "°";

            // --- ACTUALIZAR CARTESIANAS (X, Y, Z, RX, RY, RZ) ---
            // URSim envía X,Y,Z en metros. Multiplicamos por 1000 si quieres ver milímetros.
            txtX.text = (ur_data_processing.UR_Stream_Data.C_Position[0] * 1000f).ToString("F1") + " mm";
            txtY.text = (ur_data_processing.UR_Stream_Data.C_Position[1] * 1000f).ToString("F1") + " mm";
            txtZ.text = (ur_data_processing.UR_Stream_Data.C_Position[2] * 1000f).ToString("F1") + " mm";

            // RX, RY, RZ suelen venir en vectores de rotación (Radianes)
            txtRX.text = ur_data_processing.UR_Stream_Data.C_Orientation[0].ToString("F3");
            txtRY.text = ur_data_processing.UR_Stream_Data.C_Orientation[1].ToString("F3");
            txtRZ.text = ur_data_processing.UR_Stream_Data.C_Orientation[2].ToString("F3");
        }
        catch (Exception)
        {
            // Si la conexión aún no inicia, evitamos que el log se llene de errores
        }
    }

    float RadToDeg(double rad)
    {
        return (float)(rad * (180.0 / Math.PI));
    }
}