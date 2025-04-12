using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SmartInventoryManagementSystem.Services;

public class EmailSender : IEmailSender
{
    private readonly string _brevoApiKey;

    public EmailSender(IConfiguration configuration)
    {
        _brevoApiKey = configuration["Brevo:ApiKey"]
                       ?? throw new ArgumentNullException("Brevo API Key is missing");
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        try
        {
            var from = new MailboxAddress("Smart Inventory Management System Default Sender", "karina.vetlugina@georgebrown.ca");
            var to = new MailboxAddress("", email);

            var msg = new MimeMessage();
            msg.From.Add(from);
            msg.To.Add(to);
            msg.Subject = subject;

            msg.Body = new BodyBuilder
            {
                TextBody = "Welcome to Smart Inventory Management System",
                HtmlBody = message
            }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp-relay.brevo.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("894a70001@smtp-brevo.com", "dLV7zGWyN2pPwATM");
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while sending email: {ex.Message}");
            throw;
        }
    }
}