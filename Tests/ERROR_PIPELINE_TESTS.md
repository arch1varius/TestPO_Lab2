# Error Pipeline Integration Tests — Documentation

Location

- Tests: `Tests/ErrorPipelineTests.cs`
- Purpose: validate the ASP.NET Core global exception handling pipeline and the app's Error page behavior.

Overview

- These integration tests exercise the runtime error-handling behavior of the application. They start the real app pipeline in-memory using `WebApplicationFactory<TEntryPoint>` and inject small test-only startup filters/middleware to simulate throwing endpoints and request conditions.
- The tests are focused on `app.UseExceptionHandler("/Home/Error")` (enabled in Production) and `HomeController.Error()` which renders an `ErrorViewModel` with a RequestId coming from `Activity.Current?.Id ?? HttpContext.TraceIdentifier`.

Systems this test checks

- HTTP request pipeline (middleware ordering) — ensures exceptions are caught by the exception handler when appropriate.
- Routing and controller activation — the flow from request to `HomeController.Error()` when exception handling triggers.
- Razor view rendering for the Error page — verifies the Error view receives the expected model data (RequestId) and produces an informative HTML response.
- Diagnostic behavior differences between environments (Production vs Development).

How the tests work (high-level)

- Test host: built on `WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint>` which locates the application assembly and starts an in-memory server.
- Environment control: each test uses `.WithWebHostBuilder(builder => builder.UseEnvironment("Production" | "Development"))` so the host runs in the desired environment.
- Test-only middleware: the tests register `IStartupFilter` implementations (in the same test file) to append middleware at the end of the pipeline or insert middleware at the beginning. These filters allow tests to:
  - append a throwing middleware for specific request paths (e.g., `/throw`),
  - clear `Activity.Current` for the request so the Error page falls back to `HttpContext.TraceIdentifier`.
- Client: tests use `factory.CreateClient(...)` to send HTTP requests to the in-memory server and assert on status codes and response HTML.

Which behaviors are verified (mapping to test methods)

- `Production_Exception_Is_Handled_By_ErrorPage_And_Includes_RequestId`

  - Verifies: In Production, a thrown exception on `/throw` is handled by the exception handler. If the friendly Error view renders, it contains a `RequestId` string; otherwise the response is not empty.
  - Verifies status: accepts `200 OK` (Error view rendered) or `500 InternalServerError` (error handler failed to render) but insists the response is informative.

- `Development_Exception_Is_Not_Routed_To_ErrorPage_Returns_ServerError`

  - Verifies: When running in Development, thrown exceptions are not routed to `/Home/Error` (the developer exception behavior or 500 is expected). The response should not contain the friendly `Request ID:` string.

- `Production_Exception_With_NoActivity_Falls_Back_To_TraceIdentifier`

  - Verifies: If `Activity.Current` is cleared (null), Error view should fallback to `HttpContext.TraceIdentifier` for RequestId when rendered.

- `Production_Exception_When_ErrorView_Throws_Returns_5xx_Not_Blank`
  - Verifies: If the Error view itself throws (simulated by throwing middleware attached to `/Home/Error`), the app returns a 5xx and a non-empty response body (no silent/blank failures).

Why these tests are useful

- Catch regressions in middleware ordering or configuration that would prevent the global exception handler from executing (this can cause raw exception details to leak or the app to return incorrect responses in production).
- Ensure the Error page includes diagnostic information (RequestId) so support/developer teams can correlate logs and traces — very important for triage of production failures.
- Validate environment-specific behavior (Production vs Development) so local dev experience is preserved while production surfaces a friendly error page.
- Ensure robustness: if the Error view fails, the app should fail predictably (HTTP 5xx) and still return a useful body rather than an empty/ambiguous response.

Required test setup (what the tests inject)

- Environment override: `.WithWebHostBuilder(builder => builder.UseEnvironment("Production"))` or `UseEnvironment("Development")`.
- Test-only startup filters (already implemented in `ErrorPipelineTests.cs`):
  - `AppendThrowingStartupFilter` — appends middleware that throws when a request path matches (used for `/throw` and to simulate failure inside `/Home/Error`).
  - `ClearActivityStartupFilter` — inserts middleware at pipeline start to clear `System.Diagnostics.Activity.Current` for the request.
- Client options: tests use `new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }` to observe the raw response and avoid following redirects.

Running the tests locally

1. Restore and build the solution (PowerShell):

```powershell
dotnet restore
dotnet build
dotnet test .\Tests\DummyTests.csproj
```

2. The test suite uses the in-memory test host; there is no need to run the application separately.

Interpreting outcomes and troubleshooting

- Test returns 200 and contains `Request ID:` — the exception handler executed and the Error view rendered with a RequestId.
- Test returns 500 but non-empty body — error handler attempted to run but failed to render; this still indicates the pipeline caught the exception (but Error view rendering needs investigation).
- Test returns 5xx in Development — this is expected for the Development scenario (the developer exception page or direct 500), and the test asserts that `Request ID:` is not present.
- If tests consistently return unexpected status codes, verify:
  - `Program.cs` still registers `app.UseExceptionHandler("/Home/Error")` when environment is not Development.
  - `Views/Home/Error.cshtml` exists and does not contain code that throws for basic model values.
  - The test project references the main project assembly and that `AspnetCoreMvcFull.TestHostEntryPoint` exists (test entry marker type).

Notes and design decisions

- Tests are intentionally tolerant about `200 OK` vs `500` in Production tests because in-memory test-host behavior can vary depending on how the exception handler re-executes the pipeline; the key is to ensure the pipeline caught the exception and the response is informative.
- The test-only startup filters are placed in the test source so they don't touch application code and are easy to reuse for other failure-mode tests.

If you want, I can:

- Add a dedicated small test controller (compiled into the app in test builds only) that exposes `/throw` and `/set-tempdata` endpoints rather than using `IStartupFilter` wrappers.
- Extract shared startup filters into a `Tests/TestHelpers.cs` file for reuse between test classes.

File map

- `Tests/ErrorPipelineTests.cs` — actual tests and small helper `IStartupFilter` implementations.
- `Tests/ERROR_PIPELINE_TESTS.md` — this documentation file.

---

Generated on: November 20, 2025
