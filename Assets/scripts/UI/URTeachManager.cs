using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Globalization;
using System.Collections;

public enum TipoAccion { Movimiento, AbrirPinza, CerrarPinza }

[System.Serializable]
public class AccionTrayectoria
{
    public TipoAccion tipo;
    public float x, y, z, rx, ry, rz;
    public GameObject marcadorVisual;
    public Vector3 posicionUnity;

    public AccionTrayectoria(float _x, float _y, float _z, float _rx, float _ry, float _rz, Vector3 posU)
    {
        tipo = TipoAccion.Movimiento;
        x = _x; y = _y; z = _z; rx = _rx; ry = _ry; rz = _rz;
        posicionUnity = posU;
    }

    public AccionTrayectoria(TipoAccion _tipoPinza) { tipo = _tipoPinza; }

    public string TextoParaPantalla()
    {
        if (tipo == TipoAccion.Movimiento)
            return $"movel(p[{x.ToString("F4", CultureInfo.InvariantCulture)}, {y.ToString("F4", CultureInfo.InvariantCulture)}, {z.ToString("F4", CultureInfo.InvariantCulture)}, " +
                   $"{rx.ToString("F4", CultureInfo.InvariantCulture)}, {ry.ToString("F4", CultureInfo.InvariantCulture)}, {rz.ToString("F4", CultureInfo.InvariantCulture)}])";
        else if (tipo == TipoAccion.CerrarPinza) return "<color=#ff5555>[COMANDO: CERRAR PINZA]</color>";
        else return "<color=#55ff55>[COMANDO: ABRIR PINZA]</color>";
    }
}

public class URTeachManager : MonoBehaviour
{
    [Header("UI - Modo Edición")]
    public TextMeshProUGUI textoListaPuntos;
    public TextMeshProUGUI textoNombreEnVivo;

    [Header("Configuración de Movimiento UR")]
    public float aceleracion = 1.2f;
    public float velocidad = 0.25f;

    [Header("Configuración de Puntos VR")]
    public GameObject prefabPuntoVRPunto;
    public Transform efectorFinalVR;
    public float umbralLlegada = 0.1f;

    [Header("Configuración de Ciclos (VR)")]
    public TextMeshProUGUI textoCiclosObjetivo;
    public TextMeshProUGUI textoCiclosCompletados;

    private List<AccionTrayectoria> trayectoria = new List<AccionTrayectoria>();
    private int indiceSeleccionado = -1;

    private TouchScreenKeyboard tecladoArchivosVR;
    private bool esperandoNombreArchivo = false;
    private string archivoActualAbierto = "";

    private TouchScreenKeyboard tecladoCiclosVR;
    private bool esperandoCiclos = false;

    private bool ejecutandoTrayectoria = false;
    private int indicePasoActual = 0;

    private int ciclosObjetivo = 1;
    private int ciclosCompletados = 0;

    void Update()
    {
        // 1. Teclado para Nombre de Archivo
        if (esperandoNombreArchivo && tecladoArchivosVR != null)
        {
            if (textoNombreEnVivo != null) textoNombreEnVivo.text = tecladoArchivosVR.text + "_";

            if (tecladoArchivosVR.status == TouchScreenKeyboard.Status.Done || tecladoArchivosVR.status == TouchScreenKeyboard.Status.Canceled)
            {
                esperandoNombreArchivo = false;
                string nombreEscrito = tecladoArchivosVR.text.Trim();
                if (string.IsNullOrEmpty(nombreEscrito)) nombreEscrito = "Tray_UR3_" + System.DateTime.Now.ToString("HHmmss");
                GuardarArchivoLocal(nombreEscrito);
            }
        }

        // 2. Teclado para Cantidad de Ciclos
        if (esperandoCiclos && tecladoCiclosVR != null)
        {
            if (textoCiclosObjetivo != null) textoCiclosObjetivo.text = "Escribiendo: " + tecladoCiclosVR.text;

            if (tecladoCiclosVR.status == TouchScreenKeyboard.Status.Done || tecladoCiclosVR.status == TouchScreenKeyboard.Status.Canceled)
            {
                esperandoCiclos = false;
                string entradaStr = tecladoCiclosVR.text.Trim();

                if (int.TryParse(entradaStr, out int numeroParseado) && numeroParseado > 0)
                {
                    ciclosObjetivo = numeroParseado;
                    if (textoCiclosObjetivo != null) textoCiclosObjetivo.text = "Objetivo: " + ciclosObjetivo;
                    ciclosCompletados = 0;
                    ActualizarTextosUI();
                }
                else
                {
                    if (textoCiclosObjetivo != null) textoCiclosObjetivo.text = "Objetivo: " + ciclosObjetivo;
                }
            }
        }

        // 3. Máquina de Estados Visual (Tú lógica original para pintar esferas)
        if (ejecutandoTrayectoria && efectorFinalVR != null && trayectoria.Count > 0)
        {
            if (indicePasoActual < trayectoria.Count)
            {
                AccionTrayectoria pasoActual = trayectoria[indicePasoActual];
                bool pasoCompletado = false;

                if (pasoActual.tipo == TipoAccion.Movimiento)
                {
                    if (pasoActual.marcadorVisual != null)
                    {
                        // ESTA ES LA LÓGICA QUE SABEMOS QUE FUNCIONA
                        float distancia = Vector3.Distance(efectorFinalVR.position, pasoActual.marcadorVisual.transform.position);
                        if (distancia < umbralLlegada)
                        {
                            pasoActual.marcadorVisual.GetComponent<Renderer>().material.color = Color.green;
                            pasoCompletado = true;
                        }
                    }
                    else { pasoCompletado = true; }
                }
                else if (pasoActual.tipo == TipoAccion.AbrirPinza)
                {
                    ur_data_processing.UR_Control_Data.pinza_esta_cerrada = false;
                    pasoCompletado = true;
                }
                else if (pasoActual.tipo == TipoAccion.CerrarPinza)
                {
                    ur_data_processing.UR_Control_Data.pinza_esta_cerrada = true;
                    pasoCompletado = true;
                }

                if (pasoCompletado)
                {
                    indicePasoActual++;
                    ActualizarPantallaUI();
                }
            }
        }
    }

    public void PedirCiclosConTecladoNativo()
    {
        tecladoCiclosVR = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad);
        esperandoCiclos = true;
    }

    private void ActualizarTextosUI()
    {
        if (textoCiclosCompletados != null)
        {
            textoCiclosCompletados.text = "Completados: " + ciclosCompletados + " / " + ciclosObjetivo;
        }
    }

    public void EjecutarTrayectoriaEnRobot()
    {
        if (ur_data_processing.GlobalVariables_Main_Control.disconnect || !ur_data_processing.GlobalVariables_Main_Control.connect) return;
        if (trayectoria.Count == 0 || ejecutandoTrayectoria) return;

        StartCoroutine(RutinaEjecutarCiclos());
    }

    // =========================================================
    // LÓGICA DE CICLOS BASADA EN TU SISTEMA DE ESFERAS VERDES
    // =========================================================
    private IEnumerator RutinaEjecutarCiclos()
    {
        ejecutandoTrayectoria = true;
        ciclosCompletados = 0;
        ActualizarTextosUI();

        string codigoUR = GenerarCodigoURScript(false);

        while (ciclosCompletados < ciclosObjetivo)
        {
            // 1. Enviar trayectoria
            ur_data_processing.UR_Control_Data.comando_personalizado = codigoUR;
            ur_data_processing.UR_Control_Data.enviar_comando_personalizado = true;

            // 2. Reiniciar la UI y poner esferas en rojo
            indicePasoActual = 0;
            ActualizarPantallaUI();

            foreach (AccionTrayectoria accion in trayectoria)
            {
                if (accion.tipo == TipoAccion.Movimiento && accion.marcadorVisual != null)
                    accion.marcadorVisual.GetComponent<Renderer>().material.color = Color.red;
            }

            // 3. ESPERAR A QUE EL UPDATE TERMINE DE PINTAR TODAS LAS ESFERAS
            // El código se pausa aquí hasta que indicePasoActual llegue al final de la lista
            yield return new WaitUntil(() =>
                indicePasoActual >= trayectoria.Count ||
                !ejecutandoTrayectoria ||
                ur_data_processing.GlobalVariables_Main_Control.disconnect
            );

            // Cortocircuito por emergencias
            if (!ejecutandoTrayectoria || ur_data_processing.GlobalVariables_Main_Control.disconnect) break;

            // 4. ESPERAR EL RETORNO A HOME (Punto 0)
            // Como tu código URScript hace que el robot vuelva al inicio, esperamos a que toque la primera esfera
            if (trayectoria[0].marcadorVisual != null)
            {
                yield return new WaitUntil(() =>
                    Vector3.Distance(efectorFinalVR.position, trayectoria[0].marcadorVisual.transform.position) < umbralLlegada ||
                    !ejecutandoTrayectoria ||
                    ur_data_processing.GlobalVariables_Main_Control.disconnect
                );
            }

            if (!ejecutandoTrayectoria || ur_data_processing.GlobalVariables_Main_Control.disconnect) break;

            // 5. ¡CICLO EXITOSO!
            ciclosCompletados++;
            ActualizarTextosUI();

            // Pequeña pausa antes de volver a empezar para no atrofiar la red
            yield return new WaitForSeconds(0.5f);
        }

        ejecutandoTrayectoria = false;
        ActualizarPantallaUI();
    }

    public void CancelarLogicaTrayectoria()
    {
        ejecutandoTrayectoria = false;
        indicePasoActual = 0;
        ActualizarPantallaUI();
        StopAllCoroutines();
    }

    // =========================================================
    // UI PANTALLA: SEPARACIÓN DE CURSOR MANUAL (AMARILLO) Y ROBOT (CYAN)
    // =========================================================
    private void ActualizarPantallaUI()
    {
        if (textoListaPuntos == null) return;
        string display = "<b>PROGRAMA UR3:</b>\n\n";

        for (int i = 0; i < trayectoria.Count; i++)
        {
            string prefijo = "  ";
            string colorApertura = "";
            string colorCierre = "";

            // Cursor del Robot en Vivo (Cyan)
            if (ejecutandoTrayectoria && i == indicePasoActual)
            {
                prefijo = "► ";
                colorApertura = "<color=#00FFFF>";
                colorCierre = "</color>";
            }

            // Tu Cursor de Edición Manual (Siempre Amarillo)
            if (i == indiceSeleccionado)
            {
                // Si el robot pasa por la línea que estás seleccionando
                prefijo = (ejecutandoTrayectoria && i == indicePasoActual) ? "►> " : "> ";
                colorApertura = "<color=yellow>";
                colorCierre = "</color>";
            }

            display += $"{colorApertura}{prefijo}[{i}] {trayectoria[i].TextoParaPantalla()}{colorCierre}\n";
        }
        textoListaPuntos.text = display;
    }

    // =========================================================
    // GUARDADO, CARGA Y EDICIÓN DE PUNTOS
    // =========================================================
    public void GuardarPosicionActual()
    {
        float tx = (float)ur_data_processing.UR_Stream_Data.C_Position[0];
        float ty = (float)ur_data_processing.UR_Stream_Data.C_Position[1];
        float tz = (float)ur_data_processing.UR_Stream_Data.C_Position[2];
        float trx = (float)ur_data_processing.UR_Stream_Data.C_Orientation[0];
        float try_ = (float)ur_data_processing.UR_Stream_Data.C_Orientation[1];
        float trz = (float)ur_data_processing.UR_Stream_Data.C_Orientation[2];

        Vector3 posActualUnity = efectorFinalVR != null ? efectorFinalVR.position : Vector3.zero;
        AccionTrayectoria nuevaAccion = new AccionTrayectoria(tx, ty, tz, trx, try_, trz, posActualUnity);

        if (prefabPuntoVRPunto != null && efectorFinalVR != null)
        {
            nuevaAccion.marcadorVisual = Instantiate(prefabPuntoVRPunto, posActualUnity, efectorFinalVR.rotation);
            nuevaAccion.marcadorVisual.GetComponent<Renderer>().material.color = Color.cyan;
        }

        InsertarEnLista(nuevaAccion);
    }

    public void AsignarPinzaAbrir() { InsertarEnLista(new AccionTrayectoria(TipoAccion.AbrirPinza)); }
    public void AsignarPinzaCerrar() { InsertarEnLista(new AccionTrayectoria(TipoAccion.CerrarPinza)); }

    private void InsertarEnLista(AccionTrayectoria accion)
    {
        if (indiceSeleccionado == -1 || indiceSeleccionado >= trayectoria.Count)
        {
            trayectoria.Add(accion);
            indiceSeleccionado = trayectoria.Count - 1;
        }
        else
        {
            trayectoria.Insert(indiceSeleccionado + 1, accion);
            indiceSeleccionado++;
        }
        ActualizarPantallaUI();
    }

    public void BorrarLineaSeleccionada()
    {
        if (trayectoria.Count > 0 && indiceSeleccionado >= 0 && indiceSeleccionado < trayectoria.Count)
        {
            if (trayectoria[indiceSeleccionado].marcadorVisual != null) Destroy(trayectoria[indiceSeleccionado].marcadorVisual);
            trayectoria.RemoveAt(indiceSeleccionado);
            indiceSeleccionado--;
            if (indiceSeleccionado < 0 && trayectoria.Count > 0) indiceSeleccionado = 0;
            ActualizarPantallaUI();
        }
    }

    public void SeleccionarAnterior() { if (indiceSeleccionado > 0) { indiceSeleccionado--; ActualizarPantallaUI(); } }
    public void SeleccionarSiguiente() { if (indiceSeleccionado < trayectoria.Count - 1) { indiceSeleccionado++; ActualizarPantallaUI(); } }

    public void IniciarGuardadoArchivo()
    {
        if (trayectoria.Count == 0) return;
        if (!string.IsNullOrEmpty(archivoActualAbierto)) GuardarArchivoLocal(archivoActualAbierto);
        else
        {
            if (textoNombreEnVivo != null) textoNombreEnVivo.text = "_";
            tecladoArchivosVR = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
            esperandoNombreArchivo = true;
        }
    }

    private void GuardarArchivoLocal(string nombre)
    {
        string codigoUR = GenerarCodigoURScript(true);
        string ruta = Path.Combine(Application.persistentDataPath, nombre + ".script");
        File.WriteAllText(ruta, codigoUR);
        archivoActualAbierto = nombre;
        if (textoNombreEnVivo != null) textoNombreEnVivo.text = nombre + ".script";
        if (textoListaPuntos != null) textoListaPuntos.text += $"\n<color=green>¡Guardado!: {nombre}.script</color>";
    }

    private string GenerarCodigoURScript(bool incluirVRPOS)
    {
        string script = "def trayectoria_vr():\n";
        AccionTrayectoria primerMovimiento = null;

        foreach (AccionTrayectoria accion in trayectoria)
        {
            if (accion.tipo == TipoAccion.Movimiento)
            {
                if (primerMovimiento == null) primerMovimiento = accion;
                string coords = $"p[{accion.x.ToString("F4", CultureInfo.InvariantCulture)}, {accion.y.ToString("F4", CultureInfo.InvariantCulture)}, {accion.z.ToString("F4", CultureInfo.InvariantCulture)}, {accion.rx.ToString("F4", CultureInfo.InvariantCulture)}, {accion.ry.ToString("F4", CultureInfo.InvariantCulture)}, {accion.rz.ToString("F4", CultureInfo.InvariantCulture)}]";

                if (incluirVRPOS)
                {
                    string posU = $"{accion.posicionUnity.x.ToString(CultureInfo.InvariantCulture)}|{accion.posicionUnity.y.ToString(CultureInfo.InvariantCulture)}|{accion.posicionUnity.z.ToString(CultureInfo.InvariantCulture)}";
                    script += $"  movel({coords}, a={aceleracion.ToString("F2", CultureInfo.InvariantCulture)}, v={velocidad.ToString("F2", CultureInfo.InvariantCulture)}) # VRPOS:{posU}\n";
                }
                else
                {
                    script += $"  movel({coords}, a={aceleracion.ToString("F2", CultureInfo.InvariantCulture)}, v={velocidad.ToString("F2", CultureInfo.InvariantCulture)})\n";
                }
            }
            else
            {
                int posicionPinza = (accion.tipo == TipoAccion.CerrarPinza) ? 255 : 0;
                script += $"  socket_open(\"127.0.0.1\", 63352, \"rq\")\n";
                script += $"  socket_send_string(\"SET SPE 255\", \"rq\")\n  socket_send_byte(10, \"rq\")\n";
                script += $"  socket_send_string(\"SET POS {posicionPinza}\", \"rq\")\n  socket_send_byte(10, \"rq\")\n";
                script += $"  socket_close(\"rq\")\n  sleep(0.5)\n";
            }
        }

        if (primerMovimiento != null)
        {
            script += $"  # Retorno a Home\n";
            string coordsHome = $"p[{primerMovimiento.x.ToString("F4", CultureInfo.InvariantCulture)}, {primerMovimiento.y.ToString("F4", CultureInfo.InvariantCulture)}, {primerMovimiento.z.ToString("F4", CultureInfo.InvariantCulture)}, {primerMovimiento.rx.ToString("F4", CultureInfo.InvariantCulture)}, {primerMovimiento.ry.ToString("F4", CultureInfo.InvariantCulture)}, {primerMovimiento.rz.ToString("F4", CultureInfo.InvariantCulture)}]";
            script += $"  movel({coordsHome}, a={aceleracion.ToString("F2", CultureInfo.InvariantCulture)}, v={velocidad.ToString("F2", CultureInfo.InvariantCulture)})\n";
        }
        script += "end\n";
        return script;
    }

    public void CrearNuevoPrograma()
    {
        foreach (AccionTrayectoria accion in trayectoria)
        {
            if (accion.marcadorVisual != null) Destroy(accion.marcadorVisual);
        }
        trayectoria.Clear();
        indiceSeleccionado = -1;
        archivoActualAbierto = "";
        ejecutandoTrayectoria = false;
        if (textoNombreEnVivo != null) textoNombreEnVivo.text = "Nuevo Archivo";
        ActualizarPantallaUI();
    }

    public void CargarArchivoDesdeDropdown(TMP_Dropdown dropdownArchivos)
    {
        if (dropdownArchivos == null || dropdownArchivos.options.Count == 0 || !dropdownArchivos.interactable) return;

        string nombreArchivo = dropdownArchivos.options[dropdownArchivos.value].text;
        string ruta = Path.Combine(Application.persistentDataPath, nombreArchivo);

        if (!File.Exists(ruta)) return;

        CrearNuevoPrograma();
        archivoActualAbierto = nombreArchivo.Replace(".script", "");
        if (textoNombreEnVivo != null) textoNombreEnVivo.text = nombreArchivo;

        string[] lineas = File.ReadAllLines(ruta);

        foreach (string linea in lineas)
        {
            if (linea.Contains("# Retorno a Home")) break;

            if (linea.Contains("movel(p["))
            {
                try
                {
                    int inicio = linea.IndexOf("p[") + 2;
                    int fin = linea.IndexOf("]", inicio);
                    string coords = linea.Substring(inicio, fin - inicio);
                    string[] vals = coords.Split(',');

                    if (vals.Length == 6)
                    {
                        float x = float.Parse(vals[0].Trim(), CultureInfo.InvariantCulture);
                        float y = float.Parse(vals[1].Trim(), CultureInfo.InvariantCulture);
                        float z = float.Parse(vals[2].Trim(), CultureInfo.InvariantCulture);
                        float rx = float.Parse(vals[3].Trim(), CultureInfo.InvariantCulture);
                        float ry = float.Parse(vals[4].Trim(), CultureInfo.InvariantCulture);
                        float rz = float.Parse(vals[5].Trim(), CultureInfo.InvariantCulture);

                        Vector3 posCargadaUnity = Vector3.zero;
                        if (linea.Contains("# VRPOS:"))
                        {
                            string vrData = linea.Substring(linea.IndexOf("VRPOS:") + 6).Trim();
                            string[] vrVals = vrData.Split('|');
                            if (vrVals.Length == 3)
                            {
                                posCargadaUnity = new Vector3(
                                    float.Parse(vrVals[0], CultureInfo.InvariantCulture),
                                    float.Parse(vrVals[1], CultureInfo.InvariantCulture),
                                    float.Parse(vrVals[2], CultureInfo.InvariantCulture)
                                );
                            }
                        }

                        AccionTrayectoria accionCargada = new AccionTrayectoria(x, y, z, rx, ry, rz, posCargadaUnity);

                        if (prefabPuntoVRPunto != null && posCargadaUnity != Vector3.zero)
                        {
                            accionCargada.marcadorVisual = Instantiate(prefabPuntoVRPunto, posCargadaUnity, Quaternion.identity);
                            accionCargada.marcadorVisual.GetComponent<Renderer>().material.color = Color.cyan;
                        }

                        trayectoria.Add(accionCargada);
                    }
                }
                catch { continue; }
            }
            else if (linea.Contains("SET POS 255")) trayectoria.Add(new AccionTrayectoria(TipoAccion.CerrarPinza));
            else if (linea.Contains("SET POS 0")) trayectoria.Add(new AccionTrayectoria(TipoAccion.AbrirPinza));
        }

        indiceSeleccionado = trayectoria.Count > 0 ? trayectoria.Count - 1 : -1;
        ActualizarPantallaUI();
    }
}