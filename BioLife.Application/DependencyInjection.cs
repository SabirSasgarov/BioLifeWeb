using BioLife.Application.Common.Interfaces;
using BioLife.Application.Services;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using AutoMapper;

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
			services.AddScoped<IProductService, ProductService>();

			return services;
		}
	}
}
