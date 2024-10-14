using System.Threading;
using System.Threading.Tasks;

namespace GHent.Shared.Request
{
    public interface IRequestProcessor
    {
        Task<string> Download(IRequest request, CancellationToken cancellationToken);
    }
}