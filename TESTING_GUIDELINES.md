# Testing Guidelines

## Folder Conventions
- Place all tests under `Assets/Tests`.
- Place EditMode tests under `Assets/Tests/EditMode`.
- Place PlayMode tests under `Assets/Tests/PlayMode`.
- Do not place tests under runtime feature folders in `Assets/Scripts`.

## Naming Conventions
- Test assembly names:
  - `Tests.EditMode`
  - `Tests.PlayMode`
- Test file names should end with `Tests.cs`.
- Test classes should use `PascalCase` and end with `Tests`.
- Test method names should follow `Action_Condition_ExpectedResult`.

## EditMode vs PlayMode Decision Rules
Use **EditMode** tests when:
- Testing pure C# logic.
- Testing deterministic utilities and data transforms.
- No scene loading or frame progression is required.

Use **PlayMode** tests when:
- Testing `MonoBehaviour` lifecycle behavior.
- Testing scene interactions, frame timing, or coroutine flow.
- Testing runtime object creation and Unity engine integration.

## Assembly Dependency Rules
- `Tests.EditMode`:
  - Editor-only platform.
  - Allowed references: `UnityEngine.TestRunner`, `UnityEditor.TestRunner`.
- `Tests.PlayMode`:
  - Any Platform.
  - Allowed references: `UnityEngine.TestRunner`.
  - Must never reference `UnityEditor.TestRunner`.
- Do not manually reference `nunit.framework.dll`.
- Test asmdefs must set `autoReferenced` to `false`.
- Runtime asmdefs must not reference `Tests.EditMode` or `Tests.PlayMode`.

## Templates
### EditMode Template
```csharp
using NUnit.Framework;

namespace OpenTTD.Tests.EditMode
{
    public sealed class SampleEditModeTests
    {
        [Test]
        public void Action_Condition_ExpectedResult()
        {
            Assert.That(true, Is.True);
        }
    }
}
```

### PlayMode Template
```csharp
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace OpenTTD.Tests.PlayMode
{
    public sealed class SamplePlayModeTests
    {
        [UnityTest]
        public IEnumerator Action_Condition_ExpectedResult()
        {
            var gameObject = new GameObject("Sample");
            try
            {
                Assert.That(gameObject, Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }

            yield return null;
        }
    }
}
```

## Optional Local Hook (PowerShell)
If your team uses local hooks, add a pre-commit check that blocks staged test files outside `Assets/Tests`.

## CI Test Execution
- Unity tests run in GitHub Actions via `.github/workflows/unity-tests.yml`.
- EditMode and PlayMode run in a matrix job and publish per-mode artifacts.
- Configure repository secret `UNITY_LICENSE` to enable Unity test execution in CI.
- Internal runs fail fast when `UNITY_LICENSE` is missing.
- Fork pull requests without secrets skip Unity tests by policy.
- CI must fail on any Unity test failure.
