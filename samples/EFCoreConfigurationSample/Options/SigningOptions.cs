using System.ComponentModel.DataAnnotations;

namespace EFCoreConfigurationSample.Options;

public class SigningOptions
{
    public const string ConfigName = "Signing";

    [Required]
    public required DeepLinkOptions DeepLink { get; init; }
}
