using System;

namespace SpendWise.Models;

// Extended inheritance
public class Subscription : Transaction
{
    public int BillingCycleMonths { get; set; } // 1 = Monthly, 12 = Yearly

    public override string GetSummary()
    {
        string cycle = BillingCycleMonths == 1 ? "Monthly" : "Yearly";
        return base.GetSummary() + $" (Recurring: {cycle})";
    }
}
