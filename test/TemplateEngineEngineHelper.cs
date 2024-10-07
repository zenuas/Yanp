using Xunit;
using Yanp.TemplateEngine;

namespace Yanp.Test;

public class TemplateEngineEngineHelper
{
    [Fact]
    public void TrimHeaderNull1()
    {
        var source = EngineHelper.TrimHeader("");
        Assert.Equal(source, "");
    }

    [Fact]
    public void TrimHeaderNull2()
    {
        var source = EngineHelper.TrimHeader("\r\n");
        Assert.Equal(source, "\r\n");
    }

    [Fact]
    public void TrimHeaderNoHeader1()
    {
        var source = EngineHelper.TrimHeader("a\r\nb");
        Assert.Equal(source, "a\r\nb");
    }

    [Fact]
    public void TrimHeaderNoHeader2()
    {
        var source = EngineHelper.TrimHeader("a\r\nb\r\n");
        Assert.Equal(source, "a\r\nb\r\n");
    }

    [Fact]
    public void TrimHeaderWithHeader1()
    {
        var source = EngineHelper.TrimHeader("//#Assembly.Load x\r\na\r\nb");
        Assert.Equal(source, "a\r\nb");
    }

    [Fact]
    public void TrimHeaderWithHeader2()
    {
        var source = EngineHelper.TrimHeader("//#Assembly.Load x\r\n//#Assembly.Load y\r\na\r\nb\r\n");
        Assert.Equal(source, "a\r\nb\r\n");
    }

    [Fact]
    public void TrimHeaderWithHeader3()
    {
        var source = EngineHelper.TrimHeader("//#Assembly.Load x\r\n//#Assembly.Load y\r\n\r\na\r\nb\r\n");
        Assert.Equal(source, "\r\na\r\nb\r\n");
    }

    [Fact]
    public void TrimHeaderWithHeader4()
    {
        var source = EngineHelper.TrimHeader("//#Assembly.Load x\r\n\r\n//#Assembly.Load y\r\na\r\nb\r\n");
        Assert.Equal(source, "\r\n//#Assembly.Load y\r\na\r\nb\r\n");
    }
}
