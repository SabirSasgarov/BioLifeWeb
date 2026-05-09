namespace BioLife.Persistence
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<AppDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
				b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
			
			services.AddIdentity<AppUser, AppRole>(options =>
			{
				options.Password.RequireDigit = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequiredLength = 6;
				options.SignIn.RequireConfirmedEmail = true;

				options.Lockout.MaxFailedAccessAttempts = 3;
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
				options.Lockout.AllowedForNewUsers = true;
			})
				.AddEntityFrameworkStores<AppDbContext>()
				.AddDefaultTokenProviders();

			services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
			services.AddScoped<IEmailService, EmailService>();
			services.AddScoped<IBasketService, BasketService>();
			services.AddScoped<IOrderService, OrderService>();
			services.AddScoped<IProductService, ProductService>();
			services.AddScoped<ISubscriberService, SubscriberService>();
			services.AddScoped<IReviewService, ReviewService>();
			return services;
		}
	}
}
