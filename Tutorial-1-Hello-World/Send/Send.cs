using System;
using System.Text;
using System.IO;
using System.Reflection;

using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Tutoriales_RabbitMQ.Tutorial_1.Send
{
    
    /// <summary>
    /// Envía un mensaje a la cola llamada "hello".
    /// </summary>
    class Send
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
                        // Declaramos la cola en caso de que no exista
                        channel.QueueDeclare(queue: "hello",
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);

                        // Creamos el mensaje
                        string message = "Hello World!";
                        var body = Encoding.UTF8.GetBytes(message);

                        // Publicamos el mensaje
                        channel.BasicPublish(exchange: "",
                                            routingKey: "hello",
                                            basicProperties: null,
                                            body: body);
                        Console.WriteLine("<== Enviado  {0}", message);
                    }
                }
            } catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException) {
                Console.WriteLine("# Error: No pudo establecerse una conexión con el servidor RabbitMQ. Finalizando la aplicación.");
            }
        }
    }
}
