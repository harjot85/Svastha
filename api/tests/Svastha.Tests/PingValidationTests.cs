using Svastha.Api;

namespace Svastha.Tests;

public class PingValidationTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Rejects_whitespace_message(string message)
    {
        Assert.False(PingValidation.IsValid(new CreatePingRequest(message)));
    }

    [Fact]
    public void Accepts_non_whitespace_message()
    {
        Assert.True(PingValidation.IsValid(new CreatePingRequest("hello")));
    }
}
