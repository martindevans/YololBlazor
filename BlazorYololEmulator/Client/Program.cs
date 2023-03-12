using BlazorYololEmulator.Client;
using BlazorYololEmulator.Client.Core;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Drawing;
using System.Runtime.InteropServices;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddSingleton<StateManager>();
builder.Services.AddSingleton<CodeContainer>();
builder.Services.AddSingleton<CodeRunner>();

await builder.Build().RunAsync();