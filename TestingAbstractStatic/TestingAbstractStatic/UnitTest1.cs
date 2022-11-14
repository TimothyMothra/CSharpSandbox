namespace TestingAbstractStatic
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test1()
        {
            var input = new InputTypeOne
            {
                One = 111
            };

            var exporter = new TypeOneExporter();

            var output = exporter.Export(input);

            Assert.AreEqual("111", output.Value);
        }


        [TestMethod]
        public void Test2()
        {
            var input = new InputTypeTwo
            {
                Two = 222
            };

            var exporter = new TypeTwoExporter();

            var output = exporter.Export(input);

            Assert.AreEqual("222", output.Value);
        }
    }
}