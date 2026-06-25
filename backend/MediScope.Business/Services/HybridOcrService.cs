using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tesseract;
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
                    _logger.LogInformation("Routing PDF directly to Visual OCR for maximum data capture safety.");
                    return ProcessScannedPdfWithOcr(fileBytes);
                }
                else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp")
                {
                    return ProcessImageWithTesseract(fileBytes);
                }
                else
                {
                    return "[]";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error during text extraction process.");
                return "[]";
            }
        }

        private string ProcessScannedPdfWithOcr(byte[] pdfBytes)
        {
            using var docReader = DocLib.Instance.GetDocReader(pdfBytes, new PageDimensions(2048, 2048));
            int pageCount = docReader.GetPageCount();

            // Fast Sequential Image Extraction (Protects Docnet from native memory crashes)
            var pageImages = new List<(int Index, byte[] PngBytes)>();

            for (int i = 0; i < pageCount; i++)
            {
                using var pageReader = docReader.GetPageReader(i);
                byte[] rawBytes = pageReader.GetImage();
                int width = pageReader.GetPageWidth();
                int height = pageReader.GetPageHeight();

                using var image = Image.LoadPixelData<Bgra32>(rawBytes, width, height);
                using var memoryStream = new MemoryStream();
                image.SaveAsPng(memoryStream);

                pageImages.Add((i, memoryStream.ToArray()));
            }

            // Lists are NOT thread-safe for adding items concurrently. Arrays are, as long as threads write to different indexes.
            var pages = new OcrPageResult[pageCount];

            // Heavy Parallel OCR Processing (Uses 100% of available CPU cores)
            Parallel.ForEach(pageImages, pageData =>
            {
                // SAFETY: Every thread MUST create its own isolated TesseractEngine.
                using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);

                using var pix = Pix.LoadFromMemory(pageData.PngBytes);
                using var tesseractPage = engine.Process(pix);

                float rawConfidence = tesseractPage.GetMeanConfidence();
                float percentageConfidence = (float)Math.Round(rawConfidence * 100, 2);

                // Thread safely writes only to its specifically assigned index
                pages[pageData.Index] = new OcrPageResult
                {
                    Page = pageData.Index + 1,
                    Content = tesseractPage.GetText().Trim(),
                    Confidence = percentageConfidence
                };
            });

            // Convert the array back to a List for the JSON Serializer
            return JsonSerializer.Serialize(pages.ToList(), new JsonSerializerOptions { WriteIndented = true });
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