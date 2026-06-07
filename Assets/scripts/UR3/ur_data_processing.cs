/****************************************************************************
MIT License
Copyright(c) 2021 Roman Parak
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
...
*****************************************************************************
Author   : Roman Parak
Email    : Roman.Parak @outlook.com
Github   : https://github.com/rparak
File Name: ur_data_processing.cs
****************************************************************************/

// System
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
// Unity 
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ur_data_processing : MonoBehaviour
{
    [Header("Debug Gripper")]
    public TMPro.TextMeshProUGUI textoDebugGripper;
    public static class GlobalVariables_Main_Control
    {
        public static bool connect, disconnect;
    }

    public static class UR_Stream_Data
    {
        public static string ip_address;
        public const ushort port_number = 30003;
        public static int time_step;
        public static double[] J_Orientation = new double[6];
        public static double[] C_Position = new double[3];
        public static double[] C_Orientation = new double[3];
        public static bool is_alive = false;
    }
    public static class UR_Control_Data
    {
        public static string ip_address;
        public const ushort port_number = 30002;
        public static int time_step;
        public static string aux_command_str;
        public static byte[] command;
        public static bool[] button_pressed = new bool[18];
        public static bool joystick_button_pressed;
        public static bool is_alive = false;
        public static bool enviar_comando_gripper = false;
        public static int posicion_gripper = 0;
        public static string comando_personalizado = "";
        public static bool enviar_comando_personalizado = false;
        public static bool pinza_esta_cerrada = false;

    }

    private UR_Stream ur_stream_robot;
    private UR_Control ur_ctrl_robot;
    private int main_ur3_state = 0;
    private int aux_counter_pressed_btn = 0;

    void Start()
    {
        UR_Stream_Data.time_step = 8;
        UR_Control_Data.time_step = 50;
        ur_stream_robot = new UR_Stream();
        ur_ctrl_robot = new UR_Control();
    }

    void FixedUpdate()
    {
        switch (main_ur3_state)
        {
            case 0:
                {
                    if (GlobalVariables_Main_Control.connect == true)
                    {
                        UR_Control_Data.ip_address = UR_Stream_Data.ip_address;
                        ur_stream_robot.Start();
                        ur_ctrl_robot.Start();
                        main_ur3_state = 1;
                    }
                }
                break;
            case 1:
                {
                    for (int i = 0; i < UR_Control_Data.button_pressed.Length; i++)
                    {
                        if (UR_Control_Data.button_pressed[i] == true)
                        {
                            aux_counter_pressed_btn++;
                        }
                    }

                    if (aux_counter_pressed_btn > 0)
                    {
                        UR_Control_Data.joystick_button_pressed = true;
                    }
                    else
                    {
                        UR_Control_Data.joystick_button_pressed = false;
                    }

                    aux_counter_pressed_btn = 0;

                    if (GlobalVariables_Main_Control.disconnect == true)
                    {
                        if (UR_Stream_Data.is_alive == true)
                        {
                            ur_stream_robot.Stop();
                        }
                        if (UR_Control_Data.is_alive == true)
                        {
                            ur_ctrl_robot.Stop();
                        }
                        if (UR_Stream_Data.is_alive == false && UR_Control_Data.is_alive == false)
                        {
                            main_ur3_state = 0;
                        }
                    }
                }
                break;
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            ur_stream_robot.Destroy();
            ur_ctrl_robot.Destroy();
            Destroy(this);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    class UR_Stream
    {
        private Thread robot_thread = null;
        private bool exit_thread = false;
        private TcpClient tcp_client = null;
        private NetworkStream network_stream = null;

        private const byte offset = 8;

        public void UR_Stream_Thread()
        {
            var t = new Stopwatch();

            while (exit_thread == false)
            {
                try
                {
                    // Auto-Reconector BLINDADO
                    if (tcp_client == null || !tcp_client.Connected)
                    {
                        tcp_client = new TcpClient();
                        // BLINDAJE: Tolerancia de 2 segundos para saltos de red Wi-Fi o VM
                        tcp_client.ReceiveTimeout = 2000;
                        tcp_client.SendTimeout = 2000;

                        tcp_client.Connect(UR_Stream_Data.ip_address, UR_Stream_Data.port_number);
                        network_stream = tcp_client.GetStream();
                        UR_Stream_Data.is_alive = true;
                        Debug.Log("Telemetría Conectada (Puerto 30003)");
                    }

                    byte[] sizeBytes = new byte[4];
                    int bytesLeidos = 0;
                    while (bytesLeidos < 4)
                    {
                        int read = network_stream.Read(sizeBytes, bytesLeidos, 4 - bytesLeidos);
                        if (read == 0) throw new SocketException();
                        bytesLeidos += read;
                    }

                    byte[] sizeToRead = new byte[4];
                    Array.Copy(sizeBytes, sizeToRead, 4);
                    Array.Reverse(sizeToRead);
                    int packetSize = BitConverter.ToInt32(sizeToRead, 0);

                    if (packetSize < 100 || packetSize > 2000)
                    {
                        throw new Exception("Tamaño de paquete TCP corrupto");
                    }

                    byte[] packet = new byte[packetSize];
                    Array.Copy(sizeBytes, 0, packet, 0, 4);

                    bytesLeidos = 4;
                    while (bytesLeidos < packetSize)
                    {
                        int read = network_stream.Read(packet, bytesLeidos, packetSize - bytesLeidos);
                        if (read == 0) throw new SocketException();
                        bytesLeidos += read;
                    }

                    t.Start();
                    Array.Reverse(packet);

                    UR_Stream_Data.J_Orientation[0] = BitConverter.ToDouble(packet, packet.Length - 4 - (32 * offset));
                    UR_Stream_Data.J_Orientation[1] = BitConverter.ToDouble(packet, packet.Length - 4 - (33 * offset));
                    UR_Stream_Data.J_Orientation[2] = BitConverter.ToDouble(packet, packet.Length - 4 - (34 * offset));
                    UR_Stream_Data.J_Orientation[3] = BitConverter.ToDouble(packet, packet.Length - 4 - (35 * offset));
                    UR_Stream_Data.J_Orientation[4] = BitConverter.ToDouble(packet, packet.Length - 4 - (36 * offset));
                    UR_Stream_Data.J_Orientation[5] = BitConverter.ToDouble(packet, packet.Length - 4 - (37 * offset));

                    UR_Stream_Data.C_Position[0] = BitConverter.ToDouble(packet, packet.Length - 4 - (56 * offset));
                    UR_Stream_Data.C_Position[1] = BitConverter.ToDouble(packet, packet.Length - 4 - (57 * offset));
                    UR_Stream_Data.C_Position[2] = BitConverter.ToDouble(packet, packet.Length - 4 - (58 * offset));

                    UR_Stream_Data.C_Orientation[0] = BitConverter.ToDouble(packet, packet.Length - 4 - (59 * offset));
                    UR_Stream_Data.C_Orientation[1] = BitConverter.ToDouble(packet, packet.Length - 4 - (60 * offset));
                    UR_Stream_Data.C_Orientation[2] = BitConverter.ToDouble(packet, packet.Length - 4 - (61 * offset));

                    t.Stop();

                    if (t.ElapsedMilliseconds < UR_Stream_Data.time_step)
                    {
                        Thread.Sleep(UR_Stream_Data.time_step - (int)t.ElapsedMilliseconds);
                    }
                    t.Restart();
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Conexión de telemetría perdida. Intentando reconectar... (" + e.Message + ")");
                    if (tcp_client != null) tcp_client.Close();
                    UR_Stream_Data.is_alive = false;
                    Thread.Sleep(500);
                }
            }
        }

        public void Start()
        {
            exit_thread = false;
            robot_thread = new Thread(new ThreadStart(UR_Stream_Thread));
            robot_thread.IsBackground = true;
            robot_thread.Start();
        }

        public void Stop()
        {
            exit_thread = true;
            Thread.Sleep(100);
            if (robot_thread != null)
            {
                UR_Stream_Data.is_alive = false;
                robot_thread.Abort();
            }
        }

        public void Destroy()
        {
            exit_thread = true;
            if (tcp_client != null && tcp_client.Connected)
            {
                network_stream.Dispose();
                tcp_client.Close();
            }
            Thread.Sleep(100);
        }
    }

    class UR_Control
    {
        private Thread robot_thread = null;
        private bool exit_thread = false;
        private TcpClient tcp_client = null;
        private NetworkStream network_stream = null;

        public void UR_Control_Thread()
        {
            var t = new Stopwatch();

            while (exit_thread == false)
            {
                try
                {
                    // BLINDAJE: Movimos la conexión DENTRO del bucle para tener Auto-Reconexión en el Control también
                    if (tcp_client == null || !tcp_client.Connected)
                    {
                        tcp_client = new TcpClient();
                        tcp_client.ReceiveTimeout = 2000;
                        tcp_client.SendTimeout = 2000;

                        tcp_client.Connect(UR_Control_Data.ip_address, UR_Control_Data.port_number);
                        network_stream = tcp_client.GetStream();
                        UR_Control_Data.is_alive = true;
                        Debug.Log("Control Conectado (Puerto 30002)");
                    }

                    t.Start();

                    // --- LÓGICA DE BUZÓN UNIVERSAL ---
                    if (UR_Control_Data.enviar_comando_personalizado == true)
                    {
                        byte[] binaryCmd = System.Text.Encoding.ASCII.GetBytes(UR_Control_Data.comando_personalizado);
                        network_stream.Write(binaryCmd, 0, binaryCmd.Length);
                        network_stream.Flush();

                        UR_Control_Data.enviar_comando_personalizado = false;
                    }
                    // --- MOVIMIENTO CONTINUO ---
                    else if (UR_Control_Data.joystick_button_pressed == true)
                    {
                        network_stream.Write(UR_Control_Data.command, 0, UR_Control_Data.command.Length);
                    }

                    t.Stop();
                    if (t.ElapsedMilliseconds < UR_Control_Data.time_step)
                    {
                        Thread.Sleep(UR_Control_Data.time_step - (int)t.ElapsedMilliseconds);
                    }
                    t.Restart();
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Error en hilo de control. Intentando reconectar... (" + e.Message + ")");
                    if (tcp_client != null) tcp_client.Close();
                    UR_Control_Data.is_alive = false;
                    Thread.Sleep(500);
                }
            }
        }

        public void Start()
        {
            exit_thread = false;
            robot_thread = new Thread(new ThreadStart(UR_Control_Thread));
            robot_thread.IsBackground = true;
            robot_thread.Start();
        }

        public void Stop()
        {
            exit_thread = true;
            Thread.Sleep(100);
            if (robot_thread != null)
            {
                UR_Control_Data.is_alive = false;
                robot_thread.Abort();
            }
        }

        public void Destroy()
        {
            exit_thread = true;
            if (tcp_client != null && tcp_client.Connected == true)
            {
                network_stream.Dispose();
                tcp_client.Close();
            }
            Thread.Sleep(100);
        }
    }

    public void BotonAccionarGripper(bool cerrar)
    {
        int pos = cerrar ? 255 : 0;

        string scriptGripper =
            "def gripper_move():\n" +
            "  socket_open(\"127.0.0.1\", 63352, \"rq\")\n" +
            "  socket_send_string(\"SET SPE 255\", \"rq\")\n" +
            "  socket_send_byte(10, \"rq\")\n" +
            "  socket_send_string(\"SET POS " + pos + "\", \"rq\")\n" +
            "  socket_send_byte(10, \"rq\")\n" +
            "  socket_close(\"rq\")\n" +
            "end\n";

        UR_Control_Data.comando_personalizado = scriptGripper;
        UR_Control_Data.enviar_comando_personalizado = true;

        UR_Control_Data.pinza_esta_cerrada = cerrar;
    }
}