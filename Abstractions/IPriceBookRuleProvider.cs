using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Abstractions
{
    /// <summary>
    /// Price book rule providers return back the applicable price books for current context
    /// </summary>
    public interface IPriceBookRuleProvider
    {
        /// <summary>
        /// Content Type name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Retrieve all price book rules by this provider
        /// </summary>
        Task<IEnumerable<PriceBookRule>> GetPriceBookRules();
    }
}
