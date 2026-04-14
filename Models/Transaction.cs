using System;
using SpendWise.Models.Enums;
using System.ComponentModel.DataAnnotations; // Add this for the [Key] attribute

namespace SpendWise.Models;

public abstract class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public Category TransactionCategory { get; set; }
    
    public DateTime Date { get; set; } = DateTime.Now;

    public BillingCycle? BillingCycle { get; set; }
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    public virtual string GetSummary()
    {
        return $"{Date:yyyy-MM-dd} | {Name.PadRight(15)} | {Amount,10:C} | [{TransactionCategory}]";
    }
}