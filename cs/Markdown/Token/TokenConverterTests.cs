﻿using System.Linq;
using NUnit.Framework;
using FluentAssertions;

namespace Markdown
{
    [TestFixture]
    public class TokenConverterTests
    {
        private TokenConverter converter;

        [SetUp]
        public void SetUp()
        {
            converter = new TokenConverter();
        }
        
        
        [TestCase("_ab c_", 1)]
        [TestCase("_a_ _a_", 2)]
        [TestCase("_a_bcd", 1)]
        [TestCase("a_bc_d", 1)]
        [TestCase("abc_d_", 1)]
        public void FindTokens_ShouldFindAllItalicsTag_ItalicsMarkup(string source, int count)
        {
            converter.FindTokens(source)
                .GetTokens()
                .Select(t => t.Tag)
                .Should().AllBeOfType<ItalicsTag>()
                .And.HaveCount(count);
        }

        [TestCase("__")]
        [TestCase("abc_12_2")]
        [TestCase(" _abc_123 ")]
        [TestCase("a_b a_c")]
        [TestCase("_a b_c")]
        [TestCase("a_a b_")]
        [TestCase("_ a_")]
        [TestCase("_a _")]
        public void FindTokens_ShouldNotFindItalicsTag_IncorrectItalicsMarkup(string source)
        {
            converter.FindTokens(source)
                .GetTokens()
                .Should()
                .BeEmpty();
        }
    }
}