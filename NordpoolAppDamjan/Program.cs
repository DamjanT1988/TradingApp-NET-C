using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register UMMService, RSSService, and WSService
builder.Services.AddHttpClient<UMMService>();
builder.Services.AddHttpClient<RSSService>();

// Register WebSocket Background Service (WSBackgroundService should handle WebSocket connections)
builder.Services.AddHostedService<WSBackgroundService>();

// Register the SignalR service
builder.Services.AddSignalR();

// Register WSService and its dependencies
builder.Services.AddSingleton<WSService>();

var app = builder.Build();

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

// Map SignalR Hub for real-time updates
app.MapHub<UMMHub>("/ummhub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
