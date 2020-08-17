using System.Collections.Generic;
using Money;

namespace OrchardCore.Commerce.Abstractions
{
    public interface IPaymentProvider
    {
        public IPayment CreateCharge(string kind, string transactionId, Amount amount, IDictionary<string, string> data);

        public void AddData(IPayment charge, IDictionary<string, string> data);
    }
}
