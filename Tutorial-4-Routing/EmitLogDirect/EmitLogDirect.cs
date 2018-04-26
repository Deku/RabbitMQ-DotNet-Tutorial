using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Tutoriales_RabbitMQ.Tutorial_4.EmitLogDirect
{
    /// <summary>
    /// Envía un mensaje al exchange indicando como filtro (cola) la severidad indicada
    /// Ejemplo: dotnet run error "Este es un mensaje de error"
    /// </summary>
    class EmitLogDirect
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
                        channel.ExchangeDeclare("direct_log", "direct");

                        // Obtenemos la severidad (binding key)
                        var severity = (args.Length > 0) ? args[0] : "info";

                        var message = GetMessage(args);
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: "direct_log",
                                            routingKey: severity,
                                            basicProperties: null,
                                            body: body);
                        Console.WriteLine("<== Enviado  {0}", message);
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
