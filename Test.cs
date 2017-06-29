using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.TypeConverter;

namespace TinyCsvBug
{
    [TestFixture]
    class Test
    {
        [Test]
        public void Bug()
        {
            var csvParserOptions = new CsvParserOptions(false, new []{','});
            var csvMapper = new MyMapping();
            var csvParser = new CsvParser<Model>(csvParserOptions, csvMapper);
            var csv = "http://www.google.com";
            
            var result = csvParser
                .ReadFromString(new CsvReaderOptions(new[] { Environment.NewLine }), csv)
                .ToList();

            Assert.AreEqual("http://www.google.com", result[0].Result.Uri.ToString());
        }
    }

    internal class MyMapping : CsvMapping<Model>
    {
        public MyMapping()
        {
            // EXCEPTION HERE
            // TinyCsvParser.Exceptions.CsvTypeConverterNotRegisteredException : No TypeConverter registered for Type System.Uri
            MapProperty(0, x => x.Uri)
                .WithCustomConverter(new HttpUriConverter());
        }
    }
    
    internal class HttpUriConverter : ITypeConverter<Uri>
    {
        private static readonly Regex MatchHttp = new Regex(@"^\s*https?://");

        public bool TryConvert(string value, out Uri result)
        {
            result = null;
            if (!MatchHttp.IsMatch(value))
            {
                return false;
            }
            result = new Uri(value);
            return true;
        }

        public Type TargetType => typeof(Uri);
    }

    class Model
    {
        public Uri Uri { get; set; }
    }
}
