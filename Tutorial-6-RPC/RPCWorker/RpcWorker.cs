using System;
using System.IO;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutoriales_RabbitMQ.Tutorial_6.RPCWorker
{
    class RpcWorker
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
                        channel.QueueDeclare(queue: "rpc_queue",
                                            durable: false,
                                            exclusive: false,
                                            autoDelete: false,
                                            arguments: null);
                        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) => {
                            string response = null;

                            var body = ea.Body;
                            var props = ea.BasicProperties;
                            var replyProps = channel.CreateBasicProperties();
                            replyProps.CorrelationId = props.CorrelationId;

                            try
                            {
                                var message = Encoding.UTF8.GetString(body);
                                int n = int.Parse(message);
                                Console.WriteLine("==> Calculando fib({0})", message);
                                response = fib(n).ToString();
                            } catch (Exception ex) {
                                Console.WriteLine("Error: " + ex.Message);
                                response = "";
                            } finally {
                                var responseBytes = Encoding.UTF8.GetBytes(response);
                                channel.BasicPublish(
                                    exchange: "",
                                    routingKey: props.ReplyTo,
                                    basicProperties: replyProps,
                                    body: responseBytes
                                );
                                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                            }
                        };


                        channel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consumer);
                        Console.WriteLine("Esperando solicitudes RPC... Presiona [Enter] para finalizar.");
                        Console.ReadLine();
                    }
                }
            } catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException) {
                Console.WriteLine("# Error: No pudo establecerse una conexión con el servidor RabbitMQ. Finalizando la aplicación.");
            }
        }

        private static int fib(int n)
        {
            if (n == 0 || n == 1)
                return n;
            
            return fib(n - 1) + fib(n - 2);
        }
    }
}
