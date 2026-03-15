# Prompt para Codex — Implementação detalhada no Blazor existente

Você irá atuar sobre uma **aplicação Web Blazor já existente**. Sua tarefa é **implementar**, de forma incremental e organizada, um portal analítico e operacional para visualização de dados das camadas **Bronze, Silver e Gold**, além de informações de **MLflow**, **MinIO**, **health de dependências**, **jobs** e **perguntas ao gold**.

## 1. Objetivo principal

Implementar no projeto Blazor existente uma experiência completa de consulta e operação com os seguintes objetivos:

- exibir dados das camadas bronze, silver e gold;
- exibir dados gerais e operacionais do MLflow;
- exibir dados gerais e operacionais do MinIO;
- exibir status da API e das dependências externas;
- exibir e acompanhar jobs assíncronos;
- permitir perguntas em linguagem natural ao gold consolidado;
- permitir tabelas inteligentes com:
  - ocultação e exibição de colunas por checkbox;
  - ordenação por coluna;
  - filtros por contexto;
  - paginação;
  - persistência de preferências visuais.

A implementação deve ser orientada para **reuso**, **baixa duplicação**, **crescimento futuro do schema** e **integração incremental**.

---

## 2. Contexto funcional do backend existente

Considere que o sistema de origem possui:

- pipeline em camadas bronze, silver e gold;
- gold consolidado por volta;
- publicação de métricas, parâmetros e artefatos no MLflow;
- sincronização com data lake MinIO/S3;
- API FastAPI com endpoints como:
  - `GET /health`
  - `GET /health/dependencies`
  - `GET /gold/meetings`
  - `POST /gold/questions`
  - `POST /train/...`
  - `POST /driver-profiles`
  - `POST /driver-profiles/season`
  - `POST /import-season`
  - `POST /import-season/resume`
  - `POST /data-lake/sync`
  - `GET /jobs/{job_id}`
  - `GET /jobs/{job_id}/logs?lines=200`

O frontend Blazor deve atuar como camada de apresentação e consulta sobre essas capacidades.

---

## 3. Regra fundamental de implementação

**Não recrie a aplicação do zero.**

Você deve:

- analisar a estrutura atual do projeto Blazor existente;
- aproveitar layout, autenticação, componentes, services e padrões já existentes;
- adicionar apenas o que for necessário;
- manter naming consistente com o projeto existente;
- evitar quebrar páginas atuais;
- introduzir os módulos novos de forma incremental.

Se existir uma estrutura como:

- `Pages`
- `Components`
- `Services`
- `Models`
- `Shared`
- `Layout`

então siga essa organização. Se o projeto tiver outra convenção, adapte-se a ela.

---

## 4. Resultado esperado

Entregar código pronto no projeto Blazor com:

1. novas páginas;
2. componentes reutilizáveis;
3. DTOs e ViewModels;
4. serviços de integração HTTP;
5. estado de filtros e grid;
6. persistência de preferências visuais;
7. tratamento de loading, empty state e erro;
8. navegação integrada ao menu da aplicação;
9. código limpo e comentado apenas onde fizer sentido.

---

## 5. Módulos que devem ser implementados

Implemente os módulos abaixo.

### 5.1 Dashboard Executivo

Criar uma página de dashboard com visão resumida contendo:

- cards KPI para:
  - total bronze;
  - total silver;
  - total gold;
  - total de meetings;
  - total de sessões;
  - total de pilotos;
  - jobs em execução;
  - jobs concluídos;
  - jobs com falha;
  - total de runs recentes do MLflow;
  - total de artefatos relevantes;
- painel de health da aplicação;
- painel de health das dependências externas;
- painel com jobs recentes;
- painel com últimas operações relacionadas ao data lake/MinIO.

### 5.2 Catálogo de Dados

Criar uma página com abas:

- Bronze
- Silver
- Gold

Cada aba deve usar uma **grid inteligente reutilizável**.

### 5.3 Monitor de Jobs

Criar uma página para:

- listar jobs recentes;
- filtrar por status;
- filtrar por tipo;
- abrir detalhes;
- consultar logs;
- atualizar automaticamente.

### 5.4 Exploração de MLflow

Criar uma página para:

- listar execuções/runs;
- exibir métricas principais;
- exibir parâmetros principais;
- exibir artefatos;
- comparar runs.

### 5.5 Exploração de MinIO

Criar uma página para:

- listar buckets/prefixos/objetos de interesse;
- filtrar por camada bronze/silver/gold/artifacts;
- pesquisar por nome;
- exibir metadados básicos;
- permitir copiar URI ou abrir link quando aplicável.

### 5.6 Perguntas ao Gold

Criar uma página para:

- digitar pergunta;
- selecionar filtros opcionais;
- enviar ao endpoint do gold;
- exibir resposta em bloco legível;
- exibir contexto aplicado;
- manter histórico local de perguntas recentes.

---

## 6. Grid Inteligente — implementação obrigatória

A grid é o elemento central desta entrega.

Crie um componente reutilizável, por exemplo:

- `SmartDataGrid.razor`

E os componentes auxiliares necessários, por exemplo:

- `ColumnSelectorPanel.razor`
- `GridToolbar.razor`
- `SortSelector.razor`
- `PaginationBar.razor`

### 6.1 Capacidades obrigatórias da grid

A grid deve suportar:

- renderização por metadados de colunas;
- ocultar/exibir colunas por checkbox;
- ordenação por cabeçalho;
- ordenação via seletor opcional;
- filtro textual global;
- filtros específicos por coluna, quando aplicável;
- paginação;
- seleção de tamanho da página;
- loading state;
- empty state;
- error state;
- atualização de dados;
- persistência local da configuração visual.

### 6.2 Painel de colunas

Criar um painel lateral ou modal chamado `Colunas`, contendo:

- lista de todas as colunas da grid;
- um checkbox por coluna;
- campo de busca por nome da coluna;
- botão `Mostrar todas`;
- botão `Ocultar todas`;
- botão `Restaurar padrão`.

Regras:

- checkbox marcado = coluna visível;
- checkbox desmarcado = coluna oculta.

### 6.3 Ordenação

Regras de ordenação:

- 1º clique no cabeçalho: ascendente;
- 2º clique: descendente;
- 3º clique: remove ordenação.

Mostrar ícone visual no cabeçalho.

A grid deve ser preparada para dois modos:

- ordenação local;
- ordenação server-side.

### 6.4 Metadados de coluna

Crie um modelo como este, adaptando ao padrão do projeto:

```csharp
public sealed class GridColumnDefinition
{
    public string Key { get; set; } = default!;
    public string Title { get; set; } = default!;
    public bool Visible { get; set; } = true;
    public bool Sortable { get; set; } = true;
    public bool Filterable { get; set; } = true;
    public string DataType { get; set; } = "string";
    public string? Format { get; set; }
    public string? Width { get; set; }
    public string? CssClass { get; set; }
    public string? Alignment { get; set; }
}
```

Crie também modelos para:

- estado da grid;
- ordenação;
- paginação;
- preferências do usuário.

### 6.5 Persistência de preferências

Persistir pelo menos em `localStorage`:

- colunas visíveis;
- coluna de ordenação;
- direção;
- tamanho da página;
- última aba selecionada.

Criar um serviço, por exemplo:

- `IUserGridPreferenceService`
- `UserGridPreferenceService`

---

## 7. Estrutura sugerida de arquivos

Adapte ao projeto existente, mas idealmente crie algo próximo de:

```text
/Components
  /Grid
    SmartDataGrid.razor
    ColumnSelectorPanel.razor
    GridToolbar.razor
    PaginationBar.razor
  /Dashboard
    KpiCard.razor
    HealthStatusPanel.razor
    RecentJobsPanel.razor
  /Jobs
    JobStatusTable.razor
    JobDetailsPanel.razor
    JobLogsViewer.razor
  /Mlflow
    MlflowRunTable.razor
    MlflowRunComparisonTable.razor
  /Minio
    MinioObjectBrowser.razor
  /Gold
    GoldQuestionPanel.razor

/Pages
  Dashboard.razor
  DataCatalog.razor
  Jobs.razor
  Mlflow.razor
  Minio.razor
  GoldQuestions.razor

/Models
  /Grid
    GridColumnDefinition.cs
    GridQueryState.cs
    GridSortState.cs
    GridPaginationState.cs
    UserGridPreference.cs
  /Dashboard
    DashboardSummaryDto.cs
    HealthDependencyDto.cs
  /Catalog
    BronzeRowDto.cs
    SilverRowDto.cs
    GoldRowDto.cs
  /Jobs
    JobSummaryDto.cs
    JobDetailsDto.cs
    JobLogLineDto.cs
  /Mlflow
    MlflowRunDto.cs
    MlflowMetricDto.cs
    MlflowArtifactDto.cs
  /Minio
    MinioObjectDto.cs
  /Gold
    GoldQuestionRequestDto.cs
    GoldQuestionResponseDto.cs

/Services
  /Api
    IHealthApiService.cs
    HealthApiService.cs
    IDataCatalogApiService.cs
    DataCatalogApiService.cs
    IJobApiService.cs
    JobApiService.cs
    IMlflowApiService.cs
    MlflowApiService.cs
    IMinioApiService.cs
    MinioApiService.cs
    IGoldQuestionApiService.cs
    GoldQuestionApiService.cs
  /State
    IUserGridPreferenceService.cs
    UserGridPreferenceService.cs
```

Se a solução já possuir uma pasta `Features`, `Modules` ou `Application`, use o padrão atual.

---

## 8. DTOs e modelos que devem ser implementados

Implemente DTOs mínimos para suportar as telas.

### 8.1 Dashboard

Criar DTO agregado com campos como:

- BronzeCount
- SilverCount
- GoldCount
- MeetingsCount
- SessionsCount
- DriversCount
- RunningJobsCount
- CompletedJobsCount
- FailedJobsCount
- MlflowRunsCount
- ArtifactCount
- CheckedAt

### 8.2 Health

Criar DTOs para:

- status da API;
- status por dependência;
- latência;
- mensagens de degradação.

### 8.3 Catálogo Bronze

Criar DTO com colunas como:

- Season
- MeetingKey
- SessionKey
- DriverNumber
- SourceEndpoint
- FileName
- CreatedAt
- RecordCount
- StoragePath
- SyncStatus

### 8.4 Catálogo Silver

Criar DTO com colunas como:

- Season
- MeetingKey
- SessionKey
- DriverNumber
- DatasetName
- NormalizedAt
- SchemaVersion
- RecordCount
- NullPct
- StoragePath

### 8.5 Catálogo Gold

Criar DTO com os campos principais do gold:

- Season
- MeetingKey
- MeetingName
- MeetingDateStart
- SessionKey
- SessionName
- DriverNumber
- DriverName
- TeamName
- LapNumber
- LapDuration
- DurationSector1
- DurationSector2
- DurationSector3
- AvgSpeed
- MaxSpeed
- MinSpeed
- SpeedStd
- AvgRpm
- MaxRpm
- MinRpm
- RpmStd
- AvgThrottle
- MaxThrottle
- MinThrottle
- ThrottleStd
- FullThrottlePct
- BrakePct
- BrakeEvents
- HardBrakeEvents
- DrsPct
- GearChanges
- DistanceTraveled
- TrajectoryLength
- TrajectoryVariation
- TelemetryPoints
- TrajectoryPoints
- HasTelemetry
- HasTrajectory
- StintNumber
- Compound
- StintLapStart
- StintLapEnd
- TyreAgeAtStart
- TyreAgeAtLap
- TrackTemperature
- AirTemperature
- WeatherDate
- IsPitOutLap

### 8.6 Jobs

Criar DTOs para:

- JobId
- JobType
- Status
- CreatedAt
- StartedAt
- FinishedAt
- Duration
- Params
- Filters
- LogFile
- StatusFile
- LastMessage

### 8.7 MLflow

Criar DTOs para:

- ExperimentName
- RunId
- RunName
- Status
- StartTime
- EndTime
- Duration
- Metrics
- Parameters
- Artifacts

### 8.8 MinIO

Criar DTOs para:

- Bucket
- Prefix
- ObjectName
- ObjectType
- Size
- LastModified
- Layer
- StorageUri

### 8.9 Perguntas ao Gold

Criar DTOs para request/response:

```csharp
public sealed class GoldQuestionRequestDto
{
    public string Question { get; set; } = default!;
    public int? Season { get; set; }
    public string? SessionName { get; set; }
    public string? MeetingKey { get; set; }
    public int? DriverNumber { get; set; }
}
```

Criar resposta compatível com o contrato atual retornado pela API, inclusive com conteúdo textual e metadados quando disponíveis.

---

## 9. Serviços de integração HTTP

Implemente clientes HTTP separados por domínio.

### 9.1 Regras gerais

- usar `HttpClient` via DI;
- centralizar base URL em configuração;
- tratar timeout e erro;
- desserializar respostas JSON;
- retornar objetos fortes, nunca `dynamic`;
- encapsular detalhes HTTP fora das páginas.

### 9.2 Serviços mínimos

Crie interfaces e implementações para:

- `IHealthApiService`
- `IDataCatalogApiService`
- `IJobApiService`
- `IMlflowApiService`
- `IMinioApiService`
- `IGoldQuestionApiService`

### 9.3 Observação importante

Se ainda não existir endpoint backend específico para bronze, silver, gold, MLflow ou MinIO voltado ao frontend, implemente no Blazor uma camada preparada para contratos temporários/mockados e deixe pontos de extensão claros.

Ou seja:

- criar serviços com métodos reais;
- se algum endpoint ainda não existir, criar contratos e stubs controlados com TODO claro;
- não bloquear a evolução da UI por ausência pontual de backend.

---

## 10. Páginas e comportamento esperado

### 10.1 Página Dashboard

Implementar:

- cards KPI no topo;
- painel de health simples;
- painel de health das dependências;
- tabela resumida de jobs recentes;
- atualização manual por botão `Atualizar`;
- loading skeleton ou placeholder.

### 10.2 Página DataCatalog

Implementar:

- abas Bronze, Silver, Gold;
- filtros globais no topo;
- grid inteligente no centro;
- painel de colunas;
- exportação CSV opcional;
- persistência da última aba aberta.

Filtros sugeridos:

- season;
- meeting_key;
- meeting_name;
- session_name;
- driver_number;
- driver_name;
- team_name;
- constructor;
- período;
- has_telemetry;
- has_trajectory.

### 10.3 Página Jobs

Implementar:

- lista de jobs;
- filtro por status;
- filtro por tipo;
- auto refresh configurável;
- painel lateral ou inferior para detalhes;
- visualizador de logs com botão `Carregar mais linhas`.

### 10.4 Página MLflow

Implementar:

- grid de runs;
- seleção de runs para comparação;
- painel de métricas;
- painel de parâmetros;
- painel de artefatos;
- ordenação por métricas.

### 10.5 Página Minio

Implementar:

- grid de objetos;
- filtros por bucket, prefixo e camada;
- busca textual;
- ação de copiar URI;
- ação de abrir link quando aplicável.

### 10.6 Página GoldQuestions

Implementar:

- textarea ou input principal de pergunta;
- filtros opcionais;
- botão `Perguntar`;
- loading;
- card de resposta;
- histórico local das últimas perguntas.

---

## 11. Visualizações inteligentes obrigatórias

Além das grids, implemente visualizações resumidas úteis.

### 11.1 Qualidade do gold

Criar painel com:

- rows;
- null_pct;
- valid_laps;
- discarded_laps.

Se houver dados suficientes, exibir resumo por:

- temporada;
- meeting;
- sessão.

### 11.2 Ranking de pilotos

Criar uma visualização baseada em `driver_profiles.csv` ou endpoint equivalente, com métricas como:

- lap_mean;
- lap_std;
- anomaly_rate;
- finish_rate;
- points_total;
- meetings_total;
- degradation_mean;
- delta_pace_mean;
- rank_percentile_mean.

Se ainda não existir endpoint dedicado no backend do portal, deixe a estrutura pronta com serviço adaptável.

### 11.3 Comparação de modelos

Criar uma tabela de comparação para runs com métricas como:

- mae
- rmse
- r2
- mape
- accuracy
- precision
- recall
- f1
- roc_auc
- silhouette
- davies_bouldin

Destaque visual:

- melhor métrica por coluna, quando for possível determinar o melhor sentido;
- por exemplo, menor `rmse`, maior `r2`, maior `f1`, menor `davies_bouldin`.

---

## 12. UX e comportamento visual

A implementação deve seguir estas diretrizes:

- layout limpo;
- foco em leitura analítica;
- sem excesso de cores;
- badges de status com cores consistentes;
- ícones discretos;
- tooltips em campos técnicos;
- labels amigáveis nas telas;
- suporte a grande quantidade de colunas sem quebrar o layout;
- responsividade razoável.

Estados visuais obrigatórios:

- carregando;
- sem dados;
- erro de integração;
- parcial/degradado.

---

## 13. Navegação e menu

Atualize o menu do Blazor existente para incluir links para:

- Dashboard
- Catálogo de Dados
- Jobs
- MLflow
- MinIO
- Perguntas ao Gold

Não remova entradas existentes.

---

## 14. Registro no DI container

Registrar todos os novos serviços no container de injeção de dependência.

Exemplos:

```csharp
builder.Services.AddScoped<IHealthApiService, HealthApiService>();
builder.Services.AddScoped<IDataCatalogApiService, DataCatalogApiService>();
builder.Services.AddScoped<IJobApiService, JobApiService>();
builder.Services.AddScoped<IMlflowApiService, MlflowApiService>();
builder.Services.AddScoped<IMinioApiService, MinioApiService>();
builder.Services.AddScoped<IGoldQuestionApiService, GoldQuestionApiService>();
builder.Services.AddScoped<IUserGridPreferenceService, UserGridPreferenceService>();
```

Se a solução já tiver outro padrão de registro, respeite o padrão existente.

---

## 15. Configuração

Adicionar ou reutilizar configuração em `appsettings*.json` para:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:7077"
  }
}
```

Se necessário, separar por:

- API principal;
- MLflow;
- MinIO.

Mas prefira centralização simples se o backend já expuser tudo.

---

## 16. Critérios técnicos de aceite

A implementação só será considerada completa se atender aos itens abaixo:

1. existe uma grid reutilizável com checkbox de colunas;
2. é possível esconder e exibir colunas sem recarregar a página;
3. é possível ordenar por coluna;
4. a configuração de colunas fica persistida localmente;
5. há páginas separadas para Dashboard, Catálogo, Jobs, MLflow, MinIO e Perguntas ao Gold;
6. o Dashboard exibe status da API e dependências;
7. a página Jobs consulta status e logs;
8. a página GoldQuestions chama o endpoint de perguntas;
9. as páginas usam serviços e DTOs separados;
10. a solução está integrada ao projeto Blazor existente;
11. o código compila.

---

## 17. Estratégia de execução esperada pelo Codex

Execute em etapas:

### Etapa 1
- analisar estrutura atual do projeto;
- identificar layout, menu, padrão de DI, serviços e componentes reutilizáveis.

### Etapa 2
- criar modelos base da grid;
- criar `SmartDataGrid`;
- criar persistência de preferências.

### Etapa 3
- criar serviços HTTP;
- adicionar configuração;
- registrar DI.

### Etapa 4
- criar Dashboard;
- criar Catálogo de Dados;
- criar Jobs.

### Etapa 5
- criar MLflow;
- criar MinIO;
- criar Perguntas ao Gold.

### Etapa 6
- integrar ao menu;
- revisar estados visuais;
- validar compilação.

---

## 18. Restrições

- não remover funcionalidades já existentes;
- não trocar stack do projeto;
- não converter o projeto para outra tecnologia;
- não usar bibliotecas pesadas sem necessidade;
- não usar `dynamic` como solução principal;
- não acoplar UI diretamente a JSON cru.

---

## 19. Entrega final esperada do Codex

Ao concluir, entregue:

1. lista dos arquivos criados/alterados;
2. resumo do que foi implementado;
3. observações sobre endpoints inexistentes que precisem de backend complementar;
4. instruções rápidas para executar e testar.

---

## 20. Observações de domínio que devem influenciar a UI

Leve em consideração que:

- bronze = dado cru para auditoria e replay;
- silver = dado normalizado e padronizado;
- gold = dataset analítico por volta, pronto para modelagem;
- MLflow = tracking de execução, métricas, parâmetros e artefatos;
- MinIO = armazenamento de datasets e artefatos;
- jobs = operações assíncronas com logs e status;
- perguntas ao gold = interface em linguagem natural para o dataset consolidado.

A UI deve refletir esse papel de cada camada.

---

## 21. Prioridade máxima

Se precisar decidir onde investir mais esforço, priorize nesta ordem:

1. `SmartDataGrid`
2. `ColumnSelectorPanel`
3. `DataCatalog`
4. `Dashboard`
5. `Jobs`
6. `GoldQuestions`
7. `MLflow`
8. `MinIO`

---

## 22. Instrução final

Implemente a solução diretamente no projeto Blazor existente, com código de qualidade de produção, estrutura organizada, componentes reutilizáveis e foco em manutenção futura.
