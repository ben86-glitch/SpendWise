using System;
using SpendWise.Models;
using SpendWise.Models.Enums;

namespace SpendWise.Interfaces;

public interface ITransactionService
{
    void AddTransaction(Transaction transaction);
    List<Transaction> GetAllTransactions();
    List<Transaction> GetByCategory(Category category);
    decimal GetTotalExpenses();
    bool DeleteTransactionByName(string name);
    decimal GetTotalByCategory(Category category);
    Dictionary<Category, decimal> GetFullCategoryReport();

    void SaveToFile();
    void LoadFromFile();
    decimal GetCurrentMonthTotal();
    decimal PredictNext30Days();
}   
