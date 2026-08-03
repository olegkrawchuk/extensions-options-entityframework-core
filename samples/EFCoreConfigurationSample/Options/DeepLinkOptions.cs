using System.ComponentModel.DataAnnotations;

namespace EFCoreConfigurationSample.Options;

public class DeepLinkOptions
{
    [Required, Url]
    public required string Prefix { get; init; }


    [Required, Range(0, int.MaxValue)]
    public required int Ttl { get; init; }
}
