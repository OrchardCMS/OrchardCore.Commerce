namespace OrchardCore.Commerce.ViewModels
{
    public class TextProductAttributeSettingsViewModel
    {
        public string Hint { get; set; }
        public string DefaultValue { get; set; }
        public bool Required { get; set; }
        public string Placeholder { get; set; }
        public string PredefinedValues { get; set; }
        public bool RestrictToPredefinedValues { get; set; }
        public bool MultipleValues { get; set; }
    }
}
