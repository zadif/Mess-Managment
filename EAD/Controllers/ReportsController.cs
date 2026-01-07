using EAD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EAD.Controllers
{
    public class ReportsController : Controller
    {
        private readonly EadProjectContext _context;

        public ReportsController(EadProjectContext context)
        {
            _context = context;
        }

        // ============================================
        // ADMIN REPORTS
        // ============================================

        [Authorize(AuthenticationSchemes = "JwtAuth", Roles = "Admin")]
        public async Task<IActionResult> AdminSummaryReport()
        {
            // Gather data for the report
            var users = await _context.Users.ToListAsync();
            var bills = await _context.Bills.Include(b => b.User).ToListAsync();
            var mealItems = await _context.MealItems.ToListAsync();
            var dailyConsumptions = await _context.DailyConsumptions
                .Include(dc => dc.MealItem)
                .Include(dc => dc.User)
                .ToListAsync();

            // Generate PDF
            var pdfBytes = GenerateAdminReport(users, bills, mealItems, dailyConsumptions);

            var fileName = $"KskMess_Admin_Report_{DateTime.Now:yyyy-MM-dd_HH-mm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [Authorize(AuthenticationSchemes = "JwtAuth", Roles = "Admin")]
        public async Task<IActionResult> AdminBillsReport()
        {
            try
            {
                var bills = await _context.Bills
                    .Include(b => b.User)
                    .OrderByDescending(b => b.GeneratedOn)
                    .ToListAsync();

                var pdfBytes = GenerateBillsReport(bills, isAdmin: true);

                var fileName = $"KskMess_Bills_Report_{DateTime.Now:yyyy-MM-dd_HH-mm}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception)
            {
                TempData["Error"] = "Failed to generate report. Please try again.";
                return RedirectToAction("AdminHome", "Dashboard");
            }
        }

        // ============================================
        // USER REPORTS
        // ============================================

        [Authorize(AuthenticationSchemes = "JwtAuth", Roles = "User")]
        public async Task<IActionResult> UserBillsReport()
        {
            string idStr = Request.Cookies["UserId"];
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out int userId))
            {
                return RedirectToAction("LoginPage", "Login");
            }

            var user = await _context.Users.FindAsync(userId);
            var bills = await _context.Bills
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.GeneratedOn)
                .ToListAsync();

            var consumptions = await _context.DailyConsumptions
                .Include(dc => dc.MealItem)
                .Where(dc => dc.UserId == userId)
                .OrderByDescending(dc => dc.ConsumptionDate)
                .ToListAsync();

            var pdfBytes = GenerateUserReport(user!, bills, consumptions);

            var fileName = $"KskMess_My_Report_{DateTime.Now:yyyy-MM-dd_HH-mm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // ============================================
        // PDF GENERATION METHODS
        // ============================================

        private byte[] GenerateAdminReport(List<User> users, List<Bill> bills, List<MealItem> mealItems, List<DailyConsumption> consumptions)
        {
            var totalUsers = users.Count;
            var activeUsers = users.Count(u => u.IsActive);
            var totalBills = bills.Count;
            var paidBills = bills.Count(b => b.IsPaid);
            var unpaidBills = bills.Count(b => !b.IsPaid);
            var totalRevenue = bills.Where(b => b.IsPaid).Sum(b => b.TotalAmount);
            var pendingAmount = bills.Where(b => !b.IsPaid).Sum(b => b.TotalAmount);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Spacing(20);

                        // Summary Section
                        col.Item().Element(c => ComposeSummarySection(c, totalUsers, activeUsers, totalBills, paidBills, unpaidBills, totalRevenue, pendingAmount));

                        // Recent Bills Table
                        col.Item().Element(c => ComposeBillsTable(c, bills.Take(15).ToList()));

                        // Meal Items Summary
                        col.Item().Element(c => ComposeMealItemsSummary(c, mealItems));
                    });

                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateBillsReport(List<Bill> bills, bool isAdmin)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Text("All Bills Report").FontSize(16).Bold().FontColor(Colors.Grey.Darken3);
                        col.Item().Element(c => ComposeFullBillsTable(c, bills, isAdmin));
                    });

                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateUserReport(User user, List<Bill> bills, List<DailyConsumption> consumptions)
        {
            var totalBilled = bills.Sum(b => b.TotalAmount);
            var totalPaid = bills.Where(b => b.IsPaid).Sum(b => b.TotalAmount);
            var outstanding = bills.Where(b => !b.IsPaid).Sum(b => b.TotalAmount);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        col.Spacing(20);

                        // User Info
                        col.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(userCol =>
                        {
                            userCol.Spacing(5);
                            userCol.Item().Text("Account Information").FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                            userCol.Item().Text($"Name: {user.Name}");
                            userCol.Item().Text($"Email: {user.Email}");
                            userCol.Item().Text($"Member Since: {user.CreatedOn?.ToString("MMMM dd, yyyy") ?? "N/A"}");
                        });

                        // Financial Summary
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                            {
                                c.Item().Text("Total Billed").FontSize(10).FontColor(Colors.Grey.Darken2);
                                c.Item().Text($"Rs {totalBilled:N2}").FontSize(16).Bold().FontColor(Colors.Green.Darken3);
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                            {
                                c.Item().Text("Total Paid").FontSize(10).FontColor(Colors.Grey.Darken2);
                                c.Item().Text($"Rs {totalPaid:N2}").FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(c =>
                            {
                                c.Item().Text("Outstanding").FontSize(10).FontColor(Colors.Grey.Darken2);
                                c.Item().Text($"Rs {outstanding:N2}").FontSize(16).Bold().FontColor(Colors.Orange.Darken3);
                            });
                        });

                        // Bills Table
                        col.Item().Text("Your Bills").FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                        col.Item().Element(c => ComposeUserBillsTable(c, bills));

                        // Recent Consumptions
                        if (consumptions.Any())
                        {
                            col.Item().Text("Recent Consumption History").FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                            col.Item().Element(c => ComposeConsumptionsTable(c, consumptions.Take(20).ToList()));
                        }
                    });

                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        // ============================================
        // COMPONENT BUILDERS
        // ============================================

        private void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(10).BorderBottom(1).BorderColor(Colors.Green.Darken2).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Ksk Mess").FontSize(24).Bold().FontColor(Colors.Green.Darken2);
                    col.Item().Text("Mess Management System").FontSize(10).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(120).Column(col =>
                {
                    col.Item().AlignRight().Text($"Generated: {DateTime.Now:MMM dd, yyyy}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignRight().Text($"Time: {DateTime.Now:hh:mm tt}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text("© Ksk Mess - Confidential Report").FontSize(8).FontColor(Colors.Grey.Medium);
                row.RelativeItem().AlignRight().Text(x =>
                {
                    x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private void ComposeSummarySection(IContainer container, int totalUsers, int activeUsers, int totalBills, int paidBills, int unpaidBills, decimal totalRevenue, decimal pendingAmount)
        {
            container.Column(col =>
            {
                col.Item().Text("Summary Overview").FontSize(16).Bold().FontColor(Colors.Grey.Darken3);
                col.Item().PaddingTop(10).Row(row =>
                {
                    // Users Card
                    row.RelativeItem().Background(Colors.Blue.Lighten4).Padding(10).Column(c =>
                    {
                        c.Item().Text("Total Users").FontSize(10).FontColor(Colors.Grey.Darken2);
                        c.Item().Text($"{totalUsers}").FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                        c.Item().Text($"{activeUsers} active").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(10);

                    // Bills Card
                    row.RelativeItem().Background(Colors.Orange.Lighten4).Padding(10).Column(c =>
                    {
                        c.Item().Text("Total Bills").FontSize(10).FontColor(Colors.Grey.Darken2);
                        c.Item().Text($"{totalBills}").FontSize(20).Bold().FontColor(Colors.Orange.Darken3);
                        c.Item().Text($"{paidBills} paid / {unpaidBills} pending").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(10);

                    // Revenue Card
                    row.RelativeItem().Background(Colors.Green.Lighten4).Padding(10).Column(c =>
                    {
                        c.Item().Text("Total Revenue").FontSize(10).FontColor(Colors.Grey.Darken2);
                        c.Item().Text($"Rs {totalRevenue:N0}").FontSize(20).Bold().FontColor(Colors.Green.Darken3);
                        c.Item().Text($"Rs {pendingAmount:N0} pending").FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }

        private void ComposeBillsTable(IContainer container, List<Bill> bills)
        {
            container.Column(col =>
            {
                col.Item().Text("Recent Bills").FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                col.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("#").FontColor(Colors.White).FontSize(9);
                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("User").FontColor(Colors.White).FontSize(9);
                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Amount").FontColor(Colors.White).FontSize(9);
                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Date").FontColor(Colors.White).FontSize(9);
                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Status").FontColor(Colors.White).FontSize(9);
                    });

                    // Rows
                    foreach (var bill in bills)
                    {
                        var bgColor = bill.IsPaid ? Colors.Green.Lighten5 : Colors.Orange.Lighten5;
                        table.Cell().Background(bgColor).Padding(5).Text($"{bill.Id}").FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).Text(bill.User?.Name ?? "N/A").FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).Text($"Rs {bill.TotalAmount:N2}").FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).Text(bill.GeneratedOn.ToString("MMM dd, yyyy")).FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).Text(bill.IsPaid ? "Paid" : "Pending").FontSize(9).FontColor(bill.IsPaid ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                    }
                });
            });
        }

        private void ComposeMealItemsSummary(IContainer container, List<MealItem> mealItems)
        {
            container.Column(col =>
            {
                col.Item().Text("Meal Items Catalog").FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                col.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Item Name").FontColor(Colors.White).FontSize(9);
                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Category").FontColor(Colors.White).FontSize(9);
                        header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Price").FontColor(Colors.White).FontSize(9);
                    });

                    var isAlt = false;
                    foreach (var item in mealItems)
                    {
                        var bg = isAlt ? Colors.Grey.Lighten4 : Colors.White;
                        table.Cell().Background(bg).Padding(5).Text(item.Name ?? "N/A").FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(item.Category ?? "N/A").FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text($"Rs {item.Price:N2}").FontSize(9);
                        isAlt = !isAlt;
                    }
                });
            });
        }

        private void ComposeFullBillsTable(IContainer container, List<Bill> bills, bool isAdmin)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    if (isAdmin) columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("#").FontColor(Colors.White).FontSize(9);
                    if (isAdmin) header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("User").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Amount").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Generated").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Paid On").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Status").FontColor(Colors.White).FontSize(9);
                });

                var isAlt = false;
                foreach (var bill in bills)
                {
                    var bg = isAlt ? Colors.Grey.Lighten4 : Colors.White;
                    table.Cell().Background(bg).Padding(5).Text($"{bill.Id}").FontSize(9);
                    if (isAdmin) table.Cell().Background(bg).Padding(5).Text(bill.User?.Name ?? "N/A").FontSize(9);
                    table.Cell().Background(bg).Padding(5).Text($"Rs {bill.TotalAmount:N2}").FontSize(9);
                    table.Cell().Background(bg).Padding(5).Text(bill.GeneratedOn.ToString("MMM dd")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).Text(bill.PaidOn?.ToString("MMM dd") ?? "-").FontSize(9);
                    table.Cell().Background(bg).Padding(5).Text(bill.IsPaid ? "Paid" : "Pending").FontSize(9).FontColor(bill.IsPaid ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                    isAlt = !isAlt;
                }
            });
        }

        private void ComposeUserBillsTable(IContainer container, List<Bill> bills)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("#").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Amount").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Generated").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Paid On").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Status").FontColor(Colors.White).FontSize(9);
                });

                if (!bills.Any())
                {
                    table.Cell().ColumnSpan(5).Padding(15).AlignCenter().Text("No bills found").FontSize(10).FontColor(Colors.Grey.Medium);
                }
                else
                {
                    var isAlt = false;
                    foreach (var bill in bills)
                    {
                        var bg = isAlt ? Colors.Grey.Lighten4 : Colors.White;
                        table.Cell().Background(bg).Padding(5).Text($"{bill.Id}").FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text($"Rs {bill.TotalAmount:N2}").FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(bill.GeneratedOn.ToString("MMM dd, yyyy")).FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(bill.PaidOn?.ToString("MMM dd, yyyy") ?? "-").FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(bill.IsPaid ? "Paid" : "Pending").FontSize(9).FontColor(bill.IsPaid ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                        isAlt = !isAlt;
                    }
                }
            });
        }

        private void ComposeConsumptionsTable(IContainer container, List<DailyConsumption> consumptions)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Date").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Item").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Qty").FontColor(Colors.White).FontSize(9);
                    header.Cell().Background(Colors.Grey.Darken3).Padding(5).Text("Status").FontColor(Colors.White).FontSize(9);
                });

                var isAlt = false;
                foreach (var c in consumptions)
                {
                    var bg = isAlt ? Colors.Grey.Lighten4 : Colors.White;
                    table.Cell().Background(bg).Padding(5).Text(c.ConsumptionDate.ToString("MMM dd")).FontSize(9);
                    table.Cell().Background(bg).Padding(5).Text(c.MealItem?.Name ?? "N/A").FontSize(9);
                    table.Cell().Background(bg).Padding(5).Text($"{c.Quantity}").FontSize(9);
                    table.Cell().Background(bg).Padding(5).Text(c.IsBilled ? "Billed" : "Unbilled").FontSize(9).FontColor(c.IsBilled ? Colors.Green.Darken2 : Colors.Grey.Medium);
                    isAlt = !isAlt;
                }
            });
        }
    }
}
