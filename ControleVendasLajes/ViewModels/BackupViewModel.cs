using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleVendasLajes.Models;
using ControleVendasLajes.Services;

namespace ControleVendasLajes.ViewModels;

public partial class BackupViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private int _clientCount;

    [ObservableProperty]
    private int _saleCount;

    [ObservableProperty]
    private int _paymentCount;

    [ObservableProperty]
    private int _productCount;

    [ObservableProperty]
    private string _databaseSizeText = "0 KB";

    [ObservableProperty]
    private string _lastBackupText = "Nenhum backup realizado ainda";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _busyStatusText = string.Empty;

    public BackupViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    public async Task LoadStatsAsync()
    {
        try
        {
            var stats = await _databaseService.GetDatabaseStatsAsync();
            ClientCount = stats.ClientCount;
            SaleCount = stats.SaleCount;
            PaymentCount = stats.PaymentCount;
            ProductCount = stats.ProductCount;
            DatabaseSizeText = stats.FormattedSize;

            LastBackupText = stats.LastBackupDate.HasValue
                ? stats.LastBackupDate.Value.ToString("dd/MM/yyyy 'às' HH:mm")
                : "Nenhum backup realizado ainda";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar estatísticas: {ex.Message}");
        }
    }

    [RelayCommand]
    public async Task CreateBackupAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            BusyStatusText = "Gerando arquivo de backup...";

            var backupPath = await _databaseService.CreateBackupAsync();
            var fileName = Path.GetFileName(backupPath);

            string savedMessage = $"Arquivo gerado:\n{fileName}";

            // No Windows, também copia uma via direta para a pasta Downloads do usuário
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                try
                {
                    var userDownloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    if (Directory.Exists(userDownloads))
                    {
                        var downloadsDest = Path.Combine(userDownloads, fileName);
                        File.Copy(backupPath, downloadsDest, true);
                        savedMessage = $"Cópia salva com sucesso na pasta Downloads:\n{downloadsDest}";
                    }
                }
                catch { }
            }

            // Tenta abrir o diálogo de compartilhamento (Share / Salvar em Arquivos / Nuvem)
            try
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Backup do Controle de Vendas de Lajes",
                    File = new ShareFile(backupPath)
                });
            }
            catch { }

            await LoadStatsAsync();

            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlert(
                    "Backup Concluído! 📦",
                    $"Backup de segurança criado com sucesso!\n\n{savedMessage}\n\nVocê pode guardar esse arquivo em um pen drive, Google Drive ou enviá-lo por e-mail.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlert(
                    "Erro ao Gerar Backup",
                    $"Não foi possível criar o backup: {ex.Message}",
                    "OK");
            }
        }
        finally
        {
            IsBusy = false;
            BusyStatusText = string.Empty;
        }
    }

    [RelayCommand]
    public async Task RestoreBackupAsync()
    {
        if (IsBusy) return;

        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Selecione o arquivo de backup (.db3 ou .bak)"
            };

            var picked = await FilePicker.Default.PickAsync(options);
            if (picked is null) return;

            if (Shell.Current is null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Atenção: Restaurar Dados 📥",
                $"Deseja realmente restaurar os dados do arquivo:\n'{picked.FileName}'?\n\nOs registros atuais serão substituídos pelos registros contidos neste backup.",
                "Sim, Restaurar",
                "Cancelar");

            if (!confirm) return;

            IsBusy = true;
            BusyStatusText = "Validando e restaurando base de dados...";

            var result = await _databaseService.RestoreBackupAsync(picked.FullPath);

            await LoadStatsAsync();

            await Shell.Current.DisplayAlert(
                "Restauração Concluída! 🎉",
                $"Os dados foram restaurados com sucesso:\n\n" +
                $"• Clientes: {result.clients}\n" +
                $"• Vendas Registradas: {result.sales}\n" +
                $"• Pagamentos no Histórico: {result.payments}\n" +
                $"• Tipos de Laje: {result.products}",
                "OK");
        }
        catch (Exception ex)
        {
            if (Shell.Current is not null)
            {
                await Shell.Current.DisplayAlert(
                    "Erro na Restauração",
                    ex.Message,
                    "OK");
            }
        }
        finally
        {
            IsBusy = false;
            BusyStatusText = string.Empty;
        }
    }
}
