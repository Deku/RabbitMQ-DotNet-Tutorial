using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutoriales_RabbitMQ.Tutorial_2.Worker
{
    /// <summary>
    /// Simula 1 segundo de trabajo por cada punto (.) en el mensaje recibido.
    /// </summary>
    class Worker
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
                        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) => {
                            var body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);
                            Console.WriteLine("==> Recibido  {0}", message);

                            int dots = message.Split('.').Length - 1;
                            Thread.Sleep(dots * 1000);
                            Console.WriteLine("==> Tarea terminada!");
                            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        };

                        channel.BasicConsume(queue: "task_queue", autoAck: false, consumer: consumer);

                        Console.WriteLine("Escuchando mensajes desde RabbitMQ. Presione [Enter] para finalizar.");
                        Console.ReadLine();
                    }
                }
            } catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException) {
                Console.WriteLine("# Error: No pudo establecerse una conexión con el servidor RabbitMQ. Finalizando la aplicación.");
            }
        }
    }
}
