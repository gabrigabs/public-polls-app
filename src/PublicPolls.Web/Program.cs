using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PublicPolls.Web;
using PublicPolls.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? "https://localhost:5001/") 
});

// Add MudBlazor
builder.Services.AddMudServices();

// Add custom services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISurveyService, SurveyService>();

await builder.Build().RunAsync();
