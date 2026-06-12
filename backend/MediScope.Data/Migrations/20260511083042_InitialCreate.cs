using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediScope.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "metric_definitions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    metric_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    default_unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    normal_min = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    normal_max = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_definitions", x => x.id);
                    table.UniqueConstraint("AK_metric_definitions_metric_type", x => x.metric_type);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.id);
                    table.ForeignKey(
                        name: "FK_admins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    performed_by = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changes = table.Column<string>(type: "jsonb", nullable: true),
                    performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_performed_by",
                        column: x => x.performed_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "doctors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    specialization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    license_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    contact_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bio = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctors", x => x.id);
                    table.ForeignKey(
                        name: "FK_doctors_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "patients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    blood_group = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    contact_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    consent_profile_visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.id);
                    table.ForeignKey(
                        name: "FK_patients_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "doctor_patient",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_patient", x => x.id);
                    table.ForeignKey(
                        name: "FK_doctor_patient_doctors_doctor_id",
                        column: x => x.doctor_id,
                        principalTable: "doctors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_doctor_patient_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "health_metrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recorded_by_role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    metric_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_metrics", x => x.id);
                    table.ForeignKey(
                        name: "FK_health_metrics_metric_definitions_metric_type",
                        column: x => x.metric_type,
                        principalTable: "metric_definitions",
                        principalColumn: "metric_type",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_health_metrics_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_health_metrics_users_recorded_by_user_id",
                        column: x => x.recorded_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "patient_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    changed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    old_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                    changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_patient_audit_logs_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_patient_audit_logs_users_changed_by_user_id",
                        column: x => x.changed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "health_alerts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    health_metric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alert_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_acknowledged = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    acknowledged_by = table.Column<Guid>(type: "uuid", nullable: true),
                    triggered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    acknowledged_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_health_alerts", x => x.id);
                    table.ForeignKey(
                        name: "FK_health_alerts_health_metrics_health_metric_id",
                        column: x => x.health_metric_id,
                        principalTable: "health_metrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_health_alerts_patients_patient_id",
                        column: x => x.patient_id,
                        principalTable: "patients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_health_alerts_users_acknowledged_by",
                        column: x => x.acknowledged_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "uq_admins_user_id",
                table: "admins",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_audit_entity",
                table: "audit_logs",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "idx_audit_user_date",
                table: "audit_logs",
                columns: new[] { "performed_by", "performed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_doctor_patient_patient_id",
                table: "doctor_patient",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "uq_doctor_patient",
                table: "doctor_patient",
                columns: new[] { "doctor_id", "patient_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_doctors_license",
                table: "doctors",
                column: "license_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_doctors_user_id",
                table: "doctors",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_health_alerts_acknowledged_by",
                table: "health_alerts",
                column: "acknowledged_by");

            migrationBuilder.CreateIndex(
                name: "IX_health_alerts_health_metric_id",
                table: "health_alerts",
                column: "health_metric_id");

            migrationBuilder.CreateIndex(
                name: "IX_health_alerts_patient_id",
                table: "health_alerts",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "idx_hm_patient_type_date",
                table: "health_metrics",
                columns: new[] { "patient_id", "metric_type", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_health_metrics_metric_type",
                table: "health_metrics",
                column: "metric_type");

            migrationBuilder.CreateIndex(
                name: "IX_health_metrics_recorded_by_user_id",
                table: "health_metrics",
                column: "recorded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "uq_metric_type",
                table: "metric_definitions",
                column: "metric_type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_patient_audit_patient_date",
                table: "patient_audit_logs",
                columns: new[] { "patient_id", "changed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_patient_audit_logs_changed_by_user_id",
                table: "patient_audit_logs",
                column: "changed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "uq_patients_user_id",
                table: "patients",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "doctor_patient");

            migrationBuilder.DropTable(
                name: "health_alerts");

            migrationBuilder.DropTable(
                name: "patient_audit_logs");

            migrationBuilder.DropTable(
                name: "doctors");

            migrationBuilder.DropTable(
                name: "health_metrics");

            migrationBuilder.DropTable(
                name: "metric_definitions");

            migrationBuilder.DropTable(
                name: "patients");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
