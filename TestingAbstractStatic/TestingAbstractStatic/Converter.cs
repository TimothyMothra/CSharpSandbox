namespace TestingAbstractStatic
{
    /// <summary>
    /// <see href="https://devblogs.microsoft.com/dotnet/welcome-to-csharp-11/#abstracting-over-static-members"/>
    /// </summary>
    public class Converter
    {
        public interface IConverter<T> where T : class
        {
            public static abstract OutputType Convert(T input);
        }

        public class TypeOneConverter : IConverter<InputTypeOne>
        {
            public static OutputType Convert(InputTypeOne input) => new OutputType{ Value = input.One.ToString() };
        }

        public class TypeTwoConverter : IConverter<InputTypeTwo>
        {
            public static OutputType Convert(InputTypeTwo input) => new OutputType { Value = input.Two.ToString() };
        }
    }
}
