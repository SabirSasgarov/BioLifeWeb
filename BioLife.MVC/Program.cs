using BioLife.Application;
using BioLife.Persistence;

namespace BioLife.MVC
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();
			builder.Services.AddApplicationServices();
			builder.Services.AddPersistence(builder.Configuration);
			builder.Services.AddAuthentication().AddGoogle(options =>
			{
				options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
				options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
			});


			var app = builder.Build();
			
			using(var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				await BioLife.Persistence.DataSeed.SeedRolesAndAdminAsync(services);
			}

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}
			app.UseStatusCodePagesWithReExecute("/Home/Error404");

			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseStaticFiles();

			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}
	}
}
