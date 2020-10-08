using RestSharp;

namespace GHent.RequestProcessor.Singleton
{
    internal static class RestClientSingleton
    {
        public static RestClient Instance { get; } = new RestClient();
    }
}
