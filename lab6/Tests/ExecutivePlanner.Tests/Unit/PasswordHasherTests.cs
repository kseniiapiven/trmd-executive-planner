using ExecutivePlanner.Helpers;

namespace ExecutivePlanner.Tests.Unit;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_DoesNotContainPlainPassword()
    {
        var hash = PasswordHasher.Hash("secret123");

        Assert.DoesNotContain("secret123", hash);
    }

    [Fact]
    public void Hash_SamePasswordTwice_GivesDifferentHashesBecauseOfSalt()
    {
        Assert.NotEqual(PasswordHasher.Hash("secret123"), PasswordHasher.Hash("secret123"));
    }

    [Theory]
    [InlineData("secret123", "secret123", true)]
    [InlineData("secret123", "Secret123", false)]
    [InlineData("secret123", "", false)]
    [InlineData("secret123", "secret1234", false)]
    public void Verify_ChecksPasswordAgainstStoredHash(string original, string attempt, bool expected)
    {
        var stored = PasswordHasher.Hash(original);

        Assert.Equal(expected, PasswordHasher.Verify(attempt, stored));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("1.@@@.@@@")]
    public void Verify_WithMalformedStoredValue_ReturnsFalse(string stored)
    {
        Assert.False(PasswordHasher.Verify("secret123", stored));
    }
}
