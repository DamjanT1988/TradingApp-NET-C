using static UMMWebSocketService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

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

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Create an instance of UMMWebSocketService to start listening on app startup
        var webSocketService = new UMMWebSocketService();
        await webSocketService.ConnectToUMMWebSocketAsync(OnNewMessageReceived);

        await app.RunAsync();

        // Callback to process the message
        void OnNewMessageReceived(UMMMessage message)
        {
            Console.WriteLine($"New WebSocket message: {message.ProductionType} - {message.UnavailableCapacity} MW");
        }
    }
}
