namespace TestingAbstractStatic
{
    /// <summary>
    /// <see href="https://devblogs.microsoft.com/dotnet/welcome-to-csharp-11/#abstracting-over-static-members"/>
    /// </summary>
    public class Exporter
    {
        public abstract class ExporterBase<T, TConverter> where T : class where TConverter : IConverter<T>
        {
            public OutputType Export(T input) => TConverter.Convert(input);
        }

        public class TypeOneExporter : ExporterBase<InputTypeOne, TypeOneConverter>
        {
        }

        public class TypeTwoExporter : ExporterBase<InputTypeTwo, TypeTwoConverter>
        {
        }
    }
}
