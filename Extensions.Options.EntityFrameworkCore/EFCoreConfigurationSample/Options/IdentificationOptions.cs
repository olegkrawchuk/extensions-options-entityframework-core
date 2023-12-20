using System.ComponentModel.DataAnnotations;

namespace EFCoreConfigurationSample.Options;

public class IdentificationOptions
{
    public const string ConfigName = "Identification";

    [Required]
    public required DeepLinkOptions DeepLink { get; init; }
}
