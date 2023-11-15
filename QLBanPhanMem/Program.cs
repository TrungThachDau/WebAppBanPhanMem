
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QLBanPhanMem;

namespace QLBanPhanMem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            
            //builder.Services.AddAuthentication().AddFacebook(option =>
            //{
            //    option.AppId = "256451670702725";
            //    option.AppSecret = "c4d62036e87e81f036cf9be4394b8b3c";
            //});
            //builder.Services.AddAuthentication()
            //    .AddGoogle(googleOptions =>
            //    {
            //        // Đọc thông tin Authentication:Google từ appsettings.json
            //        IConfigurationSection googleAuthNSection = config.GetSection("Authentication:Google");

            //        // Thiết lập ClientID và ClientSecret để truy cập API google
            //        googleOptions.ClientId = googleAuthNSection["ClientId"];
            //        googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
            //        // Cấu hình Url callback lại từ Google (không thiết lập thì mặc định là /signin-google)
            //        googleOptions.CallbackPath = "/dangnhap-google";

            //    });
            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddControllersWithViews();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.MaxValue;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();           
            app.UseRouting();           
            app.UseAuthentication();
            app.UseSession();
            app.UseAuthorization();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}