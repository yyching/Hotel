global using Hotel.Models;
global using Hotel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddSqlServer<DB>($@"
    Data Source=(LocalDB)\MSSQLLocalDB;
    AttachDbFilename={builder.Environment.ContentRootPath}\DB.mdf;
");
builder.Services.AddScoped<Helper>();

builder.Services.AddAuthentication().AddCookie();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
var key = builder.Configuration.GetValue<string>("StripeSettings:SecretKey");

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapDefaultControllerRoute();
app.Run();
