using NUnit.Framework;
using UnityEngine;

namespace OpenTTD.Tests.PlayMode
{
    public sealed class PlayModeSmokeTests
    {
        [Test]
        public void GameObject_WithMonoBehaviour_CanBeCreated()
        {
            var gameObject = new GameObject("PlayModeSmokeObject");
            try
            {
                var behaviour = gameObject.AddComponent<SmokeBehaviour>();
                Assert.That(behaviour, Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        private sealed class SmokeBehaviour : MonoBehaviour
        {
        }
    }
}
