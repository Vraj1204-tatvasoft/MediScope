// File: MediScope.Business/Services/Interfaces/IGenericService.cs

using System.Linq.Expressions;

namespace MediScope.Business.Services.Interfaces
{
    public interface IGenericService<TEntity, TResponseDto, TCreateDto, TUpdateDto>
        where TEntity : class
    {
        Task<TResponseDto> GetByIdAsync(Guid id);
        Task<IEnumerable<TResponseDto>> GetAllAsync();
        Task<TResponseDto> CreateAsync(TCreateDto dto);
        Task<TResponseDto> UpdateAsync(Guid id, TUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
    }
}