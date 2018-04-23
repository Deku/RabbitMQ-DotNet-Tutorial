using System;
using System.IO;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Tutoriales_RabbitMQ.Tutorial_2.NewTask
{
    /// <summary>
    /// Envía el mensaje recibido como argumento en la línea de comandos, o en su defecto "Hello world!".
    /// Ejemplo: dotnet run Este es un trabajo... que demora.. 5 segundos
    /// </summary>
    class NewTask
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
                        channel.QueueDeclare(queue: "task_queue",
                                            durable: true,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);
                        string message = GetMessage(args);
                        var body = Encoding.UTF8.GetBytes(message);
                        var properties = channel.CreateBasicProperties();
                        properties.Persistent = true;

                        channel.BasicPublish(exchange: "",
                                            routingKey: "task_queue",
                                            basicProperties: properties,
                                            body: body);
                        Console.WriteLine("<== Enviado  {0}", message);
                    }
                }
            } catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException) {
                Console.WriteLine("# Error: No pudo establecerse una conexión con el servidor RabbitMQ. Finalizando la aplicación.");
            }
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? String.Join(" ", args) : "Hello world!");
        }
    }
}
