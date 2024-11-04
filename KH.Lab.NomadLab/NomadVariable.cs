namespace KH.Lab.NomadLab;

// Nomad 變數模型
public class NomadVariable
{
    public required string Namespace { get; set; }
    public string Path { get; set; } = string.Empty;  // Variable 的路徑
    public Dictionary<string, string> Items { get; set; } = [];  // 存儲的鍵值數據
}
