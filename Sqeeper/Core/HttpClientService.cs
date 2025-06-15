namespace Sqeeper.Core;

public class HttpClientService
{
    public readonly HttpClient Instance;
    
    public HttpClientService()
    {
        Instance = new HttpClient();
        Instance.DefaultRequestHeaders.Add("User-Agent", "Sqeeper");
    }
}