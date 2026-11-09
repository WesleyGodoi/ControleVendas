using SQLite;

namespace ControleVendasLajes.Models;

public enum SaleStatus
{
    Orcamento = 0,
    Fechado = 1
}

public enum DeliveryStatus
{
    Pendente = 0,
    EmRota = 1,
    Entregue = 2
}

public enum PaymentStatus
{
    Pendente = 0,
    Parcial = 1,
    Pago = 2
}

[Table("Clients")]
public class Client
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(120), NotNull]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Street { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Number { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Neighborhood { get; set; } = string.Empty;

    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Notes { get; set; } = string.Empty;

    [MaxLength(350)]
    public string DeliveryAddress { get; set; } = string.Empty;
}

[Table("Products")]
public class Product
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100), NotNull]
    public string Name { get; set; } = string.Empty;

    public decimal DefaultPricePerSqm { get; set; }
}

[Table("SalePayments")]
public class SalePayment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int SaleId { get; set; }

    public decimal Amount { get; set; } // Valor recebido nesta parcela

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [MaxLength(80)]
    public string PaymentMethod { get; set; } = "PIX"; // PIX, Dinheiro, Cartão, Transferência, Boleto

    [MaxLength(200)]
    public string Notes { get; set; } = string.Empty; // Observação (ex: Entrada 50%, Parcela 1)
}

[Table("Sales")]
public class Sale
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ClientId { get; set; }

    [Indexed]
    public int ProductId { get; set; }

    public string ClientName { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public double AreaSqm { get; set; }
    public decimal UnitPriceSqm { get; set; }
    public decimal TotalAmount { get; set; }

    // Saldo recebido acumulado
    public decimal TotalPaid { get; set; }

    [Indexed]
    public DateTime SaleDate { get; set; } = DateTime.Now;

    // Status da Negociação
    public SaleStatus Status { get; set; } = SaleStatus.Orcamento;

    // Controle Independente de Entrega
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pendente;
    public DateTime? DeliveryDate { get; set; }

    // Controle Independente de Pagamento
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pendente;
    public DateTime? PaymentDate { get; set; }

    [Ignore]
    public decimal RemainingAmount => Math.Max(0, TotalAmount - TotalPaid);

    [Ignore]
    public string StatusText => Status switch
    {
        SaleStatus.Orcamento => "Orçamento",
        SaleStatus.Fechado => "Fechado",
        _ => Status.ToString()
    };

    [Ignore]
    public string StatusColor => Status switch
    {
        SaleStatus.Orcamento => "#FF9500",
        SaleStatus.Fechado => "#0A84FF",
        _ => "#64748B"
    };

    [Ignore]
    public string DeliveryStatusText => DeliveryStatus switch
    {
        DeliveryStatus.Pendente => "Entrega Pendente",
        DeliveryStatus.EmRota => "Em Transporte",
        DeliveryStatus.Entregue => "Entregue na Obra",
        _ => DeliveryStatus.ToString()
    };

    [Ignore]
    public string DeliveryStatusColor => DeliveryStatus switch
    {
        DeliveryStatus.Pendente => "#FF9500",
        DeliveryStatus.EmRota => "#6366F1",
        DeliveryStatus.Entregue => "#10B981",
        _ => "#64748B"
    };

    [Ignore]
    public string PaymentStatusText => PaymentStatus switch
    {
        PaymentStatus.Pendente => "Pagamento Pendente",
        PaymentStatus.Parcial => "Pago Parcial",
        PaymentStatus.Pago => "Quitado",
        _ => PaymentStatus.ToString()
    };

    [Ignore]
    public string PaymentStatusColor => PaymentStatus switch
    {
        PaymentStatus.Pendente => "#EF4444",
        PaymentStatus.Parcial => "#F59E0B",
        PaymentStatus.Pago => "#10B981",
        _ => "#64748B"
    };
}

public class MonthlySummaryDto
{
    public double TotalSqmSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingDeliveriesCount { get; set; }
    public int PendingPaymentsCount { get; set; }
}

public class DatabaseStatsDto
{
    public int ClientCount { get; set; }
    public int SaleCount { get; set; }
    public int PaymentCount { get; set; }
    public int ProductCount { get; set; }
    public long DatabaseSizeBytes { get; set; }
    public string FormattedSize => DatabaseSizeBytes switch
    {
        < 1024 => $"{DatabaseSizeBytes} B",
        < 1024 * 1024 => $"{(DatabaseSizeBytes / 1024.0):N1} KB",
        _ => $"{(DatabaseSizeBytes / (1024.0 * 1024.0)):N2} MB"
    };
    public DateTime? LastBackupDate { get; set; }
}
