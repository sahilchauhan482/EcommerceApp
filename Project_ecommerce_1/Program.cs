using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Project_ecommerce_1.Data;
using Project_ecommerce_1.DataAccess.Repository;
using Project_ecommerce_1.DataAccess.Repository.IRepository;
using Project_ecommerce_1.Utility;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("ConStr");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();  
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddScoped<IEmailSender,EmailSender>();
builder.Services.AddScoped<TwilioService>();

builder.Services.Configure<StripeSetting>(builder.Configuration.GetSection("Stripe"));
builder.Services.Configure<EmailSetting>(builder.Configuration.GetSection("EmailSetting"));
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));


builder.Services.ConfigureApplicationCookie(Options=>
{
    Options.LoginPath = $"/Identity/Account/Login";
    Options.LogoutPath = $"/Identity/Account/Logout";
    Options.AccessDeniedPath = $"/Identity/Account/AccessDenied";

});

builder.Services.AddAuthentication().AddFacebook(Options =>
{
Options.AppId = "1054715635723235";
Options.AppSecret = "681adc2a57189330d8651ff4718a79e1";

});
builder.Services.AddAuthentication().AddGoogle(Options =>
{
    Options.ClientId = "470925506360-bo8jabkgp0sikv4o1islmljctpbu5u64.apps.googleusercontent.com";
    Options.ClientSecret = "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";
});
//builder.Services.AddAuthentication().AddLinkedIn(Options =>
//{
//    Options.ClientId = "861ialq1aqxib9";
//    Options.ClientSecret = "9rCD6gTorB503tpj";
//});
builder.Services.AddAuthentication().AddTwitter(Options =>
{
    Options.ConsumerKey = "DuZXHwU9rAu8rs3uS3grAKNvX";
    Options.ConsumerSecret = "2qkvQfsceiqWPNFx5HUAqFYTvK3coApCJW1iJRDmOx5RWvtEQx";
});
builder.Services.AddAuthentication().AddInstagram(Options =>
{
    Options.ClientId = "765621158727397";
    Options.ClientSecret = "3bf74b801637b22b48408006872e4b5e";
});
builder.Services.AddSession(Options =>
{
    Options.IdleTimeout = TimeSpan.FromMinutes(30);
    Options.Cookie.HttpOnly = true;
    Options.Cookie.IsEssential = true;

 }) ;


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{Area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
