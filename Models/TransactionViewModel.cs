using System.ComponentModel.DataAnnotations;
using SpendWise.Models.Enums;

namespace SpendWise.Models;

public class TransactionViewModel
{
    [Required(ErrorMessage = "Please enter a name for the transaction.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Please select a category.")]
    public Enums.Category TransactionCategory { get; set; }

    public Enums.BillingCycle? BillingCycle { get; set; }

    [Required]
    public string TransactionType { get; set; } = "Expense";
    
    [Required(ErrorMessage = "Please select a payment method.")]
    public Enums.PaymentMethod PaymentMethod { get; set; }
}