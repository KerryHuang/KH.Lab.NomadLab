namespace KH.Lab.NomadLab;

public class NomadService
{
    private readonly HttpClient _httpClient;

    public NomadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 建立 Variable
    public async Task<bool> CreateVariableAsync(string path, object data, string namespaceName = "default")
    {
        var response = await _httpClient.PutAsJsonAsync($"/v1/var/{path}?namespace={namespaceName}", data);
        return response.IsSuccessStatusCode;
    }

    // 單一取得 Variable
    public async Task<NomadVariable?> GetVariableAsync(string path, string namespaceName = "default")
    {
        var response = await _httpClient.GetAsync($"/v1/var/{path}?namespace={namespaceName}");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<NomadVariable>();
    }

    // 取得全部 Variables
    public async Task<List<NomadVariable>> GetAllVariablesAsync()
    {
        var response2 = await _httpClient.GetStringAsync("/v1/var");
        var response = await _httpClient.GetFromJsonAsync<List<string>>("/v1/var");
        if (response == null) return new List<NomadVariable>();

        var variables = new List<NomadVariable>();
        foreach (var path in response)
        {
            var variable = await GetVariableAsync(path);
            if (variable != null) variables.Add(variable);
        }
        return variables;
    }

    // 修改 Variable
    public async Task UpdateVariableAsync(string path, Dictionary<string, string> data, string namespaceName = "default")
    {
        var response = await _httpClient.PutAsJsonAsync($"/v1/var/{path}?namespace={namespaceName}", data);
        response.EnsureSuccessStatusCode();
    }

    // 刪除 Variable
    public async Task DeleteVariableAsync(string path, string namespaceName = "default")
    {
        var response = await _httpClient.DeleteAsync($"/v1/var/{path}?namespace={namespaceName}");
        response.EnsureSuccessStatusCode();
    }
}