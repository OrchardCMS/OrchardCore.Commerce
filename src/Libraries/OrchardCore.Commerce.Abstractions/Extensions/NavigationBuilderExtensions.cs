using Microsoft.Extensions.Localization;
using System;
using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Navigation;

public static class NavigationBuilderExtensions
{
    /// <summary>
    /// Applies <paramref name="callback"/> inside the Admin > Settings > Commerce menu.
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1313:Parameter names should begin with lower-case letter",
        Justification = "String localizer convention.")]
    public static NavigationBuilder AddCommerce(
        this NavigationBuilder builder,
        IStringLocalizer T,
        Action<NavigationBuilder> callback) =>
        builder
            .Add(T["Settings"], settings => settings
                .Add(T["Commerce"], commerce => callback(commerce.Id("settings-commerce"))));
}
