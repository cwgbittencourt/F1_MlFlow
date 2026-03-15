using F1_MlFlow.Components;
using F1_MlFlow.Models.Common;
using F1_MlFlow.Services.Api;
using F1_MlFlow.Services.State;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.Configure<BackendDependencySettings>(builder.Configuration.GetSection("BackendDependencies"));
builder.Services.Configure<AdminSettings>(builder.Configuration.GetSection("AdminSettings"));

builder.Services.AddHttpClient("ApiClient", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }

    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("MlflowClient", client =>
{
    var mlflowBaseUrl = builder.Configuration["BackendDependencies:MlflowTrackingUri"];
    if (!string.IsNullOrWhiteSpace(mlflowBaseUrl))
    {
        client.BaseAddress = new Uri(mlflowBaseUrl);
    }

    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IHealthApiService, HealthApiService>();
builder.Services.AddScoped<IDataCatalogApiService, DataCatalogApiService>();
builder.Services.AddScoped<IJobApiService, JobApiService>();
builder.Services.AddScoped<IMlflowApiService, MlflowApiService>();
builder.Services.AddScoped<IMinioApiService, MinioApiService>();
builder.Services.AddScoped<IGoldQuestionApiService, GoldQuestionApiService>();
builder.Services.AddScoped<IGoldLapApiService, GoldLapApiService>();
builder.Services.AddScoped<IDriverProfileApiService, DriverProfileApiService>();
builder.Services.AddScoped<IImportSeasonApiService, ImportSeasonApiService>();
builder.Services.AddScoped<IUserGridPreferenceService, UserGridPreferenceService>();
builder.Services.AddScoped<IUiCacheService, UiCacheService>();
builder.Services.AddScoped<IAdminSessionService, AdminSessionService>();
builder.Services.AddScoped<NavigationLoadingState>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
