using Microsoft.AspNetCore.Identity.UI.Services;

namespace BookDragon.Services.Interfaces
{
    public interface IEmailService : IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendEmailAsync(string email, string subject, string htmlMessage, string plainTextMessage);
    }
}