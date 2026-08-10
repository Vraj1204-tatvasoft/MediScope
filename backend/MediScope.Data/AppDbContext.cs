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
        public DbSet<PatientAuditLog> PatientAuditLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<MedicalDocument> MedicalDocuments { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<BillingItem> BillingItems { get; set; }
        public DbSet<PatientCardToken> PatientCardTokens { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<PatientAdmission> PatientAdmissions { get; set; }
        public DbSet<BedTransferHistory> BedTransferHistories { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<QuestionnaireQuestion> QuestionnaireQuestions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<QuestionnaireSubmission> QuestionnaireSubmissions { get; set; }
        public DbSet<SubmissionResponse> SubmissionResponses { get; set; }
        public DbSet<QuestionnaireAssignment> QuestionnaireAssignments { get; set; }
        public DbSet<Broadcast> Broadcasts { get; set; }
        public DbSet<BroadcastRecipient> BroadcastRecipients { get; set; }
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

                entity.Property(p => p.RazorpayCustomerId)
                          .HasColumnName("razorpay_customer_id")
                          .HasMaxLength(50);

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

                entity.Property(d => d.ExtractedText)
                          .HasColumnName("extracted_text")
                          .HasColumnType("jsonb");

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
                entity.Property(h => h.AppointmentId)
                          .HasColumnName("appointment_id")
                          .IsRequired(false);

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
                entity.HasOne(h => h.Appointment)
                          .WithMany(a => a.HealthMetrics)
                          .HasForeignKey(h => h.AppointmentId)
                          .OnDelete(DeleteBehavior.SetNull);

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
                entity.HasIndex(h => h.AppointmentId)
                          .HasDatabaseName("idx_hm_appointment_id");

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

                entity.Property(n => n.ReferenceType)
                    .HasColumnName("reference_type")
                    .HasMaxLength(50);

                entity.Property(n => n.ReferenceId)
                      .HasColumnName("reference_id");
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
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("appointments");

                entity.Property(a => a.DoctorId).HasColumnName("doctor_id");
                entity.Property(a => a.PatientId).HasColumnName("patient_id");

                entity.Property(a => a.StartTime).HasColumnName("start_time");
                entity.Property(a => a.EndTime).HasColumnName("end_time");
                entity.Property(a => a.DurationMinutes).HasColumnName("duration_minutes");

                entity.Property(a => a.DoctorNotes).HasColumnName("doctor_notes");
                entity.Property(a => a.PatientNotes).HasColumnName("patient_notes");
                entity.Property(a => a.RescheduleRequestedBy).HasColumnName("reschedule_requested_by");
                entity.Property(a => a.RescheduledTo).HasColumnName("rescheduled_to");
                entity.Property(a => a.RescheduleReason).HasColumnName("reschedule_reason");

                entity.Property(a => a.ReminderSent)
                    .HasColumnName("reminder_sent")
                    .HasDefaultValue(false);

                entity.Property(a => a.Status)
                      .HasColumnName("status")
                      .HasConversion(
                          v => ToSnakeCase(v.ToString()),
                          v => (AppointmentStatus)Enum.Parse(typeof(AppointmentStatus), v.Replace("_", ""), true)
                      );

                entity.HasOne(a => a.Doctor)
                      .WithMany()
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Patient)
                      .WithMany()
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => new { a.DoctorId, a.StartTime });
                entity.HasIndex(a => a.PatientId);
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

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("invoices");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
                entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
                entity.Property(e => e.PatientId).HasColumnName("patient_id");
                entity.Property(e => e.InvoiceDate).HasColumnName("invoice_date");
                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.SubTotal).HasColumnName("sub_total").HasColumnType("numeric(18,2)");
                entity.Property(e => e.TotalDiscount).HasColumnName("total_discount").HasColumnType("numeric(18,2)");
                entity.Property(e => e.TotalTax).HasColumnName("total_tax").HasColumnType("numeric(18,2)");
                entity.Property(e => e.GrandTotal).HasColumnName("grand_total").HasColumnType("numeric(18,2)");
                entity.Property(e => e.TotalPaid).HasColumnName("total_paid").HasColumnType("numeric(18,2)");

                entity.HasMany(i => i.InvoiceItems)
                      .WithOne(item => item.Invoice)
                      .HasForeignKey(item => item.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(i => i.Payments)
                      .WithOne(p => p.Invoice)
                      .HasForeignKey(p => p.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Refund>(entity =>
            {
                entity.ToTable("refunds");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PaymentId).HasColumnName("payment_id");
                entity.Property(e => e.RefundDate).HasColumnName("refund_date");
                entity.Property(e => e.RefundMode).HasColumnName("refund_mode").HasMaxLength(50);
                entity.Property(e => e.RefundAmount).HasColumnName("refund_amount").HasColumnType("numeric(18,2)");
                entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(255);

                entity.HasIndex(e => e.PaymentId).IsUnique();
            });

            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.ToTable("invoice_items");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
                entity.Property(e => e.BillingItemId).HasColumnName("billing_item_id");
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);
                entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("numeric(18,2)");
                entity.Property(e => e.Discount).HasColumnName("discount").HasColumnType("numeric(18,2)");
                entity.Property(e => e.IsTax).HasColumnName("is_tax");
                entity.Property(e => e.Tax).HasColumnName("tax").HasColumnType("numeric(18,2)");
                entity.Property(e => e.Total).HasColumnName("total").HasColumnType("numeric(18,2)");
                entity.HasOne(i => i.BillingItem)
                            .WithMany(b => b.InvoiceItems)
                            .HasForeignKey(i => i.BillingItemId)
                            .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
                entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
                entity.Property(e => e.PaymentMode).HasColumnName("payment_mode").HasMaxLength(50);
                entity.Property(e => e.RazorpayPaymentId).HasColumnName("razorpay_payment_id").HasMaxLength(50);
                entity.Property(e => e.PaymentAmount).HasColumnName("payment_amount").HasColumnType("numeric(18,2)");
                entity.HasOne(p => p.Refund).WithOne(r => r.Payment).HasForeignKey<Refund>(r => r.PaymentId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<BillingItem>(entity =>
            {
                entity.ToTable("billing_items");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ItemName).HasColumnName("item_name").HasMaxLength(255);
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500);

                entity.Property(e => e.DefaultAmount).HasColumnName("default_amount").HasColumnType("numeric(18,2)");
                entity.Property(e => e.IsTaxable).HasColumnName("is_taxable");
                entity.Property(e => e.IsActive).HasColumnName("is_active");

                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<PatientCardToken>(entity =>
            {
                entity.ToTable("patient_card_tokens");

                entity.HasKey(t => t.Id);
                entity.Property(t => t.Id)
                            .HasColumnName("id");

                entity.Property(t => t.PatientId)
                            .HasColumnName("patient_id")
                            .IsRequired();

                entity.Property(t => t.RazorpayTokenId)
                            .HasColumnName("razorpay_token_id")
                            .HasMaxLength(100)
                            .IsRequired();

                entity.Property(t => t.Last4Digits)
                            .HasColumnName("last_4_digits")
                            .HasMaxLength(4)
                            .IsRequired();

                entity.Property(t => t.CardNetwork)
                            .HasColumnName("card_network")
                            .HasMaxLength(30)
                            .IsRequired();

                entity.Property(t => t.IsActive)
                            .HasColumnName("is_active")
                            .HasDefaultValue(true);

                entity.Property(t => t.CreatedAt)
                            .HasColumnName("created_at");

                entity.Property(t => t.UpdatedAt)
                            .HasColumnName("updated_at");

                entity.Property(t => t.IsDeleted)
                            .HasColumnName("is_deleted")
                            .HasDefaultValue(false);

                entity.HasOne(t => t.Patient)
                            .WithMany()
                            .HasForeignKey(t => t.PatientId)
                            .HasConstraintName("fk_card_tokens_patient");

                entity.HasIndex(t => t.PatientId)
                            .HasDatabaseName("ix_card_tokens_patient_id");
            });
            modelBuilder.Entity<Ward>(entity =>
            {
                entity.ToTable("wards");

                entity.Property(w => w.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
                entity.Property(w => w.Description).HasColumnName("description");
            });
            modelBuilder.Entity<RoomType>(entity =>
            {
                entity.ToTable("room_types");

                entity.Property(rt => rt.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            });
            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("rooms");

                entity.Property(r => r.RoomNumber).HasColumnName("room_number").HasMaxLength(50).IsRequired();
                entity.Property(r => r.WardId).HasColumnName("ward_id");
                entity.Property(r => r.RoomTypeId).HasColumnName("room_type_id");
                entity.Property(r => r.Floor).HasColumnName("floor");
                entity.HasOne(r => r.Ward)
                      .WithMany(w => w.Rooms)
                      .HasForeignKey(r => r.WardId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.RoomType)
                      .WithMany(rt => rt.Rooms)
                      .HasForeignKey(r => r.RoomTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(r => r.WardId);
                entity.HasIndex(r => r.RoomTypeId);
                entity.HasIndex(r => r.RoomNumber).IsUnique();
            });
            modelBuilder.Entity<Bed>(entity =>
            {
                entity.ToTable("beds");

                entity.Property(b => b.BedNumber).HasColumnName("bed_number").HasMaxLength(50).IsRequired();
                entity.Property(b => b.RoomId).HasColumnName("room_id");

                entity.Property(b => b.Status)
                      .HasColumnName("status")
                      .HasDefaultValue(BedStatus.Available);

                entity.HasOne(b => b.Room)
                      .WithMany(r => r.Beds)
                      .HasForeignKey(b => b.RoomId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(b => b.RoomId);
                entity.HasIndex(b => new { b.RoomId, b.BedNumber }).IsUnique();
            });

            modelBuilder.Entity<PatientAdmission>(e =>
            {
                e.ToTable("patient_admissions");
                e.HasKey(a => a.Id);
                e.Property(a => a.AdmissionNumber).HasColumnName("admission_number").HasMaxLength(20).IsRequired();
                e.Property(a => a.PatientId).HasColumnName("patient_id");
                e.Property(a => a.DoctorId).HasColumnName("doctor_id");
                e.Property(a => a.WardId).HasColumnName("ward_id");
                e.Property(a => a.RoomId).HasColumnName("room_id");
                e.Property(a => a.BedId).HasColumnName("bed_id");
                e.Property(a => a.AdmissionDate).HasColumnName("admission_date");
                e.Property(a => a.AdmissionReason).HasColumnName("admission_reason");
                e.Property(a => a.ExpectedDischargeDate).HasColumnName("expected_discharge_date");
                e.Property(a => a.ActualDischargeDate).HasColumnName("actual_discharge_date");
                e.Property(a => a.DischargeNotes).HasColumnName("discharge_notes");
                e.Property(a => a.Remarks).HasColumnName("remarks");
                e.Property(a => a.Status).HasColumnName("status");
                e.Property(a => a.CreatedAt).HasColumnName("created_at");
                e.Property(a => a.UpdatedAt).HasColumnName("updated_at");
                e.Property(a => a.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
                e.HasIndex(a => a.AdmissionNumber).IsUnique().HasDatabaseName("uq_admission_number");
                e.HasOne(a => a.Patient).WithMany(p => p.PatientAdmissions).HasForeignKey(a => a.PatientId);
                e.HasOne(a => a.Doctor).WithMany().HasForeignKey(a => a.DoctorId);
                e.HasOne(a => a.Ward).WithMany().HasForeignKey(a => a.WardId);
                e.HasOne(a => a.Room).WithMany().HasForeignKey(a => a.RoomId);
                e.HasOne(a => a.Bed).WithMany().HasForeignKey(a => a.BedId);
            });

            modelBuilder.Entity<BedTransferHistory>(e =>
            {
                e.ToTable("bed_transfer_history");
                e.HasKey(t => t.Id);
                e.Property(t => t.AdmissionId).HasColumnName("admission_id");
                e.Property(t => t.FromWardId).HasColumnName("from_ward_id");
                e.Property(t => t.FromRoomId).HasColumnName("from_room_id");
                e.Property(t => t.FromBedId).HasColumnName("from_bed_id");
                e.Property(t => t.ToWardId).HasColumnName("to_ward_id");
                e.Property(t => t.ToRoomId).HasColumnName("to_room_id");
                e.Property(t => t.ToBedId).HasColumnName("to_bed_id");
                e.Property(t => t.TransferDate).HasColumnName("transfer_date");
                e.Property(t => t.TransferReason).HasColumnName("transfer_reason");
                e.Property(t => t.CreatedAt).HasColumnName("created_at");
                e.Property(t => t.CreatedBy).HasColumnName("created_by");
                e.HasOne(t => t.Admission).WithMany(a => a.TransferHistory).HasForeignKey(t => t.AdmissionId);
                e.HasOne(t => t.FromBed).WithMany().HasForeignKey(t => t.FromBedId);
                e.HasOne(t => t.ToBed).WithMany().HasForeignKey(t => t.ToBedId);
            });

            modelBuilder.Entity<Questionnaire>(entity =>
      {
          entity.ToTable("questionnaires");

          entity.Property(q => q.Name)
              .HasColumnName("name")
              .HasMaxLength(255)
              .IsRequired();

          entity.Property(q => q.Description)
              .HasColumnName("description")
              .HasColumnType("text");

          entity.Property(q => q.Department)
              .HasColumnName("department")
              .HasMaxLength(150);

          entity.Property(q => q.Status)
              .HasColumnName("status")
              .HasConversion<string>()      // stores "Active" / "Inactive" — same pattern as your Role field
              .HasDefaultValue(QuestionnaireStatus.Active)
              .IsRequired();

          entity.HasIndex(q => new { q.Status, q.IsDeleted })
              .HasDatabaseName("idx_questionnaires_status_deleted");

          entity.HasMany(q => q.Questions)
              .WithOne(qq => qq.Questionnaire)
              .HasForeignKey(qq => qq.QuestionnaireId)
              .OnDelete(DeleteBehavior.Restrict);

          entity.HasMany(q => q.Submissions)
              .WithOne(s => s.Questionnaire)
              .HasForeignKey(s => s.QuestionnaireId)
              .OnDelete(DeleteBehavior.Restrict);
      });

            modelBuilder.Entity<QuestionnaireQuestion>(entity =>
            {
                entity.ToTable("questionnaire_questions");

                entity.Property(q => q.QuestionnaireId)
                    .HasColumnName("questionnaire_id")
                    .IsRequired();

                entity.Property(q => q.Label)
                    .HasColumnName("label")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(q => q.FieldType)
                    .HasColumnName("field_type")
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(q => q.Placeholder)
                    .HasColumnName("placeholder")
                    .HasMaxLength(300);

                entity.Property(q => q.IsRequired)
                    .HasColumnName("is_required")
                    .HasDefaultValue(false);

                entity.Property(q => q.DisplayOrder)
                    .HasColumnName("display_order")
                    .HasDefaultValue(0);

                entity.Property(q => q.DefaultValue)
                    .HasColumnName("default_value")
                    .HasColumnType("text");

                entity.Property(q => q.MinValue)
                    .HasColumnName("min_value");

                entity.Property(q => q.MaxValue)
                    .HasColumnName("max_value");

                entity.Property(q => q.MinLength)
                    .HasColumnName("min_length");

                entity.Property(q => q.MaxLength)
                    .HasColumnName("max_length");

                entity.Property(q => q.RegexPattern)
                    .HasColumnName("regex_pattern")
                    .HasColumnType("text");

                entity.HasIndex(q => q.QuestionnaireId)
                    .HasDatabaseName("idx_qq_questionnaire_id");

                entity.HasOne(q => q.Questionnaire)
                    .WithMany(qn => qn.Questions)
                    .HasForeignKey(q => q.QuestionnaireId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(q => q.Options)
                    .WithOne(o => o.Question)
                    .HasForeignKey(o => o.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(q => q.Responses)
                    .WithOne(r => r.Question)
                    .HasForeignKey(r => r.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<QuestionOption>(entity =>
            {
                entity.ToTable("question_options");

                entity.HasKey(o => o.Id);

                entity.Property(o => o.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(o => o.QuestionId)
                    .HasColumnName("question_id")
                    .IsRequired();

                entity.Property(o => o.OptionLabel)
                    .HasColumnName("option_label")
                    .HasMaxLength(300)
                    .IsRequired();

                entity.Property(o => o.OptionValue)
                    .HasColumnName("option_value")
                    .HasMaxLength(300)
                    .IsRequired();

                entity.Property(o => o.DisplayOrder)
                    .HasColumnName("display_order")
                    .HasDefaultValue(0);

                entity.HasIndex(o => o.QuestionId)
                    .HasDatabaseName("idx_qo_question_id");

                entity.HasOne(o => o.Question)
                    .WithMany(q => q.Options)
                    .HasForeignKey(o => o.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<QuestionnaireSubmission>(entity =>
            {
                entity.ToTable("questionnaire_submissions");

                entity.Property(s => s.QuestionnaireId)
                    .HasColumnName("questionnaire_id")
                    .IsRequired();

                entity.Property(s => s.PatientId)
                    .HasColumnName("patient_id")
                    .IsRequired();

                entity.Property(s => s.AssignmentId)
                    .HasColumnName("assignment_id");

                entity.Property(s => s.SubmittedBy)
                    .HasColumnName("submitted_by")
                    .IsRequired();

                entity.Property(s => s.Status)
                    .HasColumnName("status")
                    .HasMaxLength(20)
                    .HasDefaultValue("Draft")
                    .IsRequired();

                entity.Property(s => s.VersionNumber)
                    .HasColumnName("version_number")
                    .HasDefaultValue(1)
                    .IsRequired();

                entity.Property(s => s.Notes)
                    .HasColumnName("notes")
                    .HasColumnType("text");

                entity.Property(s => s.SubmittedAt)
                    .HasColumnName("submitted_at")
                    .HasColumnType("timestamptz");

                entity.Property(s => s.PdfPath)
                    .HasColumnName("pdf_path")
                    .HasColumnType("text");

                entity.HasIndex(s => s.PatientId)
                    .HasDatabaseName("idx_qs_patient_id");

                entity.HasIndex(s => s.QuestionnaireId)
                    .HasDatabaseName("idx_qs_questionnaire_id");

                entity.HasIndex(s => s.AssignmentId)
                    .HasDatabaseName("idx_qs_assignment_id");

                entity.HasOne(s => s.Questionnaire)
                    .WithMany(q => q.Submissions)
                    .HasForeignKey(s => s.QuestionnaireId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Patient)
                    .WithMany()
                    .HasForeignKey(s => s.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Assignment)
                    .WithMany(a => a.Submissions)
                    .HasForeignKey(s => s.AssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(s => s.Responses)
                    .WithOne(r => r.Submission)
                    .HasForeignKey(r => r.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SubmissionResponse>(entity =>
            {
                entity.ToTable("submission_responses");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(r => r.SubmissionId)
                    .HasColumnName("submission_id")
                    .IsRequired();

                entity.Property(r => r.QuestionId)
                    .HasColumnName("question_id")
                    .IsRequired();

                entity.Property(r => r.ResponseValue)
                    .HasColumnName("response_value")
                    .HasColumnType("text");

                entity.Property(r => r.ResponseValues)
                    .HasColumnName("response_values")
                    .HasColumnType("text[]");

                entity.HasIndex(r => r.SubmissionId)
                    .HasDatabaseName("idx_sr_submission_id");

                entity.HasOne(r => r.Submission)
                    .WithMany(s => s.Responses)
                    .HasForeignKey(r => r.SubmissionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Question)
                    .WithMany(q => q.Responses)
                    .HasForeignKey(r => r.QuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<QuestionnaireAssignment>(entity =>
            {
                entity.ToTable("questionnaire_assignments");

                entity.Property(a => a.QuestionnaireId)
                    .HasColumnName("questionnaire_id")
                    .IsRequired();

                entity.Property(a => a.PatientId)
                    .HasColumnName("patient_id")
                    .IsRequired();

                entity.Property(a => a.AssignedBy)
                    .HasColumnName("assigned_by")
                    .IsRequired();

                entity.Property(a => a.Notes)
                    .HasColumnName("notes")
                    .HasColumnType("text");

                entity.HasIndex(a => a.PatientId)
                    .HasDatabaseName("idx_qa_patient_id");

                entity.HasIndex(a => a.QuestionnaireId)
                    .HasDatabaseName("idx_qa_questionnaire_id");

                entity.HasIndex(a => a.AssignedBy)
                    .HasDatabaseName("idx_qa_assigned_by");

                entity.HasOne(a => a.Questionnaire)
                    .WithMany()
                    .HasForeignKey(a => a.QuestionnaireId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Patient)
                    .WithMany()
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(a => a.Submissions)
                    .WithOne(s => s.Assignment)
                    .HasForeignKey(s => s.AssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Broadcast>(entity =>
            {
                entity.ToTable("broadcasts");

                entity.Property(e => e.Name)
                      .HasColumnName("name")
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.Channel)
                      .HasColumnName("channel");

                entity.Property(e => e.Subject)
                      .HasColumnName("subject")
                      .HasMaxLength(500);

                entity.Property(e => e.Message)
                      .HasColumnName("message")
                      .IsRequired();

                entity.Property(e => e.Audience)
                      .HasColumnName("audience");

                entity.Property(e => e.Status)
                      .HasColumnName("status");

                entity.Property(e => e.TotalRecipients)
                      .HasColumnName("total_recipients")
                      .HasDefaultValue(0);

                entity.Property(e => e.SentCount)
                      .HasColumnName("sent_count")
                      .HasDefaultValue(0);

                entity.Property(e => e.FailedCount)
                      .HasColumnName("failed_count")
                      .HasDefaultValue(0);

                entity.Property(e => e.HangfireJobId)
                      .HasColumnName("hangfire_job_id")
                      .HasMaxLength(100);

                entity.Property(e => e.BatchSize)
                      .HasColumnName("batch_size")
                      .HasDefaultValue(100);

                entity.Property(e => e.ScheduledAt)
                      .HasColumnName("scheduled_at");

                entity.Property(e => e.StartedAt)
                      .HasColumnName("started_at");

                entity.Property(e => e.CompletedAt)
                      .HasColumnName("completed_at");

                entity.Property(e => e.FailureReason)
                      .HasColumnName("failure_reason");

                entity.Property(e => e.RemainingBatches)
                      .HasColumnName("remaining_batches");

                entity.HasMany(b => b.Recipients)
                    .WithOne(r => r.Broadcast)
                    .HasForeignKey(r => r.BroadcastId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<BroadcastRecipient>(entity =>
            {
                entity.ToTable("broadcast_recipients");

                entity.Property(e => e.BroadcastId)
                      .HasColumnName("broadcast_id");

                entity.Property(e => e.UserId)
                      .HasColumnName("user_id");

                entity.Property(e => e.FullName)
                      .HasColumnName("full_name")
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(e => e.Email)
                      .HasColumnName("email")
                      .HasMaxLength(320);

                entity.Property(e => e.Status)
                      .HasColumnName("status");

                entity.Property(e => e.SentAt)
                      .HasColumnName("sent_at");

                entity.Property(e => e.ErrorMessage)
                      .HasColumnName("error_message")
                      .HasMaxLength(1000);

                entity.Property(e => e.RetryCount)
                      .HasColumnName("retry_count")
                      .HasDefaultValue(0);

                entity.Property(e => e.BatchNumber)
                      .HasColumnName("batch_number")
                      .HasDefaultValue(1);

                entity.HasOne(r => r.Broadcast)
                      .WithMany(b => b.Recipients)
                      .HasForeignKey(r => r.BroadcastId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}