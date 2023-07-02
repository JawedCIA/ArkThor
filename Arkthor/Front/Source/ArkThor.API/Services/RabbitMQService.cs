using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using System.Text;
using RabbitMQ.Client;
using ArkThor.API.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ArkThor.API.Services
{
 
    public interface IRabbitMQService
    {
        void SendMessage(string message,string? queueName=null, string? exchangeName = null);
    }

    public class RabbitMQProducer : IRabbitMQService
    {
        private readonly string _RabbitMQConnection;
        private readonly IMapper _mapper;
        public RabbitMQProducer(IMapper mapper, IConfiguration config)
        {
            _mapper = mapper;
            _RabbitMQConnection = config.GetValue<string>("RabbitMQConnection");
        }

        //public void SendMessage(string message, string? queueName = null, string? exchangeName = null)
        //{
        //    var factory = new ConnectionFactory { HostName = _RabbitMQConnection };
        //    IConnection connection = null;
        //    try
        //    {

        //       // var factory = new ConnectionFactory { HostName = _RabbitMQConnection };
        //        connection = factory.CreateConnection();
        //        using (var channel = connection.CreateModel())
        //        {

        //            channel.QueueDeclare(queueName,
        //                 durable: false,
        //                 exclusive: false,
        //                 autoDelete: false,
        //                 arguments: null);

        //            var json = JsonConvert.SerializeObject(message);
        //            var messageBytes = Encoding.UTF8.GetBytes(json);
        //            // Create the message properties
        //            var properties = channel.CreateBasicProperties();
        //            properties.Persistent = true; // Set the message as persistent if required

        //            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: messageBytes);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle the exception or log the error message
        //    }
        //    finally
        //    {
        //        if (connection != null && connection.IsOpen)
        //            connection.Close();
        //    }
        //}


        public void SendMessage(string message, string queueName = null, string exchangeName = null)
        {
            var factory = new ConnectionFactory { HostName = _RabbitMQConnection };
            IConnection connection = null;
            try
            {
                connection = factory.CreateConnection();
                using (var channel = connection.CreateModel())
                {
                    if (!string.IsNullOrEmpty(exchangeName))
                    {
                        channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
                    }

                    if (!string.IsNullOrEmpty(queueName))
                    {
                        channel.QueueDeclare(queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    }

                    var messageBytes = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true; // Set the message as persistent if required

                    if (!string.IsNullOrEmpty(exchangeName))
                    {
                        channel.BasicPublish(exchange: exchangeName, routingKey: "", basicProperties: properties, body: messageBytes);
                    }
                    else
                    {
                        channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: messageBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log the error message
            }
            finally
            {
                if (connection != null && connection.IsOpen)
                    connection.Close();
            }
        }

    }



}
