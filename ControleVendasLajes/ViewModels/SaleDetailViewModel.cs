using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleVendasLajes.Helpers;
using ControleVendasLajes.Models;
using ControleVendasLajes.Services;

namespace ControleVendasLajes.ViewModels;

[QueryProperty(nameof(SaleId), "id")]
public partial class SaleDetailViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private int _saleId;

    [ObservableProperty]
    private Sale? _sale;

    [ObservableProperty]
    private SaleStatus _currentStatus;

    [ObservableProperty]
    private DeliveryStatus _currentDeliveryStatus;

    [ObservableProperty]
    private PaymentStatus _currentPaymentStatus;

    [ObservableProperty]
    private bool _canDelete = false;

    // Controle Financeiro & Histórico de Pagamentos
    [ObservableProperty]
    private decimal _totalPaid;

    [ObservableProperty]
    private decimal _remainingAmount;

    [ObservableProperty]
    private double _paidProgress;

    [ObservableProperty]
    private decimal _newPaymentAmount;

    [ObservableProperty]
    private string _newPaymentAmountText = string.Empty;

    partial void OnNewPaymentAmountTextChanged(string value)
    {
        var formatted = MaskHelper.FormatCurrency(value, out var parsed);
        NewPaymentAmount = parsed;
        if (formatted != value)
        {
            NewPaymentAmountText = formatted;
        }
    }

    [ObservableProperty]
    private string _selectedPaymentMethod = "PIX";

    [ObservableProperty]
    private string _newPaymentNotes = string.Empty;

    [ObservableProperty]
    private DateTime _newPaymentDate = DateTime.Now;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PaymentFormButtonText))]
    private bool _showPaymentForm = false;

    public string PaymentFormButtonText => ShowPaymentForm ? "✕ Fechar" : "+ Receber Valor";

    public ObservableCollection<SalePayment> Payments { get; } = new();

    public IReadOnlyList<SaleStatus> StatusList { get; } = Enum.GetValues<SaleStatus>();
    public IReadOnlyList<DeliveryStatus> DeliveryStatusList { get; } = Enum.GetValues<DeliveryStatus>();
    public IReadOnlyList<PaymentStatus> PaymentStatusList { get; } = Enum.GetValues<PaymentStatus>();

    public List<string> PaymentMethodOptions { get; } = new()
    {
        "PIX",
        "Dinheiro",
        "Cartão de Débito",
        "Cartão de Crédito",
        "Transferência",
        "Boleto Bancário",
        "Cheque"
    };

    public SaleDetailViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    async partial void OnSaleIdChanged(int value)
    {
        if (value > 0)
        {
            await LoadSaleAsync(value);
        }
    }

    public async Task LoadSaleAsync(int id)
    {
        var sales = await _databaseService.GetSalesAsync();
        Sale = sales.FirstOrDefault(s => s.Id == id);
        if (Sale is not null)
        {
            CurrentStatus = Sale.Status;
            CurrentDeliveryStatus = Sale.DeliveryStatus;
            CurrentPaymentStatus = Sale.PaymentStatus;

            await RefreshPaymentsAsync();

            UpdateFinancialMetrics();

            // Pode excluir quando for Orçamento (ou não entregue nem pago nenhum valor)
            CanDelete = Sale.Status == SaleStatus.Orcamento || 
                        (Sale.DeliveryStatus == DeliveryStatus.Pendente && TotalPaid == 0 && Sale.PaymentStatus == PaymentStatus.Pendente);
        }
    }

    private async Task RefreshPaymentsAsync()
    {
        if (Sale is null) return;

        var list = await _databaseService.GetPaymentsBySaleIdAsync(Sale.Id);
        Payments.Clear();
        foreach (var p in list)
        {
            Payments.Add(p);
        }
    }

    private void UpdateFinancialMetrics()
    {
        if (Sale is null) return;

        TotalPaid = Payments.Sum(p => p.Amount);
        RemainingAmount = Math.Max(0, Sale.TotalAmount - TotalPaid);

        PaidProgress = Sale.TotalAmount > 0 
            ? Math.Min(1.0, (double)(TotalPaid / Sale.TotalAmount)) 
            : 0;

        // Pré-preenche o formulário com o saldo devedor restante para agilidade
        if (NewPaymentAmount == 0 && RemainingAmount > 0)
        {
            NewPaymentAmount = RemainingAmount;
            NewPaymentAmountText = MaskHelper.FormatCurrency(RemainingAmount);
        }
    }

    [RelayCommand]
    private void TogglePaymentForm()
    {
        ShowPaymentForm = !ShowPaymentForm;
        if (ShowPaymentForm && RemainingAmount > 0)
        {
            NewPaymentAmount = RemainingAmount;
            NewPaymentAmountText = MaskHelper.FormatCurrency(RemainingAmount);
        }
    }

    [RelayCommand]
    private void FillTotalRemaining()
    {
        NewPaymentAmount = RemainingAmount;
        NewPaymentAmountText = MaskHelper.FormatCurrency(RemainingAmount);
    }

    [RelayCommand]
    private async Task RegisterPaymentAsync()
    {
        if (Sale is null) return;

        if (NewPaymentAmount <= 0)
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlert("Valor Inválido", "Informe um valor de recebimento maior que zero.", "OK");
            return;
        }

        if (NewPaymentAmount > RemainingAmount && RemainingAmount > 0)
        {
            bool confirmOverpay = await Shell.Current.DisplayAlert(
                "Valor Superior ao Saldo",
                $"O valor informado (R$ {NewPaymentAmount:N2}) é maior que o saldo devedor (R$ {RemainingAmount:N2}). Deseja registrar mesmo assim?",
                "Sim, Registrar",
                "Cancelar");

            if (!confirmOverpay) return;
        }

        var payment = new SalePayment
        {
            SaleId = Sale.Id,
            Amount = NewPaymentAmount,
            PaymentMethod = SelectedPaymentMethod,
            Notes = string.IsNullOrWhiteSpace(NewPaymentNotes) ? $"Recebimento {SelectedPaymentMethod}" : NewPaymentNotes.Trim(),
            PaymentDate = NewPaymentDate
        };

        await _databaseService.AddPaymentAsync(payment);

        // Limpa campos
        NewPaymentNotes = string.Empty;
        NewPaymentAmount = 0;
        NewPaymentAmountText = string.Empty;
        ShowPaymentForm = false;

        // Recarrega dados atualizados do banco
        await LoadSaleAsync(Sale.Id);

        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlert(
                "Pagamento Registrado",
                $"Recebimento de R$ {payment.Amount:N2} cadastrado com sucesso! Novo status financeiro: {Sale.PaymentStatusText}.",
                "OK");
        }
    }

    [RelayCommand]
    private async Task DeletePaymentAsync(SalePayment payment)
    {
        if (Sale is null || payment is null || Shell.Current is null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Estornar Recebimento",
            $"Deseja remover o registro de pagamento de R$ {payment.Amount:N2} ({payment.PaymentMethod}) feito em {payment.PaymentDate:dd/MM/yyyy}?",
            "Estornar",
            "Cancelar");

        if (confirm)
        {
            await _databaseService.DeletePaymentAsync(payment);
            await LoadSaleAsync(Sale.Id);

            await Shell.Current.DisplayAlert("Estornado", "Pagamento removido e saldo recalculado.", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (Sale is null) return;

        Sale.Status = CurrentStatus;
        Sale.DeliveryStatus = CurrentDeliveryStatus;
        Sale.PaymentStatus = CurrentPaymentStatus;

        if (Sale.DeliveryStatus == DeliveryStatus.Entregue && Sale.DeliveryDate is null)
            Sale.DeliveryDate = DateTime.Now;

        if (Sale.PaymentStatus == PaymentStatus.Pago && Sale.PaymentDate is null)
            Sale.PaymentDate = DateTime.Now;

        await _databaseService.SaveSaleAsync(Sale);

        CanDelete = Sale.Status == SaleStatus.Orcamento || 
                    (Sale.DeliveryStatus == DeliveryStatus.Pendente && TotalPaid == 0 && Sale.PaymentStatus == PaymentStatus.Pendente);

        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlert("Atualizado", "Alterações salvas com sucesso!", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    [RelayCommand]
    private async Task DeleteSaleAsync()
    {
        if (Sale is null || Shell.Current is null) return;

        if (Sale.DeliveryStatus == DeliveryStatus.Entregue || Sale.PaymentStatus == PaymentStatus.Pago || TotalPaid > 0)
        {
            await Shell.Current.DisplayAlert("Ação Bloqueada", "Não é permitido excluir vendas que já foram Entregues ou que já possuem pagamentos registrados.", "OK");
            return;
        }

        bool confirm = await Shell.Current.DisplayAlert(
            "Confirmar Exclusão",
            $"Deseja realmente excluir este pedido de {Sale.ClientName}?",
            "Excluir",
            "Cancelar");

        if (confirm)
        {
            await _databaseService.DeleteSaleAsync(Sale);
            await Shell.Current.DisplayAlert("Excluído", "Pedido excluído com sucesso!", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }
}
