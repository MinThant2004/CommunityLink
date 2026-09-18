using System.Threading;
using System.Threading.Tasks;

namespace CommunityLink.Domain.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string bodyHtml, CancellationToken cancellationToken = default);
}
