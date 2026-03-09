using ecartmvc.Data;
using ecartmvc.Hubs;
using ecartmvc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add services to the container
builder.Services.AddControllersWithViews();

// ✅ Database
builder.Services.AddDbContext<EcartDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Session + Cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Custom services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();

// ✅ SignalR
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ✅ Middleware order matters
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// ✅ SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

// ✅ Default route for user site
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Separate route for Admin area
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=AdminLogin}/{id?}",
    defaults: new { controller = "Admin" });

app.Run();
