namespace KH.Lab.NomadLab;

public interface INomadService
{
    Task CreateVariableAsync(string path, Dictionary<string, string> variables);
    Task<Dictionary<string, string>?> GetVariableAsync(string path);
    Task<List<string>> GetAllVariablesAsync();
    Task UpdateVariableAsync(string path, Dictionary<string, string> variables);
    Task DeleteVariableAsync(string path);
}
