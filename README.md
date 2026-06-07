# Sistema de Control en Realidad Mixta para Brazo Robótico UR3

Este repositorio contiene el código fuente, la aplicación ejecutable y la documentación del proyecto de grado para la automatización y control de un brazo colaborativo Universal Robots UR3 mediante Realidad Mixta, orientado a la manipulación segura de contenedores en entornos de laboratorio clínico.

## 🛠️ Tecnologías y Herramientas
* **Motor Gráfico:** Unity 3D
* **Hardware Inmersivo:** Meta Quest 3 / 3S (Meta Interaction SDK)
* **Robótica:** Brazo Colaborativo UR3 (Universal Robots)
* **Lenguaje:** C# (.NET)
* **Comunicación:** Sockets TCP/IP asíncronos nativos.

## 🚀 Características del Sistema
* **Gemelo Digital Sincronizado:** Renderizado en tiempo real del estado articular del robot a través de telemetría a 50 Hz.
* **Teleoperación Segura:** Control de trayectorias e interacción con objetos virtuales que comandan los motores físicos del robot.
* **Interfaz de Telemetría (UI):** Visualización en el entorno virtual de la posición cartesiana y configuración de las articulaciones.
* **Sistema de Seguridad (E-Stop):** Protocolo de paro de emergencia que desacelera los motores, cancela trayectorias y bloquea los sockets ante anomalías.

## ⚠️ Alcance y Pruebas
Por normativas de bioseguridad, el sistema está diseñado para mitigar riesgos en la manipulación de muestras con formaldehído, pero **las pruebas físicas y de validación de este repositorio se ejecutaron en un entorno controlado utilizando objetos geométricos de referencia y contenedores vacíos**, priorizando la calibración de control, teleoperación y repetibilidad unidimensional.

## 📂 Estructura del Repositorio
* `/Unity_Project`: Proyecto completo listo para abrir en Unity.
* `/Scripts_Principales`: Archivos C# clave (Sincronización de eslabones, TCP/IP, UI, etc.).
* `/Build_APK`: Aplicación compilada para instalación directa (*sideloading*) en visores Meta Quest.
* `/Docs`: Manuales de usuario y diagramas de flujo algorítmico.

## ⚙️ Instalación y Uso
1. Instalar la aplicación `.apk` contenida en `/Build_APK` en el visor mediante SideQuest o Meta Quest Developer Hub.
2. Conectar el visor y el controlador del robot UR3 a la misma red local (LAN).
3. Iniciar la aplicación e ingresar la dirección IPv4 del controlador físico a través del teclado virtual inmersivo.

## 📄 Licencia
Este proyecto de software se distribuye bajo la licencia **MIT**. Consulta el archivo `LICENSE` para más detalles. 
*(Nota: El documento escrito de la tesis se rige bajo una licencia Creative Commons CC BY-NC-SA 4.0).*
