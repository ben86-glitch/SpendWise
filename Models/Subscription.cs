using System;
using SpendWise.Models.Enums;

namespace SpendWise.Models;

// Extended inheritance
public class Subscription : Transaction
{
    public override string GetSummary()
    {
        string cycleLabel = BillingCycle == SpendWise.Models.Enums.BillingCycle.Yearly ? "Yearly" : "Monthly";
        return base.GetSummary() + $" (Recurring: {cycleLabel})";
    }
}
