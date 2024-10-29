// Purpose: Consumes messages from the gadget inventory queue.
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
class Program
{
  // Constants for connection and queue setup
  private const string HostName = "localhost";
  private const string ExchangeName = "order_exchange";
  private const string QueueName = "gadget_inventory_queue";
  

  static void Main(string[] args)
  {
    // Create connection and channel
    var factory = new ConnectionFactory() { HostName = HostName };
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();

    // Setup queue
    SetupQueue(channel);

    Console.WriteLine(" [*] Waiting for gadget orders. Press [Ctrl+C] to exit.");

    // Create consumer and receive messages
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
      var message = Encoding.UTF8.GetString(ea.Body.ToArray());
      Console.WriteLine($" [x] Received {message}");
    };

    channel.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);

    Console.WriteLine("Press [enter] to exit.");
    Console.ReadLine();
  }

  // Helper method to setup exchange and queue
  private static void SetupQueue(IModel channel)
  {
    channel.ExchangeDeclare(exchange: ExchangeName, type: "headers");
    channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
    
    // Bind queue to exchange with header matching for gadget orders
    channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: "", arguments: new System.Collections.Generic.Dictionary<string, object>
    {
      { "x-match", "any" },
      { "order_type", "gadget" }
    });
  }
}
