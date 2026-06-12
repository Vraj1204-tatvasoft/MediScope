using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;
using MediScope.Data.Repositories;

namespace MediScope.Business.Services
{
    public class MetricDefinitionService
        : GenericService<
            MetricDefinition,
            MetricDefinitionResponseDto,
            CreateMetricDefinitionRequestDto,
            UpdateMetricDefinitionRequestDto>,
          IMetricDefinitionService
    {
        public MetricDefinitionService(
            IUnitOfWork uow,
            ICurrentUserService currentUser)
            : base(uow, currentUser)
        {
        }

        protected override IGenericRepository<MetricDefinition>
            GetRepository()
            => _uow.MetricDefinitions;

        protected override MetricDefinitionResponseDto
            MapToResponseDto(MetricDefinition entity)
        {
            return new MetricDefinitionResponseDto
            {
                Id = entity.Id,
                MetricType = entity.MetricType,
                DisplayName = entity.DisplayName,
                DefaultUnit = entity.DefaultUnit,
                NormalMin = entity.NormalMin,
                NormalMax = entity.NormalMax,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        protected override MetricDefinition
            MapToEntity(CreateMetricDefinitionRequestDto dto)
        {
            return new MetricDefinition
            {
                MetricType = dto.MetricType.Trim().ToLower(),
                DisplayName = dto.DisplayName.Trim(),
                DefaultUnit = dto.DefaultUnit.Trim(),
                NormalMin = dto.NormalMin,
                NormalMax = dto.NormalMax,
                Description = dto.Description
            };
        }

        protected override void ApplyUpdate(
            MetricDefinition entity,
            UpdateMetricDefinitionRequestDto dto)
        {
            if (dto.NormalMin.HasValue &&
            dto.NormalMax.HasValue &&
            dto.NormalMax < dto.NormalMin)
            {
                throw new InvalidOperationException(
                    "Normal max value cannot be smaller than normal min value.");
            }

            entity.DisplayName = dto.DisplayName.Trim();
            entity.DefaultUnit = dto.DefaultUnit.Trim();
            entity.NormalMin = dto.NormalMin;
            entity.NormalMax = dto.NormalMax;
            entity.Description = dto.Description;
        }

        public override async Task<MetricDefinitionResponseDto>
        CreateAsync(CreateMetricDefinitionRequestDto dto)
        {
            // ── VALIDATE RANGE ─────────────────────────────
            if (dto.NormalMin.HasValue &&
                dto.NormalMax.HasValue &&
                dto.NormalMax < dto.NormalMin)
            {
                throw new InvalidOperationException(
                    "Normal max value cannot be smaller than normal min value.");
            }

            // ── CHECK EXISTING METRIC ─────────────────────
            var existingMetric =
                await _uow.MetricDefinitions.GetFirstOrDefaultAsync(
                    m => m.MetricType.ToLower()
                        == dto.MetricType.ToLower());

            // ── IF ACTIVE METRIC EXISTS ───────────────────
            if (existingMetric is not null &&
                !existingMetric.IsDeleted)
            {
                throw new InvalidOperationException(
                    "Metric type already exists.");
            }

            // ── RESTORE SOFT-DELETED METRIC ───────────────
            if (existingMetric is not null &&
                existingMetric.IsDeleted)
            {
                existingMetric.IsDeleted = false;
                existingMetric.DeletedAt = null;
                existingMetric.DeletedBy = null;

                existingMetric.DisplayName =
                    dto.DisplayName.Trim();

                existingMetric.DefaultUnit =
                    dto.DefaultUnit.Trim();

                existingMetric.NormalMin =
                    dto.NormalMin;

                existingMetric.NormalMax =
                    dto.NormalMax;

                existingMetric.Description =
                    dto.Description;

                existingMetric.UpdatedAt =
                    DateTime.UtcNow;

                existingMetric.UpdatedBy =
                    _currentUser.UserId;

                _uow.MetricDefinitions.Update(existingMetric);

                await _uow.SaveChangesAsync();

                return MapToResponseDto(existingMetric);
            }

            return await base.CreateAsync(dto);
        }
    }
}