using DisasterAlleviationFoundation.Services;

// Create builder with custom options to fix WebRoot path
var options = new WebApplicationOptions
{
    Args = args,
    EnvironmentName = "Development",
    WebRootPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName, "wwwroot")
};

var builder = WebApplication.CreateBuilder(options);

// Debug: Log the paths being used
Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine($"WebRoot Path: {options.WebRootPath}");
Console.WriteLine($"WebRoot Exists: {Directory.Exists(options.WebRootPath)}");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "DAF.Session";
});

// Add HTTP context accessor for session access
builder.Services.AddHttpContextAccessor();

// Register custom services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IDonationService, DonationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Configure static files with explicit options
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Ensure CSS files are served with correct content type
        if (ctx.File.Name.EndsWith(".css"))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "text/css");
        }
    }
});

app.UseRouting();

// Add session middleware before authorization
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
