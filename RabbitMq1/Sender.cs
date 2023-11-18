using System;
using System.Text;
using RabbitMQ.Client;

class EmailPublisher
{
    static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Declare the exchange (topic)
            channel.ExchangeDeclare(exchange: "email_topic", type: ExchangeType.Topic);

            while (true)
            {
                Console.Write("Enter recipient email addresses separated by commas (or 'exit' to quit): ");
                string recipientEmailsInput = Console.ReadLine();

                if (recipientEmailsInput.ToLower() == "exit")
                {
                    break;
                }

                Console.Write("Enter email subject: ");
                string subject = Console.ReadLine();

                Console.Write("Enter email body: ");
                string body = Console.ReadLine();

                // Split email addresses and publish a message for each recipient
                string[] recipientEmails = recipientEmailsInput.Split(',');
                foreach (var recipientEmail in recipientEmails)
                {
                    // Format the email message
                    string emailMessage = $"{subject}\n\n{body}";

                    // Publish the email to the topic with routing key as the email address
                    var bodyBytes = Encoding.UTF8.GetBytes(emailMessage);
                    channel.BasicPublish(exchange: "email_topic", routingKey: recipientEmail.Trim(), basicProperties: null, body: bodyBytes);

                    Console.WriteLine($"Email sent to {recipientEmail.Trim()}");
                }
            }
        }
    }
}
