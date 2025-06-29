using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using VanQuangTin_2280603267_Lab04.Models;
using VanQuangTin_2280603267_Lab04.Repositories;
using GearShop.Models.Momo;
using GearShop.Services.Momo;
using ProGCoder_MomoAPI.Services;

var builder = WebApplication.CreateBuilder(args);

//Connect API MOMO
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ SỬ DỤNG Identity với Role (KHÔNG dùng AddDefaultIdentity nữa)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // Nếu bạn dùng Razor UI mặc định của Identity


builder.Services.AddRazorPages();

// Repositories
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Google Authentication (KHÔNG override DefaultScheme nữa)
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        googleOptions.CallbackPath = "/signin-google";

        googleOptions.Events.OnRemoteFailure = context =>
        {
            var error = context.Failure?.Message ?? "Đăng nhập Google thất bại.";
            context.Response.Redirect("/Home/Error?message=" + Uri.EscapeDataString(error));
            context.HandleResponse(); // Ngăn xử lý mặc định tiếp theo
            return Task.CompletedTask;
        };

        googleOptions.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
            return Task.CompletedTask;
        };
    });

builder.Services.AddSingleton<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}


app.UseSession();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "Admin",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.MapRazorPages();
app.Run();

public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("quangtin13072004@gmail.com", "pyue xwsp twqg xeeg"),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("quangtin13072004@gmail.com"),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true,
        };
        // Thêm phần văn bản thuần túy để tránh bị đánh dấu là thư rác
        var plainTextMessage = "Đây là phiên bản văn bản thuần của email. Vui lòng xem phiên bản HTML để có định dạng tốt hơn.";
        var alternateViewPlainText = AlternateView.CreateAlternateViewFromString(plainTextMessage, null, "text/plain");
        var alternateViewHtml = AlternateView.CreateAlternateViewFromString(htmlMessage, null, "text/html");



        mailMessage.To.Add(email);
        await smtpClient.SendMailAsync(mailMessage);
    }
}