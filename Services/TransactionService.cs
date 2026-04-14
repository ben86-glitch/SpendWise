using System;
using Microsoft.AspNetCore.SignalR;
using SpendWise.Services;
using SpendWise.Interfaces;
using SpendWise.Models;
using SpendWise.Models.Enums;
using SpendWise.Exceptions;
using System.IO;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace SpendWise.Services;

public class TransactionService : ITransactionService
{
    private readonly List<Transaction> _transactions = new();

    public void AddTransaction(Transaction transaction)
    {
        if (string.IsNullOrWhiteSpace(transaction.Name))
        {
            throw new InsufficientDataException("Transaction name cannot be empty!");
        }

        if (transaction.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }

        _transactions.Add(transaction);
        Console.WriteLine("Transaction added successfully");
    }

    // Get everything
    public List<Transaction> GetAllTransactions()
    {
        return _transactions;
    }

    public List<Transaction> GetByCategory(Category category)
    {
        return _transactions.Where(t => t.TransactionCategory == category).ToList();
    }

    // Calculate total burn rate
    public decimal GetTotalExpenses()
    {
        return _transactions.Sum(t => t.Amount);
    }

    public bool DeleteTransactionByName(string name)
    {
        var itemToRemove = _transactions.FirstOrDefault(t
        => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (itemToRemove != null)
        {
            _transactions.Remove(itemToRemove);
            return true; // Success!
        }
        return false; // Not found
    }

    public decimal GetTotalByCategory(Category category)
    {
        return _transactions
            .Where(t => t.TransactionCategory == category)
            .Sum(t => t.Amount);
    }

    public Dictionary<Category, decimal> GetFullCategoryReport()
    {
        var categories = Enum.GetValues(typeof(Category)).Cast<Category>();

        var report = new Dictionary<Category, decimal>();

        foreach (var cat in categories)
        {
            decimal total = _transactions
                .Where(t => t.TransactionCategory == cat)
                .Sum(t => t.Amount);

            report.Add(cat, total);
        }
        return report;
    }

    // Method for saving and loading transactions to a file (for persistence)
    private readonly string _filePath = "transactions.csv";

    public void SaveToFile()
    {
        // Convert transactions to CSV format: Name, Amount, Category
        var lines = _transactions.Select(t =>
            $"{t.GetType().Name}, {t.Name}, {t.Amount}, {t.TransactionCategory}");

        File.WriteAllLines(_filePath, lines);
    }

    public void LoadFromFile()
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        try
        {
            _transactions.Clear(); //Clear current memory to avoid duplicates
            var lines = File.ReadAllLines(_filePath);

            foreach (var line in lines)
            {
                try
                {
                    var parts = line.Split(',');
                    if (parts.Length < 4) continue;

                    string type = parts[0];
                    string name = parts[1];
                    decimal amount = decimal.Parse(parts[2]);
                    Category category = Enum.Parse<Category>(parts[3]);

                    if (type == nameof(Subscription))
                    {
                        _transactions.Add(new Subscription
                        {
                            Name = name,
                            Amount = amount,
                            TransactionCategory = category,
                            BillingCycle = (SpendWise.Models.Enums.BillingCycle)1
                        });
                    }
                    else
                    {
                        _transactions.Add(new OneTimeExpense
                        {
                            Name = name,
                            Amount = amount,
                            TransactionCategory = category
                        });
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[Warning] Skipping corrupted data line: {line}");
                    continue;
                }
            }
        }
        catch (IOException ex)
        {
            throw new Exception($"Cannot access storage file: {ex.Message}");
        }
    }
    // Total spending in this month
    public decimal GetCurrentMonthTotal()
    {
        var now = DateTime.Now;
        return _transactions
            .Where(t => t.Date.Month == now.Month && t.Date.Year == now.Year)
            .Sum(t => t.Amount);
    }

    public decimal PredictNext30Days()
    {
        var monthlySubTotal = _transactions
            .OfType<Subscription>()
            .Sum(s => s.Amount);

        return monthlySubTotal;
    }
}


