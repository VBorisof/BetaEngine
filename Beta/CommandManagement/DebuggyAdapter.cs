using System.Net.Http;
using System.Text;

namespace Beta.CommandManagement;

public class DebuggyAdapter
{
    private const string BaseUrl = "http://localhost:3000/api";
    private const string DataUlr = $"{BaseUrl}/data";

    private readonly HttpClient _httpClient = new();

    public void Push(object o)
    {
        // TODO: fix for AOT
        // var json = JsonSerializer.Serialize(o);
        var json = "{}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.PostAsync(DataUlr, content);
    }
}