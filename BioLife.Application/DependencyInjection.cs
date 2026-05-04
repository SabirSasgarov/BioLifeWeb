

namespace BioLife.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplicationServices(this IServiceCollection services)
		{
			var assembly = Assembly.GetExecutingAssembly();

			services.AddAutoMapper(config => { }, assembly);
			services.AddValidatorsFromAssembly(assembly);
			services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

			return services;
		}
	}
}
