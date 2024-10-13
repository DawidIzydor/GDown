using System;
using System.Threading;
using System.Threading.Tasks;

namespace GHent.Models
{
    public interface IRequestProcessor
    {
        Task<string> Download(IRequest request, CancellationToken cancellationToken);
    }
}