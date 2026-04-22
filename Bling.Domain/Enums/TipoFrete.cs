namespace Bling.Domain.Enums;

/// <summary>
/// Tipo de frete do pedido
/// </summary>
public enum TipoFrete
{
    CIF = 0,
    FOB = 1,
    Terceiros = 2,
    ProprioRemetente = 3,
    ProprioDestinatario = 4,
    SemTransporte = 9
}

public static class TipoFreteExtensions
{
    public static string ToDescricao(this TipoFrete tipo) => tipo switch
    {
        TipoFrete.CIF => "Contratação do Frete por conta do Remetente (CIF)",
        TipoFrete.FOB => "Contratação do Frete por conta do Destinatário (FOB)",
        TipoFrete.Terceiros => "Contratação do Frete por conta de Terceiros",
        TipoFrete.ProprioRemetente => "Transporte Próprio por conta do Remetente",
        TipoFrete.ProprioDestinatario => "Transporte Próprio por conta do Destinatário",
        TipoFrete.SemTransporte => "Sem Ocorrência de Transporte",
        _ => "Desconhecido"
    };
}
