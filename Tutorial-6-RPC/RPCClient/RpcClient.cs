using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Tutoriales_RabbitMQ.Tutorial_6.RPCClient
{
    class RpcClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;

        public RpcClient()
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
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                replyQueueName = channel.QueueDeclare().QueueName;
                var correlationId = Guid.NewGuid().ToString();
                consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) => {
                    var body = ea.Body;
                    var response = Encoding.UTF8.GetString(body);

                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        respQueue.Add(response);
                    }
                };

                props = channel.CreateBasicProperties();
                props.ReplyTo = replyQueueName;
                props.CorrelationId = correlationId;
            } catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException) {
                Console.WriteLine("# Error: No pudo establecerse una conexión con el servidor RabbitMQ. Finalizando la aplicación.");
            }
        }

        public string Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: props,
                body: messageBytes
            );

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true
            );

            return respQueue.Take();
        }

        public void Close()
        {
            connection.Close();
            connection.Dispose();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var rpcClient = new RpcClient();

            Console.WriteLine("<== Solicitando fib(30)");
            var response = rpcClient.Call("30");

            Console.WriteLine("==> Recibido  '{0}'", response);
            rpcClient.Close();
        }
    }
}
