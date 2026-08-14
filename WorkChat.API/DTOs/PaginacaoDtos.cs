using System.ComponentModel.DataAnnotations;

namespace WorkChat.DTOs;

public sealed class PaginacaoQuery
{
    [Range(1, int.MaxValue)] public int Pagina { get; init; } = 1;
    [Range(1, 100)] public int Tamanho { get; init; } = 20;
}

public sealed record PaginaResponse<T>(IReadOnlyCollection<T> Itens, int Pagina, int Tamanho, int Total)
{
    public int TotalPaginas => (int)Math.Ceiling(Total / (double)Tamanho);
}
