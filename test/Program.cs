using Xunit;

namespace Yanp.Test;

public class Program
{
    private static void RunSource(string path)
    {
        Yanp.Program.Run(new StreamReader(path));
    }

    [Fact]
    public void All()
    {
        //Directory.GetFiles("..\\..\\..\\..\\test-case").AsParallel().Each(x => RunSource(x));
    }
}
