using NUnit.Framework;

namespace OpenTTD.Tests.EditMode
{
    public sealed class EditModeSmokeTests
    {
        [Test]
        public void Arithmetic_SanityCheck_Passes()
        {
            Assert.That(2 + 2, Is.EqualTo(4));
        }
    }
}
