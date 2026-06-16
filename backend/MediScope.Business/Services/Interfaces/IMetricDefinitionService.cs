using MediScope.Common.Models.DTOs.Request;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;

namespace MediScope.Business.Services.Interfaces
{
    public interface IMetricDefinitionService
        : IGenericService<MetricDefinition, MetricDefinitionResponseDto, CreateMetricDefinitionRequestDto, UpdateMetricDefinitionRequestDto>
    {
        Task<MetricDefinitionResponseDto> ToggleStatusAsync(Guid id);
        new Task<IEnumerable<MetricDefinitionResponseDto>> GetAllAsync();
        new Task<MetricDefinitionResponseDto> GetByIdAsync(Guid id);
    }
}