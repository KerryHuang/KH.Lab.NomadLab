# 第七章 安裝Nomad

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