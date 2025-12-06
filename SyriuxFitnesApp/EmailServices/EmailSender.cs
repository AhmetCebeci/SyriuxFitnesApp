using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace SyriuxFitnesApp.EmailServices 
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //Sistem mail attığını sansın diye boş bıraktık burayı
            return Task.CompletedTask;
        }
    }
}