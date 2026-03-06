using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace aesth_clic.Utils
{
    public static class ReceiptService
    {

        static ReceiptService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // Colors
        private const string BrandBlue = "#0078D4";
        private const string BrandGreen = "#0EA47A";
        private const string BrandAmber = "#C07800";
        private const string BrandRed = "#D83B01";
        private const string BrandPurple = "#C239B3";
        private const string LightGray = "#F3F3F3";
        private const string MidGray = "#E0E0E0";
        private const string TextPrimary = "#1A1A1A";
        private const string TextSecondary = "#666666";
        private const string HeaderBlue = "#CCE4F7";

        private static string OutputDir =>
            Path.Combine(Path.GetTempPath(), "aesth_clic", "receipts");

        public static Task GenerateAndOpenAsync(ReceiptData data)
        {
            return Task.Run(() =>
            {
                Directory.CreateDirectory(OutputDir);

                string safeName = data.ReceiptNumber.Replace("/", "-").Replace("\\", "-");
                string outputPath = Path.Combine(OutputDir, $"{safeName}.pdf");

                var (statusColor, statusLabel) = GetStatusStyle(data.Status);
                string tierColor = GetTierColor(data.Tier);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(0);

                        page.DefaultTextStyle(x =>
                            x.FontFamily("Segoe UI")
                             .FontSize(10)
                             .FontColor(TextPrimary));

                        page.Content().Column(col =>
                        {
                            // HEADER
                            col.Item()
                               .Background(BrandBlue)
                               .PaddingHorizontal(20)
                               .PaddingVertical(16)
                               .Row(row =>
                               {
                                   row.RelativeItem().Column(left =>
                                   {
                                       left.Item().Row(r =>
                                       {
                                           r.AutoItem().Text("aesth")
                                               .Bold().FontSize(20).FontColor(Colors.White);

                                           r.AutoItem().Text("_clic")
                                               .FontSize(20).FontColor(Colors.White);
                                       });

                                       left.Item()
                                           .PaddingTop(2)
                                           .Text("Aesthetic Clinic ERP · Payment Receipt")
                                           .FontSize(8)
                                           .FontColor(HeaderBlue);
                                   });

                                   row.AutoItem().AlignRight().Column(right =>
                                   {
                                       right.Item()
                                           .Text("OFFICIAL RECEIPT")
                                           .Bold().FontSize(11).FontColor(Colors.White);

                                       right.Item()
                                           .PaddingTop(2)
                                           .Text($"# {data.ReceiptNumber}")
                                           .FontSize(9)
                                           .FontColor(HeaderBlue);
                                   });
                               });

                            // META
                            col.Item()
                               .PaddingHorizontal(20)
                               .PaddingTop(16)
                               .Row(row =>
                               {
                                   row.RelativeItem().Column(left =>
                                   {
                                       left.Item().Text(txt =>
                                       {
                                           txt.Span("Payment Date: ").FontColor(TextSecondary);
                                           txt.Span(data.PaymentDate).Bold();
                                       });

                                       left.Item().PaddingTop(5).Text(txt =>
                                       {
                                           txt.Span("Issued By: ").FontColor(TextSecondary);
                                           txt.Span(data.IssuedBy).Bold();
                                       });
                                   });

                                   row.AutoItem()
                                      .AlignRight()
                                      .AlignMiddle()
                                      .Border(1)
                                      .BorderColor(statusColor)
                                      .CornerRadius(12)
                                      .PaddingHorizontal(14)
                                      .PaddingVertical(5)
                                      .Text(statusLabel)
                                      .Bold()
                                      .FontSize(9)
                                      .FontColor(statusColor);
                               });

                            // DIVIDER
                            col.Item()
                               .PaddingHorizontal(20)
                               .PaddingVertical(14)
                               .LineHorizontal(0.5f)
                               .LineColor(MidGray);

                            // BILLED TO
                            col.Item()
                               .PaddingHorizontal(20)
                               .PaddingBottom(16)
                               .Column(billed =>
                               {
                                   billed.Item()
                                         .Text("BILLED TO")
                                         .FontSize(8)
                                         .Bold()
                                         .FontColor(TextSecondary);

                                   billed.Item().PaddingTop(5)
                                         .Text(data.ClientName)
                                         .Bold()
                                         .FontSize(14);

                                   billed.Item().PaddingTop(3)
                                         .Text(data.ClinicName)
                                         .FontSize(10)
                                         .FontColor(TextSecondary);

                                   billed.Item().PaddingTop(2)
                                         .Text(data.Email)
                                         .FontSize(10)
                                         .FontColor(TextSecondary);
                               });

                            // PAYMENT CARD
                            col.Item()
                               .PaddingHorizontal(20)
                               .PaddingBottom(16)
                               .Background(LightGray)
                               .Border(0.5f)
                               .BorderColor(MidGray)
                               .CornerRadius(6)
                               .Padding(16)
                               .Row(row =>
                               {
                                   void AddCell(string label, string value, string color, float size)
                                   {
                                       row.RelativeItem().Column(cell =>
                                       {
                                           cell.Item()
                                               .Text(label)
                                               .FontSize(7)
                                               .Bold()
                                               .FontColor(TextSecondary);

                                           cell.Item()
                                               .PaddingTop(6)
                                               .Text(value)
                                               .Bold()
                                               .FontSize(size)
                                               .FontColor(color);
                                       });
                                   }

                                   AddCell("MODULE TIER", data.Tier, tierColor, 13);
                                   AddCell("AMOUNT PAID", data.Amount, BrandBlue, 16);
                                   AddCell("PAYMENT DATE", data.PaymentDate, TextPrimary, 12);
                                   AddCell("NEXT DUE DATE", data.NextDueDate, TextPrimary, 12);
                               });

                            // DIVIDER
                            col.Item()
                               .PaddingHorizontal(20)
                               .PaddingBottom(14)
                               .LineHorizontal(0.5f)
                               .LineColor(MidGray);

                            // SUMMARY
                            col.Item()
                               .PaddingHorizontal(20)
                               .PaddingBottom(16)
                               .Column(tbl =>
                               {
                                   tbl.Item()
                                      .PaddingBottom(8)
                                      .Text("SUBSCRIPTION SUMMARY")
                                      .FontSize(8)
                                      .Bold()
                                      .FontColor(TextSecondary);

                                   var rows = new[]
                                   {
                                       ("Clinic",data.ClinicName,TextPrimary),
                                       ("Module Tier",data.Tier,tierColor),
                                       ("Monthly Amount",data.Amount,BrandBlue),
                                       ("Payment Status",data.Status,statusColor),
                                       ("Next Due Date",data.NextDueDate,TextPrimary)
                                   };

                                   bool shade = false;

                                   foreach (var (key, val, color) in rows)
                                   {
                                       tbl.Item()
                                          .Background(shade ? LightGray : Colors.White)
                                          .PaddingHorizontal(8)
                                          .PaddingVertical(7)
                                          .Row(r =>
                                          {
                                              r.RelativeItem()
                                               .Text(key)
                                               .FontSize(9)
                                               .FontColor(TextSecondary);

                                              r.AutoItem()
                                               .Text(val)
                                               .Bold()
                                               .FontSize(9)
                                               .FontColor(color);
                                          });

                                       shade = !shade;
                                   }
                               });

                            // THANK YOU
                            col.Item()
                               .PaddingHorizontal(20)
                               .Background("#EBF4FB")
                               .Border(0.5f)
                               .BorderColor("#C7E0F4")
                               .CornerRadius(6)
                               .Padding(14)
                               .Column(box =>
                               {
                                   box.Item()
                                      .AlignCenter()
                                      .Text("Thank you for your subscription!")
                                      .Bold()
                                      .FontSize(11)
                                      .FontColor(BrandBlue);

                                   box.Item()
                                      .PaddingTop(4)
                                      .AlignCenter()
                                      .Text("For questions, contact your system administrator.")
                                      .FontSize(8)
                                      .FontColor(TextSecondary);
                               });
                        });

                        // FOOTER
                        page.Footer()
                            .BorderTop(0.5f)
                            .BorderColor(MidGray)
                            .PaddingHorizontal(20)
                            .PaddingVertical(8)
                            .Row(row =>
                            {
                                row.RelativeItem()
                                   .Text($"Generated on {DateTime.Now:MMMM dd, yyyy 'at' hh:mm tt} · aesth_clic ERP · {data.ReceiptNumber}")
                                   .FontSize(7)
                                   .FontColor(TextSecondary);

                                row.AutoItem()
                                   .AlignRight()
                                   .Text("This is a system-generated receipt.")
                                   .FontSize(7)
                                   .FontColor(TextSecondary);
                            });
                    });
                });

                document.GeneratePdf(outputPath);
                OpenFile(outputPath);
            });
        }

        private static (string Color, string Label) GetStatusStyle(string status) =>
            status switch
            {
                "Paid" => (BrandGreen, "PAID"),
                "Overdue" => (BrandRed, "OVERDUE"),
                "Due Soon" => (BrandAmber, "DUE SOON"),
                _ => (TextSecondary, "UNPAID")
            };

        private static string GetTierColor(string tier) =>
            tier.ToLower() switch
            {
                "basic" => BrandBlue,
                "standard" => BrandPurple,
                "premium" => BrandAmber,
                _ => TextPrimary
            };

        private static void OpenFile(string path)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }

        public static string GenerateReceiptNumber()
        {
            int year = DateTime.Now.Year;
            int serial = new Random(Environment.TickCount).Next(10000, 99999);
            return $"RCP-{year}-{serial}";
        }
    }

    public class ReceiptData
    {
        public string ReceiptNumber { get; set; } = "";
        public string PaymentDate { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string Email { get; set; } = "";
        public string ClinicName { get; set; } = "";
        public string Tier { get; set; } = "";
        public string Amount { get; set; } = "";
        public string NextDueDate { get; set; } = "";
        public string IssuedBy { get; set; } = "SuperAdmin";
        public string Status { get; set; } = "";
    }
}