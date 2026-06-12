// File: MediScope.Business/Services/GenericService.cs

using System.Linq.Expressions;
using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    /// <summary>
    /// Abstract base service — provides default CRUD using UnitOfWork + Repository.
    /// All feature services inherit this and override or extend as needed.
    ///
    /// TEntity     → EF Core entity (e.g. Patient)
    /// TResponseDto → what API returns (e.g. PatientProfileResponseDto)
    /// TCreateDto  → what API receives on POST (e.g. CreatePatientDto)
    /// TUpdateDto  → what API receives on PUT  (e.g. UpdateProfileRequestDto)
    /// </summary>
    public abstract class GenericService<TEntity, TResponseDto, TCreateDto, TUpdateDto>
        : IGenericService<TEntity, TResponseDto, TCreateDto, TUpdateDto>
        where TEntity : BaseEntity
    {
        protected readonly IUnitOfWork _uow;
        protected readonly ICurrentUserService _currentUser;

        protected GenericService(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public virtual async Task<TResponseDto> GetByIdAsync(Guid id)
        {
            var entity = await GetRepository().GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{id}' not found.");

            return MapToResponseDto(entity);
        }

        public virtual async Task<IEnumerable<TResponseDto>> GetAllAsync()
        {
            var entities = await GetRepository().GetAllAsync();
            return entities.Select(MapToResponseDto);
        }

        public virtual async Task<TResponseDto> CreateAsync(TCreateDto dto)
        {
            var entity = MapToEntity(dto);

            entity.CreatedBy = _currentUser.UserId;
            entity.UpdatedBy = _currentUser.UserId;

            await GetRepository().AddAsync(entity);
            await _uow.SaveChangesAsync();

            return MapToResponseDto(entity);
        }

        // ── UPDATE ───────────────────────────────────────────────────
        public virtual async Task<TResponseDto> UpdateAsync(Guid id, TUpdateDto dto)
        {
            var entity = await GetRepository().GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{id}' not found.");

            ApplyUpdate(entity, dto);

            entity.UpdatedBy = _currentUser.UserId;
            entity.UpdatedAt = DateTime.UtcNow;

            GetRepository().Update(entity);
            await _uow.SaveChangesAsync();

            return MapToResponseDto(entity);
        }

        // ── DELETE (Soft) ────────────────────────────────────────────
        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetRepository().GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} with id '{id}' not found.");

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = _currentUser.UserId;

            GetRepository().Update(entity);
            await _uow.SaveChangesAsync();
        }

        // ── EXISTS ───────────────────────────────────────────────────
        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
            => await GetRepository().AnyAsync(predicate);

        // ── ABSTRACT METHODS — must implement in each feature service ─
        /// <summary>Returns the correct repository for this entity from UnitOfWork</summary>
        protected abstract IGenericRepository<TEntity> GetRepository();

        /// <summary>Maps entity → response DTO</summary>
        protected abstract TResponseDto MapToResponseDto(TEntity entity);

        /// <summary>Maps create DTO → new entity</summary>
        protected abstract TEntity MapToEntity(TCreateDto dto);

        /// <summary>Applies update DTO fields onto existing entity</summary>
        protected abstract void ApplyUpdate(TEntity entity, TUpdateDto dto);
    }
}