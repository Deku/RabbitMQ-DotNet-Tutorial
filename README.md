# RabbitMQ-DotNet-Tutorial

Ejercicios mostrados en los (tutoriales de RabbitMQ)[https://www.rabbitmq.com/getstarted.html], utilizando .Net Core.

# Instrucciones

Para ejecutar los ejemplos necesitarás tener instalado .Net SDK, con la versión de .Net Core 2.0. Ver instalación para (Windows)[https://www.microsoft.com/net/learn/get-started/windows] / (Linux)[https://www.microsoft.com/net/learn/get-started/linux/rhel] / (MacOS)[https://www.microsoft.com/net/learn/get-started/macos].

También necesitarás un servidor que corra el servicio de RabbitMQ, ya sea de (forma local)[https://www.rabbitmq.com/download.html] o en la nube con (CloudAMQP)[https://www.cloudamqp.com/].

1. Cambia la extensión del archivo `rabbitmq.config.json.dist` a `rabbitmq.config.json`.
2. Configura la conexión a tu servidor en el archivo `rabbitmq.config.json`, esta será compartida en todos los proyectos dentro de este repositorio. El archivo `RabbitMQConfig.cs` es una clase que nos servirá como contrato para poder cargar la configuración dentro del código.
3. Cada tutorial posee su carpeta independiente, y dentro de esta se encuentran los proyectos de ejemplo. Abre el *Símbolo de sistema* o *Terminal* y navega hasta la carpeta del proyecto que desees ejecutar.
4. Ejecuta el comando `dotnet run` para ejecutar la aplicación.