using System;
using SpendWise.Models.Enums;

namespace SpendWise.Models;

public abstract class Transaction
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Category TransactionCategory { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;

    public virtual string GetSummary()
    {
        return $"{Date:yyyy-MM-dd} | {Name.PadRight(15)} | {Amount,10:C} | [{TransactionCategory}]";
    }
}
