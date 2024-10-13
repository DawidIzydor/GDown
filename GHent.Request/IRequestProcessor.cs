using GHent.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GHent.RequestProcessor
{
    public interface IRequestProcessor
    {
        Task<string> Download(IRequest request, CancellationToken cancellationToken);
    }
}