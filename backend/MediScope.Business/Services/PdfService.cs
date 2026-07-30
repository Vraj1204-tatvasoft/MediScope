using MediScope.Business.Services.Interfaces;
using MediScope.Common.Models.DTOs.Response;
using MediScope.Data.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MediScope.Business.Services
{
    public class PdfService : IPdfService
    {
        private readonly IQuestionnaireAssignmentRepository _repository;
        private readonly IHostEnvironment _env;
        private readonly ILogger<PdfService> _logger;

        // ── Design tokens ──────────────────────────────────────────────────────
        private static readonly string PrimaryColor = "#0d9488";   // teal-600
        private static readonly string PrimaryDark = "#0f766e";   // teal-700
        private static readonly string PrimaryLight = "#f0fdfa";   // teal-50
        private static readonly string TextPrimary = "#111827";   // gray-900
        private static readonly string TextSecondary = "#6b7280";   // gray-500
        private static readonly string BorderColor = "#e5e7eb";   // gray-200
        private static readonly string AnswerBg = "#f9fafb";   // gray-50
        private static readonly string RequiredColor = "#f59e0b";   // amber-500

        public PdfService(
            IQuestionnaireAssignmentRepository repository,
            IHostEnvironment env,
            ILogger<PdfService> logger)
        {
            _repository = repository;
            _env = env;
            _logger = logger;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GenerateSubmissionPdfAsync(Guid submissionId, Guid patientId)
        {
            // 1. Fetch full submission detail
            var detail = await _repository.GetSubmissionDetailAsync(submissionId)
                ?? throw new InvalidOperationException(
                    $"Submission {submissionId} not found for PDF generation.");

            // 2. Build paths
            var relativePath = Path.Combine(
                "uploads", "questionnaires",
                detail.SubmittedByName,
                $"{submissionId}.pdf");

            var absolutePath = Path.Combine(
                _env.ContentRootPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

            // 3. Generate PDF
            var document = new SubmissionPdfDocument(detail);
            document.GeneratePdf(absolutePath);

            _logger.LogInformation(
                "PDF generated for submission {SubmissionId} at {Path}",
                submissionId, relativePath);

            // Return relative path — stored in DB
            return relativePath;
        }
    }

    // ── QuestPDF Document Definition ───────────────────────────────────────────

    internal class SubmissionPdfDocument : IDocument
    {
        private readonly SubmissionDetailResponseDto _data;

        // Colours
        private static readonly string Primary = "#0d9488";
        private static readonly string PrimaryDark = "#0f766e";
        private static readonly string PrimaryLight = "#e6f7f5";
        private static readonly string TextPrimary = "#111827";
        private static readonly string TextMuted = "#6b7280";
        private static readonly string BorderGray = "#e5e7eb";
        private static readonly string AnswerBg = "#f9fafb";

        public SubmissionPdfDocument(SubmissionDetailResponseDto data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => new()
        {
            Title = $"{_data.QuestionnaireName} — Submission",
            Author = "MediScope",
            Subject = "Questionnaire Submission",
            Keywords = "questionnaire, medical, submission",
            CreationDate = _data.SubmittedAt ?? DateTime.UtcNow,
        };

        public DocumentSettings GetSettings() => new()
        {
            PdfA = false,
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36, Unit.Point);
                page.DefaultTextStyle(ts => ts
                    .FontFamily("Arial")
                    .FontSize(10)
                    .FontColor(TextPrimary));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container
                .PaddingBottom(16)
                .Column(col =>
                {
                    // Top banner
                    col.Item()
                        .Background(Primary)
                        .Padding(16)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item()
                                    .Text("MediScope")
                                    .FontSize(18)
                                    .Bold()
                                    .FontColor(Colors.White);

                                inner.Item()
                                    .Text("Questionnaire Submission Report")
                                    .FontSize(11)
                                    .FontColor("#b2dfdb");
                            });

                            row.AutoItem()
                                .AlignRight()
                                .AlignMiddle()
                                .Column(inner =>
                                {
                                    inner.Item()
                                        .Text("SUBMITTED")
                                        .FontSize(9)
                                        .Bold()
                                        .FontColor("#b2dfdb")
                                        .LetterSpacing(0.1f);

                                    inner.Item()
                                        .Text(_data.SubmittedAt?.ToString("dd MMM yyyy") ?? "—")
                                        .FontSize(12)
                                        .Bold()
                                        .FontColor(Colors.White);
                                });
                        });

                    // Questionnaire name
                    col.Item()
                        .Background(PrimaryLight)
                        .BorderLeft(4).BorderColor(Primary)
                        .Padding(12)
                        .Column(inner =>
                        {
                            inner.Item()
                                .Text(_data.QuestionnaireName)
                                .FontSize(15)
                                .Bold()
                                .FontColor(TextPrimary);

                            if (!string.IsNullOrWhiteSpace(_data.Department))
                            {
                                inner.Item()
                                    .PaddingTop(2)
                                    .Text(_data.Department)
                                    .FontSize(9)
                                    .FontColor(TextMuted);
                            }
                        });
                });
        }

        // ── Content ────────────────────────────────────────────────────────────
        private void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                // ── Submission metadata card ──────────────────────────────────
                col.Item()
                    .PaddingBottom(20)
                    .Border(1).BorderColor(BorderGray)
                    .Padding(14)
                    .Column(meta =>
                    {
                        meta.Item()
                            .PaddingBottom(8)
                            .Text("Submission Details")
                            .FontSize(11)
                            .Bold()
                            .FontColor(TextPrimary);

                        meta.Item()
                            .BorderTop(1).BorderColor(BorderGray)
                            .PaddingTop(8);

                        meta.Item().Row(row =>
                        {
                            MetaField(row.RelativeItem(), "Submission ID",
                                _data.SubmissionId.ToString()[..8].ToUpper() + "...");
                            MetaField(row.RelativeItem(), "Submitted By",
                                _data.SubmittedByName);
                            MetaField(row.RelativeItem(), "Submitted At",
                                _data.SubmittedAt?.ToString("dd MMM yyyy, HH:mm") ?? "—");
                            MetaField(row.RelativeItem(), "Status",
                                _data.Status.ToUpper());
                        });

                        if (!string.IsNullOrWhiteSpace(_data.Notes))
                        {
                            meta.Item()
                                .PaddingTop(10)
                                .Column(n =>
                                {
                                    n.Item()
                                        .Text("Notes")
                                        .FontSize(9)
                                        .FontColor(TextMuted);
                                    n.Item()
                                        .PaddingTop(2)
                                        .Text(_data.Notes)
                                        .FontSize(10)
                                        .Italic();
                                });
                        }
                    });

                // ── Questions & Answers ───────────────────────────────────────
                col.Item()
                    .Text("Responses")
                    .FontSize(12)
                    .Bold()
                    .FontColor(TextPrimary);

                col.Item()
                    .PaddingTop(2)
                    .PaddingBottom(12)
                    .BorderBottom(1).BorderColor(Primary)
                    .Text($"{_data.Responses.Count} question(s)")
                    .FontSize(9)
                    .FontColor(TextMuted);

                var ordered = _data.Responses
                    .OrderBy(r => r.DisplayOrder)
                    .ToList();

                for (int i = 0; i < ordered.Count; i++)
                {
                    var response = ordered[i];
                    var isLast = i == ordered.Count - 1;

                    col.Item()
                        .PaddingBottom(isLast ? 0 : 12)
                        .Element(q => ComposeQuestionBlock(q, i + 1, response));
                }
            });
        }

        // ── Individual Q&A block ───────────────────────────────────────────────
        private void ComposeQuestionBlock(
            IContainer container, int number,
            SubmissionResponseItemDto response)
        {
            var answer = GetAnswerText(response);
            var hasAnswer = !string.IsNullOrWhiteSpace(answer)
                            && answer != "—";

            container
                .Border(1)
                .BorderColor(BorderGray)
                .Column(col =>
                {
                    // Question label row
                    col.Item()
                        .Background(hasAnswer ? Colors.White : "#fffbeb")
                        .Padding(12)
                        .Row(row =>
                        {
                            // Number badge
                            row.AutoItem()
                                .Width(24)
                                .Height(24)
                                .Background(Primary)
                                .AlignCenter()
                                .AlignMiddle()
                                .Text(number.ToString())
                                .FontSize(9)
                                .Bold()
                                .FontColor(Colors.White);

                            row.AutoItem().Width(10);

                            // Label
                            row.RelativeItem()
                                .AlignMiddle()
                                .Text(response.Label)
                                .FontSize(10)
                                .Bold()
                                .FontColor(TextPrimary);

                            // Field type chip
                            row.AutoItem()
                                .AlignMiddle()
                                .Background(PrimaryLight)
                                .PaddingVertical(4)
                                .PaddingHorizontal(6)
                                .Text(FormatFieldType(response.FieldType))
                                .FontSize(8)
                                .FontColor(Primary);
                        });

                    // Answer row
                    col.Item()
                        .BorderTop(1).BorderColor(BorderGray)
                        .Background(hasAnswer ? AnswerBg : "#fffbeb")
                        .PaddingVertical(12)
                        .PaddingHorizontal(10)
                        .Row(row =>
                        {
                            row.AutoItem()
                                .Width(34);   // indent to align with label

                            if (response.FieldType == "Checkbox"
                                && response.ResponseValues?.Any() == true)
                            {
                                // Render checkbox values as bullet list
                                row.RelativeItem()
                                    .Column(checkCol =>
                                    {
                                        foreach (var val in response.ResponseValues!)
                                        {
                                            checkCol.Item()
                                                .Row(r =>
                                                {
                                                    r.AutoItem()
                                                        .Width(12)
                                                        .Text("✓")
                                                        .FontSize(9)
                                                        .FontColor(Primary);
                                                    r.RelativeItem()
                                                        .Text(val)
                                                        .FontSize(10)
                                                        .FontColor(TextPrimary);
                                                });
                                        }
                                    });
                            }
                            else
                            {
                                row.RelativeItem()
                                    .Text(answer)
                                    .FontSize(10)
                                    .FontColor(hasAnswer ? TextPrimary : TextMuted)
                                    .Italic(!hasAnswer);
                            }
                        });
                });
        }

        // ── Footer ─────────────────────────────────────────────────────────────
        private void ComposeFooter(IContainer container)
        {
            container
                .BorderTop(1).BorderColor(BorderGray)
                .PaddingTop(8)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text(text =>
                        {
                            text.Span("MediScope — ")
                                .FontSize(8).FontColor(TextMuted);
                            text.Span("Questionnaire Submission")
                                .FontSize(8).FontColor(TextMuted);
                        });

                    row.AutoItem()
                        .Text(text =>
                        {
                            text.Span("Page ")
                                .FontSize(8).FontColor(TextMuted);
                            text.CurrentPageNumber()
                                .FontSize(8).FontColor(TextMuted);
                            text.Span(" of ")
                                .FontSize(8).FontColor(TextMuted);
                            text.TotalPages()
                                .FontSize(8).FontColor(TextMuted);
                        });

                    row.RelativeItem()
                        .AlignRight()
                        .Text($"Generated: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC")
                        .FontSize(8)
                        .FontColor(TextMuted);
                });
        }

        // ── Helpers ────────────────────────────────────────────────────────────
        private static void MetaField(IContainer container, string label, string value)
        {
            container.Column(col =>
            {
                col.Item()
                    .Text(label)
                    .FontSize(8)
                    .FontColor("#6b7280");
                col.Item()
                    .PaddingTop(2)
                    .Text(value)
                    .FontSize(10)
                    .Bold();
            });
        }

        private static string GetAnswerText(SubmissionResponseItemDto r)
        {
            if (r.FieldType == "Checkbox")
            {
                return r.ResponseValues?.Any() == true
                    ? string.Join(", ", r.ResponseValues)
                    : "—";
            }
            return string.IsNullOrWhiteSpace(r.ResponseValue) ? "—" : r.ResponseValue;
        }

        private static string FormatFieldType(string ft) => ft switch
        {
            "TextBox" => "Text",
            "TextArea" => "Text Area",
            "Number" => "Number",
            "Date" => "Date",
            "Dropdown" => "Dropdown",
            "RadioButton" => "Radio",
            "Checkbox" => "Checkbox",
            _ => ft
        };
    }
}