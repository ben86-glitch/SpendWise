using SpendWise.Interfaces;
using SpendWise.Models;
using SpendWise.Models.Enums;
using SpendWise.Services;
using Microsoft.Extensions.ObjectPool;
using SpendWise.Exceptions;

namespace SpendWise.UI;

public class ConsoleUserInterface
{
    bool isRunning = true;
    private readonly ITransactionService _service;

    public ConsoleUserInterface(ITransactionService service)
    {
        _service = service;
    }

    public void Run()
    {
        do
        {
            Console.Clear();
            Console.WriteLine("=== SpedWise: Personal Finance Tracker ===");
            Console.WriteLine("1. Add One-Time Expense");
            Console.WriteLine("2. Add Subscription");
            Console.WriteLine("3. View All Transactions");
            Console.WriteLine("4. View Total Spending");
            Console.WriteLine("5. Delete");
            Console.WriteLine("6. Category Reports");
            Console.WriteLine("7. Global Category Summary");
            Console.WriteLine("8. Financial Dashboard");
            Console.WriteLine("9. Exit");

            string choice = Console.ReadLine() ?? "";
            try
            {
                switch (choice)
                {
                    case "1":
                        Console.Clear();
                        Console.WriteLine("--- Add New Expense ---");

                        Console.Write("Enter Item Name: ");
                        string name = Console.ReadLine() ?? "Unknown";

                        Console.Write("Enter Amount (₱): ");
                        string amountInput = Console.ReadLine() ?? "0";

                        // Safety Check: Try to turn the string into a decimal
                        if (decimal.TryParse(amountInput, out decimal amount))
                        {
                            Category selectedCat = PickCategory();
                            var newExpense = new OneTimeExpense
                            {
                                Name = name,
                                Amount = amount,
                                TransactionCategory = selectedCat 
                            };

                            _service.AddTransaction(newExpense);
                            Console.WriteLine("\nSaved!");
                        }
                        else
                        {
                            Console.WriteLine("\nError: Invalid amount. Please enter a number.");
                        }
                        break;
                    case "2":
                        Console.Clear();
                        Console.WriteLine("---Add New Subscription");

                        Console.Write("Enter Service Name (e.g. Netflix):");
                        string subName = Console.ReadLine() ?? "Unknown";

                        Console.Write("Enter Monthly Cost (₱): ");
                        if (decimal.TryParse(Console.ReadLine(), out decimal subAmount))
                        {
                            Console.Write("Billing Cycle (in months, e.g., 1 or 12): ");
                            if (!int.TryParse(Console.ReadLine(), out int months)) months = 1;

                            var newSub = new Subscription
                            {
                                Name = subName,
                                Amount = subAmount,
                                TransactionCategory = Category.Subscription,
                                BillingCycleMonths = 1
                            };

                            _service.AddTransaction(newSub);
                            Console.WriteLine("\nSubscription Saved!");
                        }
                        break;
                    case "3":
                        ShowAllTransactions();
                        break;
                    case "4":
                        Console.WriteLine($"\nTotal Monthly Spending: {_service.GetTotalExpenses():C}");
                        break;
                    case "5":
                        Console.Clear();
                        Console.WriteLine("--- Delete a Transaction ---");
                        Console.Write("Enter the name of the item to remove: ");
                        string nameToDelete = Console.ReadLine() ?? "";

                        // Confirmation first before deleting
                        Console.Write($"\nAre you sure you want to delete '{nameToDelete}'? (y/n): ");
                        string confirm = Console.ReadLine()?.ToLower() ?? "";

                        if (confirm == "y" || confirm == "yes")
                        {
                            try
                            {
                                // Let the service handle the search AND the delete in one go
                                bool wasDeleted = _service.DeleteTransactionByName(nameToDelete);

                                if (wasDeleted)
                                {
                                    Console.WriteLine($"\n[SUCCESS]: '{nameToDelete}' has been removed.");
                                }
                                else
                                {
                                    // This handles the "Item not found" case without a separate search
                                    Console.WriteLine("\n[NOT FOUND]: No transaction matches that name.");
                                }
                            }
                            catch (Exception ex)
                            {
                                // This handles actual crashes (e.g. NullReference, FileErrors)
                                Console.WriteLine($"\n[CRITICAL ERROR]: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nDelete cancelled. Returning to menu...");
                        }
                        break;
                    case "6":
                        Console.Clear();
                        Console.WriteLine("--- Category Report ---");
                        Console.WriteLine("0: Food, 1: Transport, 2: Tech, 3: Utilities, 4: Entertainment, 5: Subscription");
                        Console.Write("Enter Category Number: ");

                        if (int.TryParse(Console.ReadLine(), out int catIndex) && Enum.IsDefined(typeof(Category), catIndex))
                        {
                            Category selectedCat = (Category)catIndex;
                            var total = _service.GetTotalByCategory(selectedCat);

                            Console.WriteLine($"\nTotal spent on {selectedCat}: {total:C}");
                        }
                        else
                        {
                            Console.WriteLine("Invalid category selection.");
                        }
                        break;
                    case "7":
                        Console.Clear();
                        Console.WriteLine("=== SpendWise Financial Report ===");
                        Console.WriteLine("----------------------------------");

                        var report = _service.GetFullCategoryReport();

                        foreach (var entry in report)
                        {
                            //Only shows categories that have spending
                            if (entry.Value > 0)
                            {
                                Console.WriteLine($"{entry.Key,-15}: {entry.Value,10:C}");
                            }
                        }

                        Console.WriteLine("----------------------------------");
                        Console.WriteLine($"GRAND TOTAL     : {_service.GetTotalExpenses(),10:C}");
                        break;
                    case "8":
                        Console.Clear();
                        Console.WriteLine("=== SpendWise Financial Dashboard ===");
                        Console.WriteLine("-------------------------------------");

                        decimal thisMonth = _service.GetCurrentMonthTotal();
                        decimal prediction = _service.PredictNext30Days();

                        Console.WriteLine($"Actual Spending (This Month): {thisMonth:C}");
                        Console.WriteLine($"Fixed Commitments (Next 30 Days): {prediction:C}");
                        Console.WriteLine("------------------------------------");

                        if (prediction > 5000)
                        {
                            Console.WriteLine("Warning: Your subscription 'burn rate' is high!");
                        }
                        else
                        {
                            Console.WriteLine("Your fixed costs are looking healthy.");
                        }
                        break;
                    case "9":
                        _service.SaveToFile();
                        isRunning = false;
                        Console.Clear();
                        Console.WriteLine("Thank you for using SpendWise!");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Try Again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[SYSTEM ERROR]: Something went wrong. {ex.Message}");
                Console.WriteLine("Press any key ro try again...");
                Console.ReadKey();
            }

            if (isRunning)
            {
                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey();
            }
        } while (isRunning);

    }

    private void ShowAllTransactions()
    {
        var list = _service.GetAllTransactions();
        if (list.Count == 0)
        {
            Console.WriteLine("No transaction yet!");
            return;
        }

        Console.WriteLine("\n--- Transaction History ---");
        foreach (var item in list)
        {
            Console.WriteLine(item.GetSummary());
        }
    }

    private Category PickCategory()
    {
        Console.WriteLine("\nSelect Category:");
        Console.WriteLine("0: Food | 1: Transport | 2: Tech | 3: Utilities | 4: Entertainment");
        Console.Write("Choice: ");

        if (int.TryParse(Console.ReadLine(), out int index) && Enum.IsDefined(typeof(Category), index))
        {
            return (Category)index;
        }
        return Category.Food; // Default fallback
    }
}