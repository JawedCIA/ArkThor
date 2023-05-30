using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using System.Text;
using RabbitMQ.Client;
using ArkThor.API.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace ArkThor.API.Services
{
 
    public interface IRabbitMQService
    {
        void SendMessage<MessageInfo>(MessageInfo message,string queueName);
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

        public void SendMessage<MessageInfo>(MessageInfo message, string queueName)
        {
            try
            {

                var factory = new ConnectionFactory { HostName = _RabbitMQConnection };
                var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                channel.QueueDeclare(queueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
              
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                channel.Close();
            }
            catch (Exception ex)
            {

            }
        }
    }



}
