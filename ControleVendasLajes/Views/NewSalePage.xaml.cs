using ControleVendasLajes.ViewModels;

namespace ControleVendasLajes.Views;

public partial class NewSalePage : ContentPage
{
    private readonly NewSaleViewModel _viewModel;

    public NewSalePage(NewSaleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
