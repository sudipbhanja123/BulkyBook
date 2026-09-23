namespace BulkyBook.Business.Services.IServices
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent);
        Task<bool> SendEmailConfirmationEmailAsync(string toEmail, int orderId, decimal orderToal);
    }
}
