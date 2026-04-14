using Microsoft.AspNetCore.Mvc;
using SpendWise.Data;
using SpendWise.Models;
using X.PagedList.Extensions;
using SpendWise.Models.Enums;

namespace SpendWise.Controllers;

public class HomeController : Controller
{
    private readonly SpendWiseDbContext _context;

    // This is Dependency Injection: The Web App gives the DB automatically
    public HomeController(SpendWiseDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(int? page, string searchString, DateTime? startDate, DateTime? endDate)
    {
        int pageSize = 10;
        int pageNumber = page ?? 1;
        var now = DateTime.Now;

        // 1. Start with the base query
        var transactionsQuery = _context.Transactions.AsQueryable();

        // 2. Apply Smart Search Filter
        if (!string.IsNullOrEmpty(searchString))
        {
            string query = searchString.ToLower().Trim();

            transactionsQuery = transactionsQuery.Where(t =>
                t.Name.ToLower().Contains(query) ||
                t.TransactionCategory.ToString().ToLower().Contains(query) ||
                t.PaymentMethod.ToString().ToLower().Contains(query) ||
                (t.Date.Month == 1 && "january".Contains(query)) ||
                (t.Date.Month == 2 && "february".Contains(query)) ||
                (t.Date.Month == 3 && "march".Contains(query)) ||
                (t.Date.Month == 4 && "april".Contains(query)) ||
                (t.Date.Month == 5 && "may".Contains(query)) ||
                (t.Date.Month == 6 && "june".Contains(query)) ||
                (t.Date.Month == 7 && "july".Contains(query)) ||
                (t.Date.Month == 8 && "august".Contains(query)) ||
                (t.Date.Month == 9 && "september".Contains(query)) ||
                (t.Date.Month == 10 && "october".Contains(query)) ||
                (t.Date.Month == 11 && "november".Contains(query)) ||
                (t.Date.Month == 12 && "december".Contains(query))
            );

            ViewData["CurrentFilter"] = searchString;
        }

        // 3. Apply Date Range Logic
        if (startDate.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Date >= startDate.Value);
            ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
        }
        if (endDate.HasValue)
        {
            transactionsQuery = transactionsQuery.Where(t => t.Date <= endDate.Value);
            ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
        }

        // 4. Finalize Query: Ordering
        var allTransactions = transactionsQuery.OrderByDescending(t => t.Date);

        // 5. Calculate "Total in View" based on the filtered query
        ViewBag.TotalInView = allTransactions.Sum(t => (decimal?)t.Amount) ?? 0;

        // 6. Create the PagedList for display
        var pagedList = allTransactions.ToPagedList(pageNumber, pageSize);

        // --- Static Dashboard Calculations (Always based on full DB) ---
        ViewBag.TotalMonth = _context.Transactions
            .Where(t => t.Date.Month == now.Month && t.Date.Year == now.Year)
            .Sum(t => (decimal?)t.Amount) ?? 0;

        ViewBag.TotalYear = _context.Transactions
            .Where(t => t.Date.Year == now.Year)
            .Sum(t => (decimal?)t.Amount) ?? 0;

        var totalAllTime = _context.Transactions.Sum(t => (decimal?)t.Amount) ?? 0;
        ViewBag.TotalAllTime = totalAllTime;

        ViewBag.FixedCosts = _context.Transactions
            .Where(t => t is Subscription)
            .Sum(t => t.BillingCycle == SpendWise.Models.Enums.BillingCycle.Yearly
                    ? t.Amount / 12
                    : t.Amount);

        // Category Data for Chart/UI
        var categoryData = _context.Transactions
            .GroupBy(t => t.TransactionCategory)
            .Select(g => new
            {
                Category = g.Key.ToString(),
                Total = g.Sum(x => x.Amount),
                Percentage = totalAllTime > 0 ? (g.Sum(x => x.Amount) / totalAllTime) * 100 : 0
            })
            .OrderByDescending(g => g.Total)
            .ToList();

        ViewBag.CategoryData = categoryData;

        return View(pagedList);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(TransactionViewModel vm)
    {
        if (ModelState.IsValid)
        {
            Transaction finalRecord;

            if (vm.TransactionType == "Subscription")
            {
                finalRecord = new Subscription
                {
                    Name = vm.Name,
                    Amount = vm.Amount,
                    TransactionCategory = vm.TransactionCategory,
                    BillingCycle = vm.BillingCycle,
                    Date = DateTime.Now
                };
            }
            else
            {
                finalRecord = new OneTimeExpense
                {
                    Name = vm.Name,
                    Amount = vm.Amount,
                    TransactionCategory = vm.TransactionCategory,
                    Date = DateTime.Now
                };
            }

            _context.Transactions.Add(finalRecord);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(vm);
    }

    // 1. GET: Show the confirmation page
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var transaction = _context.Transactions.FirstOrDefault(t => t.Id == id);
        if (transaction == null)
        {
            return NotFound();
        }
        return View(transaction);
    }

    // 2. POST: Remove the record from SQL
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var transaction = _context.Transactions.Find(id);
        if (transaction != null)
        {
            _context.Transactions.Remove(transaction);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    // GET: Show the Edit Form with existing data
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var transaction = _context.Transactions.Find(id);
        if (transaction == null) return NotFound();

        var vm = new TransactionViewModel
        {
            Name = transaction.Name,
            Amount = transaction.Amount,
            TransactionCategory = transaction.TransactionCategory,
            // Detect the type based on the actual C# class
            TransactionType = transaction is Subscription ? "Subscription" : "Expense",
            BillingCycle = (transaction as Subscription)?.BillingCycle
        };

        return View(vm);
    }

    // POST: Save the changes to SQL
    [HttpPost]
    public IActionResult Edit(int id, TransactionViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var existing = _context.Transactions.Find(id);
            if (existing == null) return NotFound();

            // Update the values
            existing.Name = vm.Name;
            existing.Amount = vm.Amount;
            existing.TransactionCategory = vm.TransactionCategory;
            existing.BillingCycle = vm.BillingCycle;

            _context.Update(existing);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(vm);
    }

    public IActionResult ExportToCSV(string searchString)
    {
        var transactionsQuery = _context.Transactions.AsQueryable();

        // 1. Apply the same filter logic as the Index
        if (!string.IsNullOrEmpty(searchString))
        {
            string query = searchString.ToLower().Trim();
            transactionsQuery = transactionsQuery.Where(t =>
                t.Name.ToLower().Contains(query) ||
                t.TransactionCategory.ToString().ToLower().Contains(query) ||
                t.PaymentMethod.ToString().ToLower().Contains(query) ||
                (t.Date.Month == 1 && "january".Contains(query)) ||
                (t.Date.Month == 2 && "february".Contains(query)) ||
                (t.Date.Month == 3 && "march".Contains(query)) ||
                (t.Date.Month == 4 && "april".Contains(query)) ||
                (t.Date.Month == 5 && "may".Contains(query)) ||
                (t.Date.Month == 6 && "june".Contains(query)) ||
                (t.Date.Month == 7 && "july".Contains(query)) ||
                (t.Date.Month == 8 && "august".Contains(query)) ||
                (t.Date.Month == 9 && "september".Contains(query)) ||
                (t.Date.Month == 10 && "october".Contains(query)) ||
                (t.Date.Month == 11 && "november".Contains(query)) ||
                (t.Date.Month == 12 && "december".Contains(query))
            );
        }

        var list = transactionsQuery.OrderByDescending(t => t.Date).ToList();

        // 2. Build the CSV String
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("Date,Name,Category,Amount,Payment Method,Type");

        foreach (var t in list)
        {
            string type = t is Subscription ? "Subscription" : "Expense";
            builder.AppendLine($"{t.Date:yyyy-MM-dd},{t.Name},{t.TransactionCategory},{t.Amount},{t.PaymentMethod},{type}");
        }

        // 3. Return as a file download
        var csvData = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
        return File(csvData, "text/csv", $"SpendWise_Export_{DateTime.Now:yyyyMMdd}.csv");
    }

    public IActionResult About()
    {
        return View();
    }
}