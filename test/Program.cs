using Extensions;
using Xunit;

public class Program
{
    private static void RunSource(string path)
    {
        Yanp.Program.Run(new StreamReader(path));
    }

    [Fact]
    public void All()
    {
        Directory.GetFiles("..\\..\\..\\..\\test-case").AsParallel().Each(x => RunSource(x));
    }
}
