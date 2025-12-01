using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MPolls.WebUI;
using MPolls.WebUI.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.HostEnvironment.BaseAddress.StartsWith("https", StringComparison.OrdinalIgnoreCase)
    ? "https://localhost:5001/"
    : "http://localhost:5000/";

builder.Services.AddTransient<IncludeBrowserCredentialsHandler>();

builder.Services.AddHttpClient<AuthClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<IncludeBrowserCredentialsHandler>();

builder.Services.AddScoped<AuthState>();

builder.Services.AddHttpClient<EmployeesClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<IncludeBrowserCredentialsHandler>();

builder.Services.AddHttpClient<SurveyCategoriesClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<IncludeBrowserCredentialsHandler>();

builder.Services.AddHttpClient<ProfileSurveyClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<IncludeBrowserCredentialsHandler>();

builder.Services.AddHttpClient<UserRewardClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<IncludeBrowserCredentialsHandler>();

builder.Services.AddHttpClient<SurveyResultsClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<IncludeBrowserCredentialsHandler>();

builder.Services.AddHttpClient<PanelistClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<IncludeBrowserCredentialsHandler>();

builder.Services.AddHttpClient<CountriesClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<IncludeBrowserCredentialsHandler>();

builder.Services.AddHttpClient<RecommendedSurveyClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<IncludeBrowserCredentialsHandler>();

builder.Services.AddHttpClient<DashboardClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<IncludeBrowserCredentialsHandler>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
