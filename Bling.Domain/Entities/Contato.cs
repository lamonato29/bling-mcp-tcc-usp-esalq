using Bling.Domain.Enums;

namespace Bling.Domain.Entities;

/// <summary>
/// Contato (cliente/fornecedor) do Bling
/// </summary>
public class Contato : Entity
{
    public string? Nome { get; set; }
    public string? Codigo { get; set; }
    public string? Situacao { get; set; }
    public string? NumeroDocumento { get; set; }
    public string? Telefone { get; set; }
    public string? Celular { get; set; }
    public string? Fantasia { get; set; }
    public string? Tipo { get; set; }
    public int IndicadorIe { get; set; }
    public string? Ie { get; set; }
    public string? Rg { get; set; }
    public string? OrgaoEmissor { get; set; }
    public string? Email { get; set; }
    public TipoPessoa? TipoPessoa { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Sexo { get; set; }
    public DateTime? ClienteDesde { get; set; }
    public Endereco? EnderecoGeral { get; set; }
}

/// <summary>
/// Endereço de um contato
/// </summary>
public class Endereco
{
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cep { get; set; }
    public string? Municipio { get; set; }
    public string? Uf { get; set; }
    public string? Pais { get; set; }

    /// <summary>
    /// Retorna o endereço formatado em uma linha
    /// </summary>
    public string FormatarCompleto()
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(Logradouro)) partes.Add(Logradouro);
        if (!string.IsNullOrWhiteSpace(Numero)) partes.Add(Numero);
        if (!string.IsNullOrWhiteSpace(Complemento)) partes.Add(Complemento);
        if (!string.IsNullOrWhiteSpace(Bairro)) partes.Add(Bairro);
        if (!string.IsNullOrWhiteSpace(Municipio)) partes.Add(Municipio);
        if (!string.IsNullOrWhiteSpace(Uf)) partes.Add(Uf);
        if (!string.IsNullOrWhiteSpace(Cep)) partes.Add($"CEP: {Cep}");
        return string.Join(", ", partes);
    }
}
