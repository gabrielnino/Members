using Autodesk.Members.Portal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
var apiConfig = builder.Configuration.GetSection("AutodeskApi");
var baseUri = apiConfig.GetValue<string>("BaseAddress");
builder.Services.AddHttpClient("AutodeskApi", api =>
{
    api.BaseAddress = new Uri(baseUri);
});
await builder.Build().RunAsync();
