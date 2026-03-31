using SpendWise.Services;
using SpendWise.Interfaces;
using SpendWise.UI;

/*
PROJECT: SpendWise
--------------------------------------------------------------------------
DESCRIPTION:
    A lightweight Personal Finance & Subscription Tracker designed to help users 
    monitor their "burn rate" and manage financial health. SpendWise categorizes 
    expenses into one-time purchases and recurring subscriptions (Netflix, Gym, etc.), 
    providing a clear view of total monthly commitments.
KEY FEATURES:
    - Automated Monthly Cost Calculation for Subscriptions.
    - Category-based Spending Reports (Food, Tech, Bills).
    - Console-based Dashboard with formatted PHP currency (en-PH).
Data Persistence via File I/O (to be implemented in Week 2).
--------------------------------------------------------------------------
*/

ITransactionService service = new TransactionService();
service.LoadFromFile();

var ui = new ConsoleUserInterface(service);
ui.Run(); // Start the app

