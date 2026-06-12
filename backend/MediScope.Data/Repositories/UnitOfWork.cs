// File: MediScope.Data/Repositories/UnitOfWork.cs

using Microsoft.EntityFrameworkCore.Storage;
using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        private IGenericRepository<User>? _users;
        private IPatientRepository? _patients;
        private IDoctorRepository? _doctors;
        private IGenericRepository<Admin>? _admins;
        private IDoctorPatientRepository? _doctorPatients;
        private IHealthMetricRepository? _healthMetrics;
        private IGenericRepository<MetricDefinition>? _metricDefinitions;
        private IPasswordResetTokenRepository? _passwordResetTokens;
        private IGenericRepository<HealthAlert>? _healthAlerts;
        private IGenericRepository<AuditLog>? _auditLogs;
        private IGenericRepository<PatientAuditLog>? _patientAuditLogs;
        private IGenericRepository<RefreshToken>? _refreshTokens;
        //private IHealthMetricSubmissionRepository? _healthMetricSubmissions;
        private INotificationRepository? _notifications;
        public IDoctorDashboardRepository? _doctorDashboard;
        private IPatientDashboardRepository? _patientDashboard;
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
        public IPatientRepository Patients => _patients ??= new PatientRepository(_context);
        public IDoctorRepository Doctors => _doctors ??= new DoctorRepository(_context);
        public IGenericRepository<Admin> Admins => _admins ??= new GenericRepository<Admin>(_context);
        public IDoctorPatientRepository DoctorPatients => _doctorPatients ??= new DoctorPatientRepository(_context);
        public IHealthMetricRepository HealthMetrics => _healthMetrics ??= new HealthMetricRepository(_context);
        public IGenericRepository<MetricDefinition> MetricDefinitions => _metricDefinitions ??= new GenericRepository<MetricDefinition>(_context);
        public IGenericRepository<HealthAlert> HealthAlerts => _healthAlerts ??= new GenericRepository<HealthAlert>(_context);
        public IGenericRepository<AuditLog> AuditLogs => _auditLogs ??= new GenericRepository<AuditLog>(_context);
        public IGenericRepository<PatientAuditLog> PatientAuditLogs => _patientAuditLogs ??= new GenericRepository<PatientAuditLog>(_context);
        public IGenericRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new GenericRepository<RefreshToken>(_context);
        //public IHealthMetricSubmissionRepository HealthMetricSubmissions => _healthMetricSubmissions ??= new HealthMetricSubmissionRepository(_context);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
        public IPatientDashboardRepository PatientDashboard => _patientDashboard ??= new PatientDashboardRepository(_context);
        public IPasswordResetTokenRepository PasswordResetTokens => _passwordResetTokens ??= new PasswordResetTokenRepository(_context);
        public IDoctorDashboardRepository DoctorDashboard => _doctorDashboard ??= new DoctorDashboardRepository(_context);
        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
            => _transaction = await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}