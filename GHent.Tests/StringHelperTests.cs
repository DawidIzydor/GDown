using System;
using Xunit;
using GHent.Shared;

namespace GHent.Models.Tests
{
    public class StringHelperTests
    {
        [Theory]
        [InlineData("Hello World!", "Hello_World_")]
        [InlineData("Filename:Test", "Filename_Test")]
        [InlineData("Invalid/Characters\\", "Invalid_Characters_")]
        [InlineData("Special@#%&*()Chars", "Special_______Chars")]
        [InlineData("1234567890", "1234567890")]
        [InlineData("NormalText", "NormalText")]
        [InlineData("", "")]
        [InlineData("   ", "___")]
        public void RemoveIllegalCharacters_ShouldReplaceIllegalCharactersWithUnderscore(string input, string expected)
        {
            // Act
            var result = input.RemoveIllegalCharacters();

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
