using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System.Net.WebSockets;
using System.Text;
using WangenPizza.Context;
using WangenPizza.Helper;
using WangenPizza.Helper.Mapper;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Services;
using static WangenPizza.Api_s.Controllers.ProductController;
using ProductService = WangenPizza.Services.ProductService;

var builder = WebApplication.CreateBuilder(args);

// Must survive IIS / Docker restarts; otherwise auth cookies become invalid and users see "random" logouts.
var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "dp-keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("WangenPizzaAdmin");

// Add services to the container.
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

    // Configure JSON serialization to handle decimal values properly
    options.SerializerSettings.FloatParseHandling = Newtonsoft.Json.FloatParseHandling.Decimal;
    options.SerializerSettings.FloatFormatHandling = Newtonsoft.Json.FloatFormatHandling.String;
});

builder.Services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// CORS: Frontend auf www.pizzawangen.ch, API oft auf admin.pizzawangen.ch — mitCredentials erfordert explizite Origins (kein AllowAnyOrigin).
builder.Services.AddCors(options =>
{
    options.AddPolicy("WangenPublicCors", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin)) return false;
                try
                {
                    var host = new Uri(origin).Host;
                    if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (host.Equals("pizzawangen.ch", StringComparison.OrdinalIgnoreCase))
                        return true;
                    if (host.EndsWith(".pizzawangen.ch", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                catch
                {
                    /* ignore */
                }
                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddScoped<StripeService>();

builder.Services.AddAutoMapper(x => x.AddProfile(new DomainProfile()));
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefultConnection")));

// Kürzere Keep-Alive helfen hinter Proxys; Long Polling im Client umgeht WS-1006 bei Shared Hosting.
builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(120);
});

// Add WebSocket support

#region Dependency Injection
builder.Services.AddTransient<DbInitializer>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<ISubCategoryService, SubCategoryService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IDiscountCodeService, DiscountCodeService>();
builder.Services.AddTransient<IContactService, ContactService>();
builder.Services.AddTransient<IOfferService, OfferService>();
builder.Services.AddTransient<ITodayBonusService, TodayBonusService>();
builder.Services.AddTransient<ICartService, CartService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ICompanyService, CompanyService>();
builder.Services.AddTransient<IDeliveryService, DeliveryService>();
builder.Services.AddTransient<IEmailTextService, EmailTextService>();
builder.Services.AddTransient<IpaymentService, paymentService>();
builder.Services.AddTransient<IReservationService, ReservationService>();
builder.Services.AddTransient<IExtensionService, ExtensionService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ITempReservationService, TempReservationService>();
builder.Services.AddSingleton<ITempOrderService, TempOrderService>();
builder.Services.AddSingleton<TwintService>();
builder.Services.AddScoped<PostFinancePaymentService>();
builder.Services.AddScoped<IOrderPaymentCompletionService, OrderPaymentCompletionService>();

builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddTransient<IEmailHtmlTemplateService, EmailHtmlTemplateService>();

#endregion

// WICHTIG: Default NICHT JwtBearer — Admin nutzt Identity-Cookie. Mit JWT als Default schlagen Browser-/SignalR-Requests oft fehl (kein Bearer-Token).
IdentityBuilder identityBuilder = builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 0;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    // Nicht dasselbe wie LoginPath — sonst kann «authenticated but wrong role» mit Challenge-Logik kollidieren.
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Hinter Cloudflare/HTTPS: Cookie sonst manchmal nicht gesetzt → Endlosschleifen Login.
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

// JWT nur als benanntes Schema (z. B. [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]). Identity bleibt Default.
builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
    {
        o.RequireHttpsMetadata = false;
        o.SaveToken = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
        };
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});





app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("WangenPublicCors");
app.UseSession(); // Session MUST be before Authentication for VisitorId to work
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    // Map controllers (API)
    endpoints.MapControllers();

    // Map controller routes
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Stand 2026-04-05: Kein WebSocket (1006). SSE + Long Polling — auf HTTP/2 stabiler als nur LP (ERR_HTTP2_PROTOCOL_ERROR).
    endpoints.MapHub<NotificationHub>("/notificationHub", options =>
    {
        options.Transports = HttpTransportType.ServerSentEvents | HttpTransportType.LongPolling;
    });

    // SPA-Fallback nur auf Kunden-Host; auf admin.* sonst index.html = falsche «Shop»-Oberfläche nach Login.
    endpoints.MapFallback(async context =>
    {
        var cfg = context.RequestServices.GetRequiredService<IConfiguration>();
        var marker = (cfg["Hosting:AdminPanelHostSubstring"] ?? "admin.").Trim();
        var host = context.Request.Host.Host;
        if (!string.IsNullOrEmpty(marker) && host.Contains(marker, StringComparison.OrdinalIgnoreCase))
        {
            var path = context.Request.Path.Value ?? "";
            // Niemals Login-URL über Fallback erneut auf Login umleiten (Schleife).
            if (path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Route nicht gefunden. Bitte Deployment / IIS Path prüfen.");
                return;
            }
            // Nicht eingeloggt: direkt Login — sonst /Home/Index → Challenge → Login (mehrfach Redirects / Loop mit Proxys).
            // Eingeloggt: Dashboard (Rollen-Check erfolgt dort; ohne Admin-Rolle → AccessDenied / Login).
            if (context.User?.Identity?.IsAuthenticated == true)
                context.Response.Redirect("/Home/Index");
            else
                context.Response.Redirect("/Account/Login");
            return;
        }

        var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var filePath = Path.Combine(env.WebRootPath, "index.html");
        if (!System.IO.File.Exists(filePath))
        {
            context.Response.StatusCode = 404;
            return;
        }
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync(filePath);
    });
});

// Seed the database if needed
await app.UseItToSeedSqlServer();

app.Run();
