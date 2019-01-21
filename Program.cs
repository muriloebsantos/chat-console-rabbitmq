using System;
using EasyNetQ;
using EasyNetQ.Topology;
using System.Text;
using EasyNetQ.Management.Client;
using Newtonsoft.Json;
using System.Threading;
using System.Linq;

namespace ChatRoom
{
    class Program
    {
        private static readonly Guid _user = Guid.NewGuid();
        private static ManagementClient _client;

        // ex: amqp://nmclidrm:loremipson@shark.rmq.cloudamqp.com/nmclidrm
        const string urlAmqp = "";

        const string password = "";


        static void Main(string[] args)
        {
            _client = new ManagementClient("shark.rmq.cloudamqp.com", "nmclidrm", password);

            using(var bus = RabbitHutch.CreateBus(urlAmqp).Advanced) 
            {
                System.AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                {
                    CleanUp();
                };

                Console.CancelKeyPress += (a, b) =>
                {
                    CleanUp();
                };

                var exchange = bus.ExchangeDeclare("exchange", ExchangeType.Fanout);
                var queue = bus.QueueDeclare(_user.ToString());

                bus.Bind(exchange, queue, "");

                Console.WriteLine("Digite seu nome para entrar no chat: ");
                var userName = Console.ReadLine();

               new Thread(() => {

                   bus.Consume(queue, (body, props, info) => {
                       var response = Encoding.UTF8.GetString(body);
                       var message = JsonConvert.DeserializeObject<Message>(response);
                       PrintMessage(message);
                   });

               }).Start();
               

               Console.WriteLine("Digite sua mensagem");

               while(true)
               {
                   var msg = Console.ReadLine();

                   ClearLine();

                   var message = new Message(
                       userId: _user,
                       userName: userName,
                       content: msg);
                   
                   var json = JsonConvert.SerializeObject(message);
                   var body = Encoding.UTF8.GetBytes(json);
                   bus.Publish(exchange, "", false, new MessageProperties(), body);
               }
            }
        }

        private static void PrintMessage(Message message) 
        {
            if(string.IsNullOrWhiteSpace(message.Content)) return;

            if(message.UserId == _user)
              PrintMyMessage(message);
            else
              PrintOtherMessage(message);
        }

        private static void PrintOtherMessage(Message message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{message.UserName}: {message.Content}");
            Console.ResetColor();
        }

        private static void PrintMyMessage(Message message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Você: {message.Content}");
            Console.ResetColor();
        }

        private static void ClearLine() 
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

          private static void CleanUp()
        {
            var deleteQueue = _client.GetQueuesAsync().Result.FirstOrDefault(x => x.Name == _user.ToString());
            _client.DeleteQueueAsync(deleteQueue).Wait();
        }

    }
}
