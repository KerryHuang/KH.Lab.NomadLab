# Nomad 介紹與安裝

Nomad 是 HashiCorp 開發的一款分散式的工作負載編排系統，用於管理和部署容器化和非容器化的應用程序。它類似於 Kubernetes，但比 Kubernetes 更簡單，並且支持更多類型的工作負載，不僅僅是容器。Nomad 可以與其他 HashiCorp 工具（如 Consul 和 Vault）無縫整合，提供服務發現和安全管理等功能。以下是 Nomad 的一些核心特點：

1. **多種工作負載支持**：Nomad 支持多種工作負載類型，包括 Docker 容器、可執行文件、Java 應用程序和虛擬機等，這使得它比專注於容器的 Kubernetes 更靈活。
2. **簡單性和高效性**：Nomad 的架構和操作比起 Kubernetes 更加簡單，適合需要輕量級解決方案的情境。它的核心設計遵循簡單性和高效性原則，可以輕鬆地擴展到數千個節點。
3. **多區域支持**：Nomad 支持多區域部署，使其在多地理位置上運行應用變得更簡單，適合高可用性需求。
4. **與 Consul 和 Vault 整合**：Nomad 可以整合 HashiCorp Consul 來進行服務發現和健康檢查，並整合 HashiCorp Vault 來管理安全憑證和機密信息。
5. **彈性擴展**：Nomad 可以水平擴展和動態分配資源，根據需求分配和調整工作負載。

Nomad 適合那些需要輕量級、高性能的工作負載編排解決方案的企業和團隊，並且如果你已經使用其他 HashiCorp 工具，整合 Nomad 會讓整體基礎設施更加一致和便於管理。

在 Docker 上安裝 Nomad 的過程相對簡單。可以利用官方的 Docker 映像來啟動 Nomad。以下是安裝和配置 Nomad 的步驟：

### 步驟 1：下載 Nomad Docker 映像

首先，從 Docker Hub 上下載 HashiCorp 提供的 Nomad 官方映像：

```bash
docker pull hashicorp/nomad
```

### 步驟 2：運行 Nomad 容器

啟動 Nomad 容器時，可以選擇啟動 Dev 模式（適合測試和開發環境），或以伺服器模式和客戶端模式來運行 Nomad。這裡的範例展示如何以 Dev 模式來運行 Nomad，這會啟動一個單節點的 Nomad：

```bash
docker run --name nomad-dev -d --network host hashicorp/nomad agent -dev
```

這個命令將啟動一個 Dev 模式的 Nomad 容器，它會在單一節點上運行 Nomad Server 和 Client，並開啟 Nomad 的 HTTP API 介面。`--network host` 允許 Nomad 在容器內部和外部進行通信。

### 步驟 3：檢查 Nomad 運行狀態

運行以下命令查看 Nomad 容器的狀態，確認 Nomad 已成功啟動：

```bash
docker ps
```

這會列出所有正在運行的容器，找到 `hashicorp/nomad` 的容器，並確認它的狀態是 `Up`。

### 步驟 4：訪問 Nomad Web 界面（可選）

如果希望使用 Nomad 的 Web 界面，默認情況下可以通過 `localhost:4646` 訪問它。打開瀏覽器並輸入以下 URL：

```arduino
http://localhost:4646
```

這會顯示 Nomad 的 Web 界面，可以用來查看集群狀態、作業等。

### 使用 Nomad Docker 驅動運行工作負載

如果需要讓 Nomad 管理其他 Docker 容器，可以通過定義 Nomad 作業文件，並指定 `driver = "docker"`，讓 Nomad 啟動和管理容器。

### Nomad Dev 模式之外的配置（生產環境）

若要在生產環境中使用，可以使用 Nomad Server 和 Client 分別運行在多個容器中，並設定集群配置。這樣可以提高容錯性和性能，並允許更高效的資源調度。

這些步驟可以讓你快速在 Docker 中安裝並運行 Nomad，並進行基本的測試和開發。

---

## 建立 `docker-compose.yml` 文件

以下是使用 Docker Compose 來建置 Nomad 環境的範例。此範例會啟動一個 Nomad Server 和一個 Nomad Client，並讓它們互相通信。這樣的配置適合本地開發和測試，但在生產環境中應進行額外的配置。

### 1. 建立 `docker-compose.yml` 文件

在你的專案目錄中，創建一個 `docker-compose.yml` 文件，並將以下內容添加進去：

```yaml
version: "3.8"

services:
  nomad-server:
    image: hashicorp/nomad:latest
    command: agent -server -bootstrap-expect=1 -data-dir=/nomad/data -bind=0.0.0.0
    volumes:
      - nomad-server-data:/nomad/data
    networks:
      - nomad-network
    ports:
      - "4646:4646"   # HTTP API
      - "4647:4647"   # RPC
      - "4648:4648"   # Serf
    environment:
      - NOMAD_BIND_ADDR=0.0.0.0

  nomad-client:
    image: hashicorp/nomad:latest
    command: agent -client -data-dir=/nomad/data -bind=0.0.0.0 -network-interface=eth0
    volumes:
      - nomad-client-data:/nomad/data
    networks:
      - nomad-network
    depends_on:
      - nomad-server
    environment:
      - NOMAD_BIND_ADDR=0.0.0.0
      - NOMAD_SERVERS=nomad-server:4647   # 指定 Nomad Server 地址

volumes:
  nomad-server-data:
  nomad-client-data:

networks:
  nomad-network:
    driver: bridge
```

### 配置說明：

- **Nomad Server**:
  - 使用 `hashicorp/nomad:latest` 映像。
  - 運行 Nomad Server，並設置 `-bootstrap-expect=1`，表示只有一個 Nomad Server（適合單節點測試環境）。
  - 開放 4646、4647 和 4648 端口，以支持 HTTP API、RPC 和 Serf 通信。
  - 使用 Docker Volume 來持久化數據，以避免容器重啟後數據丟失。
- **Nomad Client**:
  - 使用 `hashicorp/nomad:latest` 映像。
  - 運行 Nomad Client，並將 `NOMAD_SERVERS` 環境變量設置為 `nomad-server:4647`，使其能夠連接到 Nomad Server。
  - 使用 Volume 來存儲數據。

### 2. 啟動 Nomad 容器

在包含 `docker-compose.yml` 文件的目錄中運行以下命令來啟動服務：

```bash
docker-compose up -d
```

這將會在後台啟動 Nomad Server 和 Client。`-d` 參數表示以分離模式（detached mode）運行。

### 3. 檢查 Nomad 狀態

使用以下命令查看 Nomad Server 和 Client 容器的運行狀態：

```bash
docker-compose ps
```

你應該會看到 `nomad-server` 和 `nomad-client` 容器都在運行。

### 4. 訪問 Nomad 的 Web UI

在瀏覽器中訪問以下地址以打開 Nomad 的 Web UI：

```arduino
http://localhost:4646
```

你將能夠查看 Nomad 集群的狀態和運行的任務。

### 5. 運行 Nomad 作業

要提交作業，可以在本地安裝 Nomad CLI 工具，並在 `docker-compose` 的 Nomad 配置上提交。也可以通過 Nomad Web UI 添加新的作業。

這樣的 Docker Compose 設定適合本地開發和測試 Nomad 的功能。在生產環境中，可以將 Nomad Server 配置成多節點集群並進行更進一步的網路和安全性配置。

---

在 .NET 8 的 Web API 中，您可以透過 Nomad 的 HTTP API 來設定和取得變數（Variables）。Nomad 提供了 `/var` 和 `/vars` 端點，允許您與變數進行互動。 

[HashiCorp Developer](https://developer.hashicorp.com/nomad/api-docs/variables)

以下是如何在 .NET 8 Web API 中實現與 Nomad 變數的互動：

### 1. 安裝必要的 NuGet 套件

首先，確保您的專案已安裝 `System.Net.Http.Json` 套件，以便輕鬆地處理 JSON 內容。

```bash
dotnet add package System.Net.Http.Json
```

### 2. 創建 Nomad 客戶端服務

接著，創建一個服務類別來封裝與 Nomad API 的互動邏輯。

```csharp
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class NomadClient
{
    private readonly HttpClient _httpClient;

    public NomadClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 取得變數
    public async Task<NomadVariable?> GetVariableAsync(string path, string namespaceName = "default")
    {
        var response = await _httpClient.GetAsync($"/v1/var/{path}?namespace={namespaceName}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<NomadVariable>();
        }
        return null;
    }

    // 設定變數
    public async Task<bool> SetVariableAsync(string path, object data, string namespaceName = "default")
    {
        var response = await _httpClient.PutAsJsonAsync($"/v1/var/{path}?namespace={namespaceName}", data);
        return response.IsSuccessStatusCode;
    }
}

// Nomad 變數模型
public class NomadVariable
{
    public string Namespace { get; set; }
    public string Path { get; set; }
    public Dictionary<string, string> Items { get; set; }
}
```

### 3. 在 DI 容器中註冊 NomadClient

在 `Program.cs` 中，將 `NomadClient` 註冊到依賴注入容器，並設定基底地址。

```csharp
var builder = WebApplication.CreateBuilder(args);

// 註冊 NomadClient
builder.Services.AddHttpClient<NomadClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:4646"); // Nomad API 的地址
});

var app = builder.Build();

// 其他中介軟體和路由設定

app.Run();
```

### 4. 在控制器中使用 NomadClient

最後，在您的控制器中注入 `NomadClient`，並使用它來與 Nomad 的變數進行互動。

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class NomadVariablesController : ControllerBase
{
    private readonly NomadClient _nomadClient;

    public NomadVariablesController(NomadClient nomadClient)
    {
        _nomadClient = nomadClient;
    }

    [HttpGet("{path}")]
    public async Task<IActionResult> GetVariable(string path)
    {
        var variable = await _nomadClient.GetVariableAsync(path);
        if (variable == null)
        {
            return NotFound();
        }
        return Ok(variable);
    }

    [HttpPost("{path}")]
    public async Task<IActionResult> SetVariable(string path, [FromBody] Dictionary<string, string> data)
    {
        var success = await _nomadClient.SetVariableAsync(path, new { Items = data });
        if (!success)
        {
            return StatusCode(500, "Failed to set variable.");
        }
        return NoContent();
    }
}
```

### 注意事項

- **安全性**：在生產環境中，確保 Nomad API 的訪問是安全的，並且您的應用程式具有適當的權限來讀取和寫入變數。
- **錯誤處理**：在實際應用中，應加強錯誤處理，以應對可能的網路問題或 Nomad API 的錯誤回應。
- **配置**：將 Nomad 的 API 地址和命名空間等配置項目放入應用程式的配置文件中，以便於管理和修改。

透過上述步驟，您可以在 .NET 8 Web API 中實現與 Nomad 變數的設定和取得功能，從而更靈活地管理您的應用程式配置。