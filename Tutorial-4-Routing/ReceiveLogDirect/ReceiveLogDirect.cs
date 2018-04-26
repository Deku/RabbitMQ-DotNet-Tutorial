using System;
using System.IO;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutoriales_RabbitMQ.Tutorial_4.ReceiveLogDirect
{
    /// <summary>
    /// Suscribe a uno o más niveles de log (colas) y los imprime en pantalla.
    /// Ejemplo: dotnet run info warning error
    /// </summary>
    class ReceiveLogDirect
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: dotnet run [info] [warning] [error]");
                Console.WriteLine("Presione [Enter] para finalizar.");
                Console.ReadLine();
                Environment.ExitCode = 1;
                return;
            }

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
                        // Declaramos el exchange en caso de que no exista
                        channel.ExchangeDeclare(exchange: "direct_log", type: "direct");

                         // Declaramos una cola autogenerada y la enlazamos al exchange
                         // mediante las binding keys
                        var queueName = channel.QueueDeclare().QueueName;

                        foreach (var severity in args)
                        {
                            channel.QueueBind(queue: queueName, exchange: "direct_log", routingKey: severity);
                        }

                        // Creamos el consumidor
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) => {
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);
                            Console.WriteLine("==> Recibido  {0}: {1}", ea.RoutingKey, message);
                        };

                        // Iniciamos la escucha
                        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                        Console.WriteLine("Utilizando cola {0}", queueName);
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
