using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using wada.Models;

namespace wada.Services
{
    public static class InvoicePdfService
    {
        /// <summary>Generates an invoice PDF and saves it to <paramref name="savePath"/>.</summary>
        public static void Generate(
            FreelancerProfile    profile,
            ProjectModel         project,
            List<ClientModel>    clients,
            List<MilestoneModel> milestones,
            string               savePath)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            double total         = milestones.Sum(m => m.Price);
            string invoiceNumber = $"INV-{project.Id:D4}-{DateTime.Now:yyyyMMdd}";

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(t => t.FontSize(11).FontFamily("Arial"));

                    // ── HEADER ────────────────────────────────────────────────
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            if (!string.IsNullOrWhiteSpace(profile.BusinessName))
                                col.Item().Text(profile.BusinessName).Bold().FontSize(18);
                            if (!string.IsNullOrWhiteSpace(profile.Name))
                                col.Item().Text(profile.Name).FontSize(12);
                            if (!string.IsNullOrWhiteSpace(profile.Email))
                                col.Item().Text(profile.Email);
                            if (!string.IsNullOrWhiteSpace(profile.Phone))
                                col.Item().Text(profile.Phone);
                            if (!string.IsNullOrWhiteSpace(profile.Address))
                                col.Item().Text(profile.Address);
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("INVOICE").Bold().FontSize(26);
                            col.Item().Text($"# {invoiceNumber}").FontSize(11);
                            col.Item().Text($"Date: {DateTime.Today:MMMM d, yyyy}");
                        });
                    });

                    // ── BODY ──────────────────────────────────────────────────
                    page.Content().PaddingTop(20).Column(body =>
                    {
                        body.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        body.Item().PaddingTop(16);

                        body.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("BILL TO").Bold().FontSize(9)
                                   .FontColor(Colors.Grey.Medium);
                                col.Item().PaddingTop(4);
                                if (clients.Any())
                                {
                                    foreach (var c in clients)
                                    {
                                        col.Item().Text(c.Name).Bold();
                                        if (!string.IsNullOrWhiteSpace(c.Email))
                                            col.Item().Text(c.Email);
                                        if (!string.IsNullOrWhiteSpace(c.MobileNumber))
                                            col.Item().Text(c.MobileNumber);
                                        col.Item().PaddingTop(4);
                                    }
                                }
                                else
                                {
                                    col.Item().Text("—").FontColor(Colors.Grey.Medium);
                                }
                            });

                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Text("PROJECT").Bold().FontSize(9)
                                   .FontColor(Colors.Grey.Medium);
                                col.Item().PaddingTop(4);
                                col.Item().Text(project.Name).Bold();
                                col.Item().Text($"Status: {project.Status}");
                                col.Item().Text($"Deadline: {project.EndDateTime:MMM d, yyyy}");
                            });
                        });

                        body.Item().PaddingTop(24);

                        body.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Black).Padding(8)
                                      .Text("Milestone / Description").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Black).Padding(8)
                                      .AlignCenter().Text("Deadline").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Black).Padding(8)
                                      .AlignRight().Text("Amount").Bold().FontColor(Colors.White);
                            });

                            bool shade = false;
                            foreach (var m in milestones)
                            {
                                var bg = shade ? Colors.Grey.Lighten4 : Colors.White;
                                shade = !shade;

                                table.Cell().Background(bg).Padding(8).Text(m.Description);
                                table.Cell().Background(bg).Padding(8).AlignCenter()
                                     .Text(m.Deadline == DateTime.MinValue ? "—" : m.Deadline.ToString("MMM d, yyyy"));
                                table.Cell().Background(bg).Padding(8).AlignRight()
                                     .Text($"${m.Price:N2}");
                            }

                            if (!milestones.Any())
                            {
                                table.Cell().ColumnSpan(3).Padding(12).AlignCenter()
                                     .Text("No milestones defined.").FontColor(Colors.Grey.Medium);
                            }
                        });

                        body.Item().PaddingTop(8).AlignRight().Row(row =>
                        {
                            row.AutoItem().Background(Colors.Black).Padding(10)
                               .Text($"TOTAL   ${total:N2}").Bold().FontSize(13).FontColor(Colors.White);
                        });

                        body.Item().PaddingTop(32);
                        body.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        body.Item().PaddingTop(10).Text("Thank you for your business.")
                            .FontColor(Colors.Grey.Medium).FontSize(10);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ").FontSize(9).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                        x.Span(" of ").FontSize(9).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });
            })
            .GeneratePdf(savePath);
        }
    }
}
