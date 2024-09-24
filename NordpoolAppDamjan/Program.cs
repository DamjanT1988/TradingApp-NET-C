using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register UMMService, RSSService, and WSService
builder.Services.AddHttpClient<UMMService>();
builder.Services.AddHttpClient<RSSService>();

// Register WSService as a singleton to handle WebSocket connections
builder.Services.AddSingleton<WSService>();

var app = builder.Build();

// Start the WebSocket listener on application start
var wsService = app.Services.GetRequiredService<WSService>();
Task.Run(() => wsService.StartListeningAsync());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
