using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Tutoriales_RabbitMQ.Tutorial_5.EmitLogTopic
{
    class EmitLogTopic
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
                        // Declaramos el exchange por si no existe
                        channel.ExchangeDeclare("topic_log", "topic");

                        // Obtenemos la severidad (binding key)
                        var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";

                        var message = GetMessage(args);
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: "topic_log",
                                            routingKey: routingKey,
                                            basicProperties: null,
                                            body: body);
                        Console.WriteLine("<== Enviado  {0}:{1}", routingKey, message);
                    }
                }
            } catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException) {
                Console.WriteLine("# Error: No pudo establecerse una conexión con el servidor RabbitMQ. Finalizando la aplicación.");
            }
        }

        public static string GetMessage(string[] args)
        {
            return (args.Length > 1) ? string.Join(" ", args.Skip(1).ToArray()) : "Hello world!";
        }
    }
}
