using System.Threading.Tasks;

namespace FigureShop.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
