using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using MediScope.Common.Models.Enums;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Common.Models.Entities;

namespace MediScope.Data.Repositories
{
    public class MedicalDocumentRepository
        : IMedicalDocumentRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public MedicalDocumentRepository(
            AppDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task UploadDocumentAsync(
            MedicalDocument document)
        {
            string connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection")!;

            await using var connection =
                new NpgsqlConnection(
                    connectionString);

            await using var command =
                new NpgsqlCommand(
                    "sp_upload_document",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "p_patient_id",
                document.PatientId);

            command.Parameters.AddWithValue(
                "p_doctor_id",
                document.DoctorId);

            command.Parameters.AddWithValue(
                "p_file_name",
                document.FileName);

            command.Parameters.AddWithValue(
                "p_stored_name",
                document.StoredName);

            command.Parameters.AddWithValue(
                "p_file_path",
                document.FilePath);

            command.Parameters.AddWithValue(
                "p_content_type",
                document.ContentType);

            command.Parameters.AddWithValue(
                "p_file_size",
                document.FileSizeBytes);

            command.Parameters.AddWithValue(
                "p_description",
                (object?)document.Description
                ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "p_category",
                (object?)document.Category
                ?? DBNull.Value);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task AddFeedbackAsync(
            Guid documentId,
            string feedback,
            string? severity)
        {
            string connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection")!;

            await using var connection =
                new NpgsqlConnection(
                    connectionString);

            await using var command =
                new NpgsqlCommand(
                    "sp_review_document",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "p_document_id",
                documentId);

            command.Parameters.AddWithValue(
                "p_feedback",
                feedback);

            command.Parameters.AddWithValue(
                "p_severity",
                (object?)severity
                ?? DBNull.Value);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task MarkViewedAsync(
            Guid documentId)
        {
            string connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection")!;

            await using var connection =
                new NpgsqlConnection(
                    connectionString);

            await using var command =
                new NpgsqlCommand(
                    "sp_mark_document_viewed",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "p_document_id",
                documentId);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<MedicalDocumentResponseDto>> GetPatientDocumentsAsync(Guid patientId)
        {
            var result = new List<MedicalDocumentResponseDto>();

            await using var conn = new NpgsqlConnection(
                _context.Database.GetConnectionString());

            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM fn_get_patient_documents(@patientId)",
                conn);

            cmd.Parameters.AddWithValue("patientId", patientId);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new MedicalDocumentResponseDto
                {
                    Id = reader.GetGuid(reader.GetOrdinal("id")),
                    FileName = reader.GetString(reader.GetOrdinal("file_name")),
                    Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetFieldValue<string>("description"),
                    Category = reader.IsDBNull(reader.GetOrdinal("category")) ? null : reader.GetFieldValue<string>("category"),
                    DoctorName = reader.GetString(reader.GetOrdinal("doctor_name")),
                    IsViewedByDoctor = reader.GetFieldValue<bool>(reader.GetOrdinal("is_viewed_by_doctor")),
                    IsReviewed = reader.GetFieldValue<bool>(reader.GetOrdinal("is_reviewed")),
                    Feedback = reader.IsDBNull(reader.GetOrdinal("feedback")) ? null : reader.GetFieldValue<string>("feedback"),
                    Severity = reader.IsDBNull(reader.GetOrdinal("severity")) ? null : Enum.Parse<Severity>(reader.GetString(reader.GetOrdinal("severity")), true),
                    UploadedAt = reader.GetFieldValue<DateTime>(reader.GetOrdinal("uploaded_at")),
                    ReviewedAt = reader.IsDBNull(reader.GetOrdinal("reviewed_at")) ? null : reader.GetFieldValue<DateTime>("reviewed_at"),
                });
            }

            return result;
        }

        public async Task<List<DoctorDocumentResponseDto>> GetDoctorDocumentsAsync(Guid doctorId)
        {
            var result = new List<DoctorDocumentResponseDto>();

            await using var conn = new NpgsqlConnection(
                _context.Database.GetConnectionString());

            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                "SELECT * FROM fn_get_doctor_documents(@doctorId)",
                conn);

            cmd.Parameters.AddWithValue("doctorId", doctorId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new DoctorDocumentResponseDto
                {
                    Id = reader.GetFieldValue<Guid>("id"),
                    PatientId = reader.GetFieldValue<Guid>("patient_id"),
                    PatientName = reader.GetFieldValue<string>("patient_name"),
                    FileName = reader.GetFieldValue<string>("file_name"),

                    // Handle potentially null columns safely
                    Description = reader.IsDBNull(reader.GetOrdinal("description"))
                        ? null
                        : reader.GetFieldValue<string>("description"),

                    Category = reader.IsDBNull(reader.GetOrdinal("category"))
                        ? null
                        : reader.GetFieldValue<string>("category"),

                    UploadedAt = reader.GetFieldValue<DateTime>("uploaded_at"),
                    IsViewedByDoctor = reader.GetFieldValue<bool>("is_viewed_by_doctor"),
                    IsReviewed = reader.GetFieldValue<bool>("is_reviewed"),

                    Feedback = reader.IsDBNull(reader.GetOrdinal("feedback"))
                        ? null
                        : reader.GetFieldValue<string>("feedback"),

                    Severity = reader.IsDBNull(reader.GetOrdinal("severity")) ? null : Enum.Parse<Severity>(reader.GetString(reader.GetOrdinal("severity")), true),
                });
            }
            return result;
        }
        // Add to MedicalDocumentRepository.cs
        public async Task<MedicalDocument?> GetDocumentByIdAsync(Guid documentId)
        {
            // Using EF Core since you have _context injected
            return await _context.Set<MedicalDocument>()
                .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);
        }
    }
}