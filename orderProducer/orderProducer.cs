using System;
using System.Text;
using RabbitMQ.Client;

class Program
{
  // Exchange name and connection factory constants
  private const string ExchangeName = "order_exchange";
  private static readonly ConnectionFactory Factory = new ConnectionFactory() { HostName = "localhost" };

  static void Main(string[] args)
  {
    // Create connection and channel
    using var connection = Factory.CreateConnection();
    using var channel = connection.CreateModel();
    
    // Declare exchange (header based routing / content based routing)
    channel.ExchangeDeclare(exchange: ExchangeName, type: "headers");

    while (true)
    {
      string orderType = GetOrderType();
      if (orderType == null) continue;

      // Create message with order type and properties
      string message = $"Order for {orderType}";
      var body = Encoding.UTF8.GetBytes(message);
      var properties = CreateBasicProperties(channel, orderType);

      // Publish message and print to console
      channel.BasicPublish(exchange: ExchangeName, routingKey: "", basicProperties: properties, body: body);
      Console.WriteLine($" [x] Sent '{message}' with header 'order_type: {orderType}'");
    }
  }

  // Helper method for getting order type
  private static string GetOrderType()
  {
    Console.Write("Enter order type (widget/gadget): ");
    string orderType = Console.ReadLine()?.ToLower();
    if (orderType != "widget" && orderType != "gadget")
    {
      Console.WriteLine("Invalid order type. Please enter 'widget' or 'gadget'.");
      return null;
    }
    return orderType;
  }

  // Helper method for creating basic properties (order type header)
  private static IBasicProperties CreateBasicProperties(IModel channel, string orderType)
  {
    var properties = channel.CreateBasicProperties();
    properties.Headers = new System.Collections.Generic.Dictionary<string, object>
    {
      { "order_type", orderType }
    };
    return properties;
  }
}
