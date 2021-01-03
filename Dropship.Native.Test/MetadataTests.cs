using System.Reflection;
using Xunit;

namespace Dropship.Native.Test
{
    public class MetadataTests
    {
        [Fact]
        public void Test()
        {
            var metadata = ModMetadataParser.Parse(Assembly.GetExecutingAssembly().GetManifestResourceStream("Dropship.Native.Test.Example.dll")!);

            Assert.Equal("Example", metadata.assemblyName);
            Assert.Equal("gg.reactor.Example", metadata.id);
            Assert.Equal("Example", metadata.name);
            Assert.Equal("1.0.0", metadata.version);
            Assert.Equal("Mod template for Reactor", metadata.description);
            Assert.Equal("js6pak", metadata.authors);
        }
    }
}
