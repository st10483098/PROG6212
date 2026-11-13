using RaceDay.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ApiService>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5000/");
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
