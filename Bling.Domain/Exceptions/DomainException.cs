namespace Bling.Domain.Exceptions;

/// <summary>
/// Exceção base de domínio
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exceção quando entidade não é encontrada
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityName, object entityId)
        : base($"{entityName} com ID '{entityId}' não encontrado(a).")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}

/// <summary>
/// Exceção quando o token de autenticação está expirado ou inválido
/// </summary>
public class TokenExpiradoException : DomainException
{
    public TokenExpiradoException()
        : base("Token de autenticação expirado ou inválido. É necessário re-autenticar.") { }

    public TokenExpiradoException(string message) : base(message) { }
}

/// <summary>
/// Exceção quando uma regra de validação de negócio é violada
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message) { }
}
