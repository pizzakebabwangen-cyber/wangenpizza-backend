using AutoMapper.Internal;
using WangenPizza.Helper;

namespace WangenPizza.Interfaces
{
    public interface IMailService
    {

        Task SendEmailAsync(MailRequest mailRequest, CancellationToken cancellationToken = default);
    }
}
