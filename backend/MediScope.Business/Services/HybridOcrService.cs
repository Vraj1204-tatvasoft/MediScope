using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tesseract;
using UglyToad.PdfPig;
using Docnet.Core;
using Docnet.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using MediScope.Business.Services.Interfaces;

namespace MediScope.Business.Services
{
    public class OcrPageResult
    {
        public int Page { get; set; }
        public string Content { get; set; } = string.Empty;
        public float Confidence { get; set; }
    }

    public class HybridOcrService : IOcrService
    {
        private readonly ILogger<HybridOcrService> _logger;
        private readonly string _tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

        public HybridOcrService(ILogger<HybridOcrService> logger)
        {
            _logger = logger;
        }

        public string ExtractTextFromFile(byte[] fileBytes, string fileExtension)
        {
            try
            {
                string ext = fileExtension.ToLower();

                if (ext == ".pdf")
                {
                    return ProcessPdfWithFallback(fileBytes);
                }
                else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp")
                {
                    return ProcessImageWithTesseract(fileBytes);
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error during text extraction process.");
                return "[]";
            }
        }

        private string ProcessPdfWithFallback(byte[] pdfBytes)
        {
            var pages = new List<OcrPageResult>();

            try
            {
                using var document = PdfDocument.Open(pdfBytes);
                int pageNumber = 1;

                foreach (var page in document.GetPages())
                {
                    pages.Add(new OcrPageResult
                    {
                        Page = pageNumber++,
                        Content = page.Text.Trim(),
                        Confidence = 100.0f
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PdfPig failed to open document natively.");
            }

            int totalChars = pages.Sum(p => p.Content.Length);

            if (pages.Count == 0 || totalChars < 50)
            {
                _logger.LogInformation("PDF appears to be a scanned image. Triggering Visual OCR...");
                return ProcessScannedPdfWithOcr(pdfBytes);
            }

            return JsonSerializer.Serialize(pages, new JsonSerializerOptions { WriteIndented = true });
        }

        private string ProcessScannedPdfWithOcr(byte[] pdfBytes)
        {
            var pages = new List<OcrPageResult>();
            using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);

            using var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(2048, 2048));
            int pageCount = docReader.GetPageCount();

            for (int i = 0; i < pageCount; i++)
            {
                using var pageReader = docReader.GetPageReader(i);

                byte[] rawBytes = pageReader.GetImage();
                int width = pageReader.GetPageWidth();
                int height = pageReader.GetPageHeight();

                using var image = Image.LoadPixelData<Bgra32>(rawBytes, width, height);
                using var memoryStream = new MemoryStream();
                image.SaveAsPng(memoryStream);
                byte[] pngBytes = memoryStream.ToArray();

                using var pix = Pix.LoadFromMemory(pngBytes);
                using var tesseractPage = engine.Process(pix);

                float rawConfidence = tesseractPage.GetMeanConfidence();
                float percentageConfidence = (float)Math.Round(rawConfidence * 100, 2);

                pages.Add(new OcrPageResult
                {
                    Page = i + 1,
                    Content = tesseractPage.GetText().Trim(),
                    Confidence = percentageConfidence
                });
            }

            return JsonSerializer.Serialize(pages, new JsonSerializerOptions { WriteIndented = true });
        }

        private string ProcessImageWithTesseract(byte[] imageBytes)
        {
            var pages = new List<OcrPageResult>();

            using var standardEngine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
            using var standardPix = Pix.LoadFromMemory(imageBytes);
            using var standardPage = standardEngine.Process(standardPix);

            float standardConfidence = standardPage.GetMeanConfidence();
            string standardText = standardPage.GetText().Trim();

            if (!string.IsNullOrWhiteSpace(standardText) && standardText.Length > 20)
            {
                pages.Add(new OcrPageResult
                {
                    Page = 1,
                    Content = standardText,
                    Confidence = (float)Math.Round(standardConfidence * 100, 2)
                });
                return JsonSerializer.Serialize(pages, new JsonSerializerOptions { WriteIndented = true });
            }

            _logger.LogInformation("Standard OCR found nothing. Attempting aggressive handwriting pre-processing...");

            using var image = Image.Load(imageBytes);

            image.Mutate(x => x
                .Grayscale()
                .Contrast(1.5f)
                .BinaryThreshold(0.5f)
            );

            using var cleanedStream = new MemoryStream();
            image.SaveAsPng(cleanedStream);
            byte[] processedBytes = cleanedStream.ToArray();

            using var lstmEngine = new TesseractEngine(_tessDataPath, "eng", EngineMode.LstmOnly);
            lstmEngine.DefaultPageSegMode = PageSegMode.SparseText;

            using var processedPix = Pix.LoadFromMemory(processedBytes);
            using var processedPage = lstmEngine.Process(processedPix);

            float processedConfidence = processedPage.GetMeanConfidence();

            pages.Add(new OcrPageResult
            {
                Page = 1,
                Content = processedPage.GetText().Trim(),
                Confidence = (float)Math.Round(processedConfidence * 100, 2)
            });

            return JsonSerializer.Serialize(pages, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}