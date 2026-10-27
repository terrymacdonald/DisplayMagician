# DisplayMagician.Gateway.Tests

Paging regressions verify that mutations to original DTOs, nested collections, and returned pages cannot change an existing snapshot.

Run `dotnet test DisplayMagician.Gateway.Tests/DisplayMagician.Gateway.Tests.csproj -p:Platform=x64` from the repository root.

These isolated tests cover HTTP protocol/authentication middleware, idempotency, response limits, paging, ETags, and SSE snapshot/replay, grant omission, concurrent streams, credential revocation, and stalled reads. `MobileContractFixtureTests` verifies that the shared Swift/Kotlin operation fixture matches the actual Gateway mapper, including a 64-bit sequence.

They do not install Windows services or validate a phone's native TLS/secure-store behavior. Use `test_rest_p3.ps1 -PrepareSandbox` and [the installed P3 plan](../Sandbox/REST_P3_TEST_PLAN.md), plus [native client acceptance](../Clients/README.md#acceptance-checklist), for those checks.
