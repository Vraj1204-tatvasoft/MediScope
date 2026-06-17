using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using MediScope.Common.Models.Entities;
using MediScope.Common.Models.Enums;

namespace MediScope.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── DbSets ──────────────────────────────────────────────────
        public DbSet<User> Users { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<DoctorPatient> DoctorPatients { get; set; }
        public DbSet<HealthMetric> HealthMetrics { get; set; }
        public DbSet<MetricDefinition> MetricDefinitions { get; set; }
        public DbSet<HealthAlert> HealthAlerts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentSlot> AppointmentSlots { get; set; }
        public DbSet<PatientAuditLog> PatientAuditLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<MedicalDocument> MedicalDocuments { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        private static string ToSnakeCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return Regex.Replace(text, "([a-z])([A-Z])", "$1_$2").ToLower();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType, entity =>
                    {
                        entity.Property(nameof(BaseEntity.Id))
                              .HasColumnName("id")
                              .HasDefaultValueSql("gen_random_uuid()");

                        entity.Property(nameof(BaseEntity.CreatedAt))
                              .HasColumnName("created_at")
                              .HasDefaultValueSql("now()");

                        entity.Property(nameof(BaseEntity.UpdatedAt))
                              .HasColumnName("updated_at")
                              .HasDefaultValueSql("now()");

                        entity.Property(nameof(BaseEntity.CreatedBy))
                              .HasColumnName("created_by");

                        entity.Property(nameof(BaseEntity.UpdatedBy))
                              .HasColumnName("updated_by");
                        entity.Property(nameof(BaseEntity.IsDeleted))
                              .HasColumnName("is_deleted")
                              .HasDefaultValue(false);

                        entity.Property(nameof(BaseEntity.DeletedAt))
                              .HasColumnName("deleted_at");

                        entity.Property(nameof(BaseEntity.DeletedBy))
                              .HasColumnName("deleted_by");
                    });
                }
            }

            // ── USERS ────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(u => u.FullName)
                      .HasColumnName("full_name")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(u => u.Email)
                      .HasColumnName("email")
                      .HasMaxLength(200)
                      .IsRequired();

                entity.HasIndex(u => u.Email)
                      .IsUnique()
                      .HasDatabaseName("uq_users_email");

                entity.Property(u => u.PasswordHash)
                      .HasColumnName("password_hash")
                      .IsRequired();

                entity.Property(u => u.Role)
                      .HasColumnName("role")
                      .HasConversion<string>()
                      .IsRequired();

                entity.Property(u => u.IsActive)
                      .HasColumnName("is_active")
                      .HasDefaultValue(true);

                entity.Property(u => u.MustChangePassword)
                      .HasColumnName("must_change_password")
                      .HasDefaultValue(false);

                entity.Property(u => u.CurrentSessionId)
                      .HasColumnName("current_session_id")
                      .HasDefaultValueSql("gen_random_uuid()");
            });

            // ── PATIENTS ─────────────────────────────────────────────
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("patients");

                entity.Property(p => p.UserId)
                      .HasColumnName("user_id")
                      .IsRequired();

                entity.HasOne(p => p.User)
                      .WithOne(u => u.Patient)
                      .HasForeignKey<Patient>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(p => p.UserId)
                      .IsUnique()
                      .HasDatabaseName("uq_patients_user_id");

                entity.Property(p => p.DateOfBirth).HasColumnName("date_of_birth");
                entity.Property(p => p.Gender).HasColumnName("gender").HasMaxLength(20).HasConversion<string>();
                entity.Property(p => p.BloodGroup).HasColumnName("blood_group").HasMaxLength(5);
                entity.Property(p => p.ContactNumber).HasColumnName("contact_number").HasMaxLength(10);
                entity.Property(p => p.Address).HasColumnName("address");

                entity.Property(p => p.ConsentProfileVisible)
                      .HasColumnName("consent_profile_visible")
                      .HasDefaultValue(false);

                entity.HasMany(p => p.HealthMetrics)
                      .WithOne(s => s.Patient)
                      .HasForeignKey(s => s.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // ── DOCTORS ──────────────────────────────────────────────
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.ToTable("doctors");

                entity.Property(d => d.UserId).HasColumnName("user_id").IsRequired();

                entity.HasOne(d => d.User)
                      .WithOne(u => u.Doctor)
                      .HasForeignKey<Doctor>(d => d.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(d => d.UserId)
                      .IsUnique()
                      .HasDatabaseName("uq_doctors_user_id");

                entity.Property(d => d.Specialization).HasColumnName("specialization").HasMaxLength(100);

                entity.Property(d => d.LicenseNumber)
                      .HasColumnName("license_number")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(d => d.LicenseNumber)
                      .IsUnique()
                      .HasDatabaseName("uq_doctors_license");

                entity.Property(d => d.Hospital)
                      .HasColumnName("hospital")
                      .HasMaxLength(150);

                entity.Property(d => d.YearsExperience)
                      .HasColumnName("years_experience");

                entity.Property(d => d.ContactNumber).HasColumnName("contact_number").HasMaxLength(10).IsRequired();
                entity.HasIndex(d => d.ContactNumber).IsUnique();
                entity.Property(d => d.Bio).HasColumnName("bio");
            });

            // ── ADMINS ───────────────────────────────────────────────
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.ToTable("admins");

                entity.Property(a => a.UserId).HasColumnName("user_id").IsRequired();

                entity.HasOne(a => a.User)
                      .WithOne(u => u.Admin)
                      .HasForeignKey<Admin>(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => a.UserId)
                      .IsUnique()
                      .HasDatabaseName("uq_admins_user_id");

                entity.Property(a => a.Department).HasColumnName("department").HasMaxLength(100);
            });


            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.ToTable("password_reset_tokens");

                entity.Property(t => t.UserId)
                      .HasColumnName("user_id")
                      .IsRequired();

                entity.Property(t => t.Token)
                      .HasColumnName("token")
                      .HasMaxLength(500)
                      .IsRequired();

                entity.HasIndex(t => t.Token)
                      .IsUnique()
                      .HasDatabaseName("uq_password_reset_token");

                entity.Property(t => t.ExpiresAt)
                      .HasColumnName("expires_at")
                      .IsRequired();

                entity.Property(t => t.IsUsed)
                      .HasColumnName("is_used")
                      .HasDefaultValue(false)
                      .IsRequired();

                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(t => t.UserId)
                      .HasDatabaseName("ix_password_reset_tokens_user_id");
            });


            modelBuilder.Entity<MedicalDocument>(entity =>
            {
                entity.ToTable("medical_documents");

                entity.Property(d => d.PatientId)
                          .HasColumnName("patient_id")
                          .IsRequired();

                entity.Property(d => d.DoctorId)
                          .HasColumnName("doctor_id")
                          .IsRequired();

                entity.Property(d => d.FileName)
                          .HasColumnName("file_name")
                          .HasMaxLength(255)
                          .IsRequired();

                entity.Property(d => d.StoredName)
                          .HasColumnName("stored_name")
                          .HasMaxLength(255)
                          .IsRequired();

                entity.Property(d => d.FilePath)
                          .HasColumnName("file_path")
                          .IsRequired();

                entity.Property(d => d.ContentType)
                          .HasColumnName("content_type")
                          .HasMaxLength(150)
                          .IsRequired();

                entity.Property(d => d.FileSizeBytes)
                          .HasColumnName("file_size_bytes");

                entity.Property(d => d.Description)
                          .HasColumnName("description");

                entity.Property(d => d.Category)
                          .HasColumnName("category")
                          .HasMaxLength(100);

                entity.Property(d => d.IsViewedByDoctor)
                          .HasColumnName("is_viewed_by_doctor")
                          .HasDefaultValue(false);

                entity.Property(d => d.IsReviewed)
                          .HasColumnName("is_reviewed")
                          .HasDefaultValue(false);

                entity.Property(d => d.Feedback)
                          .HasColumnName("feedback");

                entity.Property(d => d.Severity)
                              .HasColumnName("severity")
                              .HasMaxLength(50)
                              .HasConversion(
                                  v => v.ToString().ToUpper(),
                                  v => (Severity)Enum.Parse(typeof(Severity), v, true)
                              );

                entity.Property(d => d.ReviewedAt)
                          .HasColumnName("reviewed_at");

                entity.HasOne(d => d.Patient)
                          .WithMany(p => p.MedicalDocuments)
                          .HasForeignKey(d => d.PatientId)
                          .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Doctor)
                          .WithMany(doc => doc.MedicalDocuments)
                          .HasForeignKey(d => d.DoctorId)
                          .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(d => d.PatientId)
                          .HasDatabaseName("idx_medical_documents_patient");

                entity.HasIndex(d => d.DoctorId)
                          .HasDatabaseName("idx_medical_documents_doctor");

                entity.HasIndex(d => new
                {
                    d.PatientId,
                    d.DoctorId
                })
                      .HasDatabaseName("idx_medical_documents_patient_doctor");
            });
            // ── DOCTOR_PATIENT ────────────────────────────────────────
            modelBuilder.Entity<DoctorPatient>(entity =>
            {
                entity.ToTable("doctor_patient");

                entity.Property(dp => dp.DoctorId).HasColumnName("doctor_id");
                entity.Property(dp => dp.PatientId).HasColumnName("patient_id").IsRequired();

                entity.HasIndex(dp => new { dp.DoctorId, dp.PatientId })
                      .IsUnique()
                      .HasDatabaseName("uq_doctor_patient");

                entity.HasOne(dp => dp.Doctor)
                      .WithMany(d => d.DoctorPatients)
                      .HasForeignKey(dp => dp.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(dp => dp.Patient)
                      .WithMany(p => p.DoctorPatients)
                      .HasForeignKey(dp => dp.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(dp => dp.ReviewedByAdminId)
                      .HasColumnName("reviewed_by_admin_id");

                entity.Property(dp => dp.Status)
                        .HasColumnName("status")
                        .HasMaxLength(20)
                        .HasDefaultValue(ConnectionStatus.PendingAdmin)
                        .HasConversion(
                            v => ToSnakeCase(v.ToString()),
                            v => (ConnectionStatus)Enum.Parse(typeof(ConnectionStatus), v.Replace("_", ""), true)
                        );

                entity.Property(dp => dp.AdminNote)
                      .HasColumnName("admin_note")
                      .HasMaxLength(200);

                entity.Property(dp => dp.RequestedAt)
                      .HasColumnName("requested_at")
                      .HasDefaultValueSql("now()");

                entity.Property(dp => dp.AdminReviewedAt)
                      .HasColumnName("admin_reviewed_at");

                entity.Property(dp => dp.LastReminderSentAt)
                      .HasColumnName("last_reminder_sent_at");

                entity.Property(dp => dp.AssignedAt)
                      .HasColumnName("assigned_at");

                entity.Property(dp => dp.RevokedAt).HasColumnName("revoked_at");
            });

            // ── METRIC_DEFINITIONS ────────────────────────────────────
            modelBuilder.Entity<MetricDefinition>(entity =>
            {
                entity.ToTable("metric_definitions");

                entity.Property(m => m.MetricType).HasColumnName("metric_type").HasMaxLength(50).IsRequired();

                entity.HasIndex(m => m.MetricType)
                      .IsUnique()
                      .HasDatabaseName("uq_metric_type");

                entity.Property(m => m.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
                entity.Property(m => m.DefaultUnit).HasColumnName("default_unit").HasMaxLength(30).IsRequired();
                entity.Property(m => m.NormalMin).HasColumnName("normal_min").HasPrecision(10, 2);
                entity.Property(m => m.NormalMax).HasColumnName("normal_max").HasPrecision(10, 2);
                entity.Property(m => m.Description).HasColumnName("description");
            });

            // ── HEALTH_METRICS ────────────────────────────────────────
            modelBuilder.Entity<HealthMetric>(entity =>
            {
                entity.ToTable("health_metrics");

                // GROUPING TAG (No longer an FK constraint) ──
                entity.Property(h => h.SubmissionId)
                          .HasColumnName("submission_id")
                          .IsRequired();

                // ORIGINAL METRIC DATA ──
                entity.Property(h => h.MetricType)
                          .HasColumnName("metric_type")
                          .HasMaxLength(50)
                          .IsRequired();

                entity.Property(h => h.Value)
                          .HasColumnName("value")
                          .HasPrecision(10, 2)
                          .IsRequired();

                entity.Property(h => h.Unit)
                          .HasColumnName("unit")
                          .HasMaxLength(30)
                          .IsRequired();

                //  NEW FLATTENED DATA (Migrated from Submission) ──
                entity.Property(h => h.PatientId)
                          .HasColumnName("patient_id")
                          .IsRequired();

                entity.Property(h => h.RecordedByUserId)
                          .HasColumnName("recorded_by_user_id")
                          .IsRequired();

                entity.Property(h => h.RecordedByRole)
                          .HasColumnName("recorded_by_role")
                          .HasMaxLength(50)
                          .IsRequired();

                entity.Property(h => h.RecordedAt)
                          .HasColumnName("recorded_at")
                          .IsRequired();

                entity.Property(h => h.Notes)
                          .HasColumnName("notes")
                          .HasMaxLength(1000); // Nullable, so no .IsRequired()

                entity.Property(h => h.Status)
                              .HasColumnName("status")
                              .HasMaxLength(50)
                              .HasDefaultValue(Severity.Normal)
                              .IsRequired()
                              .HasConversion(
                                  v => v.ToString().ToUpper(),
                                  v => (Severity)Enum.Parse(typeof(Severity), v, true)
                              );

                // ── RELATIONS ─────────────────────────────────

                // The old entity.HasOne(h => h.Submission) has been completely removed.

                entity.HasOne(h => h.MetricDefinition)
                          .WithMany(m => m.HealthMetrics)
                          .HasForeignKey(h => h.MetricType)
                          .HasPrincipalKey(m => m.MetricType)
                          .OnDelete(DeleteBehavior.Restrict);

                // New relation to Patient
                entity.HasOne(h => h.Patient)
                          .WithMany(p => p.HealthMetrics)
                          .HasForeignKey(h => h.PatientId)
                          .OnDelete(DeleteBehavior.Cascade);

                // New relation to the User who recorded it
                entity.HasOne(h => h.RecordedByUser)
                          .WithMany()
                          .HasForeignKey(h => h.RecordedByUserId)
                          .OnDelete(DeleteBehavior.Restrict); // Restrict prevents accidental user deletion from wiping medical records

                // ── INDEXES ───────────────────────────────────

                // Keep this! You will use it heavily to group records together in your Service layer
                entity.HasIndex(h => h.SubmissionId)
                          .HasDatabaseName("idx_hm_submission");

                entity.HasIndex(h => h.MetricType)
                          .HasDatabaseName("idx_hm_metric_type");

                // Critical for fast pagination and sorting by date
                entity.HasIndex(h => h.RecordedAt)
                          .HasDatabaseName("idx_hm_recorded_at");

                // Critical for loading a specific patient's dashboard quickly
                entity.HasIndex(h => h.PatientId)
                          .HasDatabaseName("idx_hm_patient_id");
            });

            // ── HEALTH_ALERTS ─────────────────────────────────────────
            modelBuilder.Entity<HealthAlert>(entity =>
            {
                entity.ToTable("health_alerts");

                entity.Property(a => a.HealthMetricId).HasColumnName("health_metric_id").IsRequired();
                entity.Property(a => a.PatientId).HasColumnName("patient_id").IsRequired();
                entity.Property(a => a.AlertType).HasColumnName("alert_type").HasMaxLength(50).IsRequired();
                entity.Property(a => a.Severity).HasColumnName("severity").HasMaxLength(20).IsRequired();
                entity.Property(a => a.IsAcknowledged).HasColumnName("is_acknowledged").HasDefaultValue(false);
                entity.Property(a => a.AcknowledgedBy).HasColumnName("acknowledged_by");
                entity.Property(a => a.TriggeredAt).HasColumnName("triggered_at").HasDefaultValueSql("now()");
                entity.Property(a => a.AcknowledgedAt).HasColumnName("acknowledged_at");

                entity.HasOne(a => a.HealthMetric)
                      .WithMany(h => h.HealthAlerts)
                      .HasForeignKey(a => a.HealthMetricId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.HealthAlerts)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.AcknowledgedByUser)
                      .WithMany()
                      .HasForeignKey(a => a.AcknowledgedBy)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");

                // ─────────────────────────────────────
                // COLUMNS
                // ─────────────────────────────────────

                entity.Property(n => n.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(n => n.Type)
                      .HasColumnName("type")
                      .HasMaxLength(20)
                      .IsRequired()
                      .HasConversion(
                          v => v.ToString().ToLower(),
                          v => (NotificationType)Enum.Parse(typeof(NotificationType), v, true)
                      );

                entity.Property(n => n.Message)
                    .HasColumnName("message")
                    .IsRequired();

                entity.Property(n => n.IsRead)
                    .HasColumnName("is_read")
                    .HasDefaultValue(false);

                entity.Property(n => n.ReadAt)
                    .HasColumnName("read_at");

                // ─────────────────────────────────────
                // RELATIONSHIPS
                // ─────────────────────────────────────

                entity.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // ─────────────────────────────────────
                // INDEXES
                // ─────────────────────────────────────

                entity.HasIndex(n => n.UserId)
                    .HasDatabaseName("idx_notifications_user");

                entity.HasIndex(n => new
                {
                    n.UserId,
                    n.IsRead
                })
                .HasDatabaseName("idx_notifications_user_read");

                entity.HasIndex(n => new
                {
                    n.UserId,
                    n.CreatedAt
                })
                .HasDatabaseName("idx_notifications_user_created");
            });

            // ── AUDIT_LOGS ────────────────────────────────────────────
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");

                entity.Property(a => a.PerformedBy).HasColumnName("performed_by").IsRequired();
                entity.Property(a => a.Action).HasColumnName("action").HasMaxLength(50).IsRequired();
                entity.Property(a => a.EntityType).HasColumnName("entity_type").HasMaxLength(50).IsRequired();
                entity.Property(a => a.EntityId).HasColumnName("entity_id").IsRequired();
                entity.Property(a => a.Changes).HasColumnName("changes").HasColumnType("jsonb");
                entity.Property(a => a.PerformedAt).HasColumnName("performed_at").HasDefaultValueSql("now()");

                entity.HasOne(a => a.PerformedByUser)
                      .WithMany()
                      .HasForeignKey(a => a.PerformedBy)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => new { a.PerformedBy, a.PerformedAt })
                      .HasDatabaseName("idx_audit_user_date");

                entity.HasIndex(a => a.EntityId)
                      .HasDatabaseName("idx_audit_entity");
            });
            modelBuilder.Entity<AppointmentSlot>(entity =>
            {
                entity.ToTable("appointment_slots");
                entity.Property(s => s.Status)
                    .HasConversion(
                        v => ToSnakeCase(v.ToString()),
                        v => (AppointmentSlotStatus)Enum.Parse(typeof(AppointmentSlotStatus), v.Replace("_", ""), true)
                    );
                entity.HasOne(s => s.Doctor)
                      .WithMany()
                      .HasForeignKey(s => s.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(s => new { s.DoctorId, s.StartTime });
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("appointments");
                entity.Property(a => a.Status)
                    .HasConversion(
                        v => ToSnakeCase(v.ToString()),
                        v => (AppointmentStatus)Enum.Parse(typeof(AppointmentStatus), v.Replace("_", ""), true)
                    );
                entity.HasOne(a => a.Slot)
                      .WithOne(s => s.Appointment)
                      .HasForeignKey<Appointment>(a => a.SlotId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                      .WithMany()
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Patient)
                      .WithMany()
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => a.DoctorId);
                entity.HasIndex(a => a.PatientId);
                entity.HasIndex(a => a.SlotId).IsUnique();
            });
            // ── PATIENT_AUDIT_LOGS ────────────────────────────────────
            modelBuilder.Entity<PatientAuditLog>(entity =>
            {
                entity.ToTable("patient_audit_logs");

                entity.Property(p => p.PatientId).HasColumnName("patient_id").IsRequired();
                entity.Property(p => p.ChangedByUserId).HasColumnName("changed_by_user_id").IsRequired();
                entity.Property(p => p.FieldName).HasColumnName("field_name").HasMaxLength(100).IsRequired();
                entity.Property(p => p.OldValue).HasColumnName("old_value");
                entity.Property(p => p.NewValue).HasColumnName("new_value");
                entity.Property(p => p.ChangedAt).HasColumnName("changed_at").HasDefaultValueSql("now()");

                entity.HasOne(p => p.Patient)
                      .WithMany(pt => pt.AuditLogs)
                      .HasForeignKey(p => p.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.ChangedByUser)
                      .WithMany()
                      .HasForeignKey(p => p.ChangedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => new { p.PatientId, p.ChangedAt })
                      .HasDatabaseName("idx_patient_audit_patient_date");
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("refresh_tokens");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Id)
                      .HasColumnName("id")
                      .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(r => r.UserId)
                      .HasColumnName("user_id")
                      .IsRequired();

                entity.Property(r => r.Token)
                      .HasColumnName("token")
                      .IsRequired();

                entity.HasIndex(r => r.Token)
                      .IsUnique()
                      .HasDatabaseName("uq_refresh_token");

                entity.Property(r => r.ExpiresAt)
                      .HasColumnName("expires_at")
                      .IsRequired();

                entity.Property(r => r.IsRevoked)
                      .HasColumnName("is_revoked")
                      .HasDefaultValue(false);

                entity.Property(r => r.CreatedAt)
                      .HasColumnName("created_at")
                      .HasDefaultValueSql("now()");

                entity.Property(r => r.RevokedAt)
                      .HasColumnName("revoked_at");

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(r => r.UserId)
                      .HasDatabaseName("idx_refresh_token_user");
            });
        }
    }
}