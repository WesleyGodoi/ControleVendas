using ControleVendasLajes.Services;
using ControleVendasLajes.ViewModels;
using ControleVendasLajes.Views;
using Microsoft.Extensions.Logging;

namespace ControleVendasLajes;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<DatabaseService>();

		builder.Services.AddTransient<DashboardViewModel>();
		builder.Services.AddTransient<DashboardPage>();

		builder.Services.AddTransient<NewSaleViewModel>();
		builder.Services.AddTransient<NewSalePage>();

		builder.Services.AddTransient<SalesListViewModel>();
		builder.Services.AddTransient<SalesListPage>();

		builder.Services.AddTransient<SaleDetailViewModel>();
		builder.Services.AddTransient<SaleDetailPage>();

		builder.Services.AddTransient<ClientsViewModel>();
		builder.Services.AddTransient<ClientsPage>();

		builder.Services.AddTransient<ProductsViewModel>();
		builder.Services.AddTransient<ProductsPage>();

		builder.Services.AddTransient<BackupViewModel>();
		builder.Services.AddTransient<BackupPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
