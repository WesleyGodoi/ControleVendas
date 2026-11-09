using ControleVendasLajes.ViewModels;

namespace ControleVendasLajes.Views;

public partial class SaleDetailPage : ContentPage
{
    public SaleDetailPage(SaleDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
