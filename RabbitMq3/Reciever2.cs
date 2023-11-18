using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class EmailConsumer
{
    static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Declare the exchange (topic)
            channel.ExchangeDeclare(exchange: "email_topic", type: ExchangeType.Topic);

            // Declare a unique queue for each receiver
            //var queueName = channel.QueueDeclare().QueueName;

            // Declare a common queue
            var queueName = "common_email_queue";
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);


            // Bind the queue to the exchange with a routing key (email address)
            channel.QueueBind(queue: queueName, exchange: "email_topic", routingKey: "#");

            Console.WriteLine("Waiting for emails. To exit press Ctrl+C");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var emailMessage = Encoding.UTF8.GetString(body);
                var recipientEmail = ea.RoutingKey;

                // Process the email message (e.g., send email)
                SendEmail(recipientEmail, emailMessage);

                Console.WriteLine($"Email sent to {recipientEmail}");
            };

            // Start consuming messages from the queue
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            Console.ReadLine(); // Keep the application running
        }
    }

    static void SendEmail(string recipientEmail, string emailMessage)
    {
        // Configure your SMTP settings
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("$$$", "nocr cixc kvxi gyal"),
            EnableSsl = true,
        };

        // Create and send the email
        var mail = new MailMessage("$$$", recipientEmail)
        {
            Subject = "Email from RabbitMQ",
            Body = emailMessage,
            IsBodyHtml = false
        };

        smtpClient.Send(mail);
    }
}