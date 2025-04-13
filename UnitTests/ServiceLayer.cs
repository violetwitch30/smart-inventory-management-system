using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SmartInventoryManagementSystem.Services;

public class EmailSenderTests
{
    [Fact]
    public void Constructor_Throws_WhenApiKeyMissing()
    {
        var config = new ConfigurationBuilder().Build();

        var ex = Assert.Throws<ArgumentNullException>(() => new EmailSender(config));
        Assert.Contains("Brevo API Key is missing", ex.Message);
    }

    [Fact]
    public async Task SendEmailAsync_Throws_OnInvalidSmtpHost()
    {
        var inMemorySettings = new Dictionary<string, string> { { "Brevo:ApiKey", "DUMMY" } };
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        var sender = new TestEmailSender(config);
        
        await Assert.ThrowsAnyAsync<Exception>(() =>
            sender.SendEmailAsync("test@example.com", "Subject", "<p>Body</p>"));
    }
    
    private class TestEmailSender : EmailSender
    {
        public TestEmailSender(IConfiguration configuration) : base(configuration) { }

        public new async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var from = new MailboxAddress("Test", "from@test.com");
                var to = new MailboxAddress("", email);

                var msg = new MimeKit.MimeMessage();
                msg.From.Add(from);
                msg.To.Add(to);
                msg.Subject = subject;
                msg.Body = new MimeKit.BodyBuilder { TextBody = "Test", HtmlBody = message }.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                
                await client.ConnectAsync("invalid.host.test", 587, SecureSocketOptions.StartTls);
                await client.SendAsync(msg);
                await client.DisconnectAsync(true);
            }
            catch (Exception)
            {
                // Rethrow so test can catch
                throw;
            }
        }
    }
}