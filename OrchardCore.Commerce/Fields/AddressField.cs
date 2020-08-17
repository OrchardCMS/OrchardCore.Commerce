using InternationalAddress;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Fields
{
    public class AddressField : ContentField
    {
        public Address Address { get; set; }
    }
}
