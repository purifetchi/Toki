using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Toki.ActivityPub.Converters.Ulid;

public class UlidToStringConverter : ValueConverter<System.Ulid, string>
{
    private static readonly ConverterMappingHints DefaultHints = new ConverterMappingHints(size: 26);

    public UlidToStringConverter() 
        : this(null!)
    {
    }

    public UlidToStringConverter(ConverterMappingHints mappingHints = null!)
        : base(
            convertToProviderExpression: x => x.ToString(),
            convertFromProviderExpression: x => System.Ulid.Parse(x),
            mappingHints: DefaultHints.With(mappingHints))
    {
    }
}