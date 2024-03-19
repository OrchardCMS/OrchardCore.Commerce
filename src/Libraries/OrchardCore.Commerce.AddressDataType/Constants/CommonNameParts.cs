namespace OrchardCore.Commerce.AddressDataType.Constants;

/// <summary>
/// Some name parts common across many cultures. None of these are guaranteed to be in every name, their order and
/// applicability can be dependant on culture or local laws.
/// </summary>
/// <remarks>
/// <para>Prefer using "given name" instead of "first name" or "forename", and prefer "family name" instead of "last
/// name" or "surname" because these imply a naming order that can cause confusion in Chinese, Korean, Hungarian, etc
/// users. While this still carries cultural assumptions (e.g. some cultures use some variant of the father's given name
/// instead of a family name), it's less technically intrusive.</para>
/// </remarks>
public static class CommonNameParts
{
    /// <summary>
    /// A form of address, such as formal or academic titles (e.g. "Dr") or the typical gendered honorifics (e.g. "Mr",
    /// "Mrs", "Miss"). Often these prefix the name, but not always. Suffixes include "Esq" in English and most Japanese
    /// honorifics.
    /// </summary>
    public const string Honorific = nameof(Honorific);

    /// <summary>
    /// Given names are used to primarily identify the individual. In all European cultures except Hungarian this
    /// corresponds to the first name (so it's also called the "western" name order), but this correlation should be
    /// handled on the front-end in a culture-aware manner.
    /// </summary>
    public const string GivenName = nameof(GivenName);

    /// <summary>
    /// Refers to a second given name. This is very common in English, especially in the USA. This name part is most
    /// often called a "middle name", but in cultures with different name orders this may be the last name. In some
    /// cultures this may be a patronymic or matronymic, either as-is or in a derived form.
    /// </summary>
    public const string MiddleName = nameof(MiddleName);

    /// <summary>
    /// The hereditary portion of one's name that indicates family connection.
    /// </summary>
    public const string FamilyName = nameof(FamilyName);
}
