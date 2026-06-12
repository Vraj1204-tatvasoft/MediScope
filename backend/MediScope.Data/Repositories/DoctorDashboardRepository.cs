using Microsoft.EntityFrameworkCore;
using MediScope.Common.Models.DTOs.Response;

namespace MediScope.Data.Repositories
{
    public class DoctorDashboardRepository : IDoctorDashboardRepository
    {
        private readonly AppDbContext _context;

        public DoctorDashboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<VitalTrendFlatResult>> CallVitalTrendsFunctionAsync(
            Guid doctorId,
            string metricType,
            string patientId,
            DateTime start,
            DateTime end)
        {
            FormattableString sql = $@" SELECT 
                    patient_id, 
                    patient_name, 
                    metric_type, 
                    unit, 
                    recorded_at, 
                    metric_value 
                FROM get_vital_trends({doctorId}, {metricType}, {patientId}, {start}, {end})";

            return await _context.Database
                .SqlQuery<VitalTrendFlatResult>(sql)
                .ToListAsync();
        }
    }
}