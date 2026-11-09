using ControleVendasLajes.Views;

namespace ControleVendasLajes;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(SaleDetailPage), typeof(SaleDetailPage));
		Routing.RegisterRoute(nameof(NewSalePage), typeof(NewSalePage));
	}
}
