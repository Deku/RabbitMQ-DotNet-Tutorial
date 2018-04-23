using System;
using System.IO;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutoriales_RabbitMQ.Tutorial_1.Receive
{
    /// <summary>
    /// Escucha los mensajes enviados a la cola llamada "hello" y los muestra en pantalla.
    /// </summary>
    class Receive
    {
        static void Main(string[] args)
        {
            RabbitMQConfig config;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\rabbitmq.config.json";
            
            try{
                config = JsonConvert.DeserializeObject<RabbitMQConfig>(File.ReadAllText(path));
            } catch {
                Console.WriteLine("# Error: Archivo de configuración no encontrado.");
                return;
            }

            var factory = new ConnectionFactory() { HostName =  config.Hostname, 
                                                    UserName = config.Username, 
                                                    VirtualHost = config.VHost, 
                                                    Password = config.Password };
            try
            {
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        // Creamos la cola en caso de que no exista
                        channel.QueueDeclare(queue: "hello",
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

                        // Creamos el consumidor
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) => {
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);
                            Console.WriteLine("==> Recibido  {0}", message);
                        };

                        // Iniciamos la escucha
                        channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

                        Console.WriteLine("Escuchando mensajes desde RabbitMQ. Presiona [Enter] para finalizar.");
                        Console.ReadLine();
                    }
                }
            } catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException) {
                Console.WriteLine("# Error: No pudo establecerse una conexión con el servidor RabbitMQ. Finalizando la aplicación.");
            }
        }
    }
}
