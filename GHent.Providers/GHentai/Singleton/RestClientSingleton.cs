using RestSharp;

namespace GHent.GHentai.Singleton
{
    internal static class RestClientSingleton
    {
        public static RestClient Instance { get; } = new RestClient();
    }
}
