// File: MediScope.Data/Repositories/IUnitOfWork.cs

using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IPatientRepository Patients { get; }
        IDoctorRepository Doctors { get; }
        IGenericRepository<Admin> Admins { get; }
        IDoctorPatientRepository DoctorPatients { get; }
        IHealthMetricRepository HealthMetrics { get; }
        IGenericRepository<MetricDefinition> MetricDefinitions { get; }
        IGenericRepository<HealthAlert> HealthAlerts { get; }
        IGenericRepository<AuditLog> AuditLogs { get; }
        IGenericRepository<PatientAuditLog> PatientAuditLogs { get; }
        IGenericRepository<RefreshToken> RefreshTokens { get; }
        //IHealthMetricSubmissionRepository HealthMetricSubmissions { get; }
        INotificationRepository Notifications { get; }
        IPatientDashboardRepository PatientDashboard { get; }
        IPasswordResetTokenRepository PasswordResetTokens { get; }
        IDoctorDashboardRepository DoctorDashboard { get; }
        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}