# Exception Handling System — Overview & Test Notes

This document explains how exception handling is configured in this ASP.NET Core MVC project, how the `Error` page is populated, and how the tests in `Tests/ErrorPipelineTests.cs` exercise and validate the behavior.

Where it's configured

- `Program.cs` registers the global exception handler only when the environment is NOT Development:

```
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}
```

- That means in Production (or any non-Development environment) unhandled exceptions are routed to `/Home/Error` where the app renders a friendly Error view.

How the Error view model is populated

- `HomeController.Error()` creates an `ErrorViewModel` with the RequestId populated from:

```
RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
```

- `Models/ErrorViewModel` exposes `RequestId` and a boolean `ShowRequestId` (true when RequestId is present). The Error view uses this to display a diagnostic identifier that helps correlate logs/traces.

Behavior differences by environment

- Production / Non-Development: `UseExceptionHandler("/Home/Error")` will catch unhandled exceptions and re-execute the pipeline for the `/Home/Error` path, rendering the friendly error page (HTTP 200 if the view renders successfully).
- Development: the exception handler is not registered. Unhandled exceptions will not be routed to `/Home/Error` by the app; tests observe a direct server error (5xx) or developer exception behavior depending on host/test-server configuration.

What the tests exercise

- `Tests/ErrorPipelineTests.cs` uses `WebApplicationFactory` to start an in-memory test host and exercises the pipeline by:
  - Overriding the environment (`UseEnvironment("Production")` or `UseEnvironment("Development")`).
  - Injecting small test-only startup filters / middleware:
    - `AppendThrowingStartupFilter` — appends middleware that throws for matched request paths (e.g., `/throw` or `/Home/Error`) to simulate application or view failures.
    - `ClearActivityStartupFilter` — inserts middleware at the start of the pipeline to clear `Activity.Current`, forcing the Error view to fall back to `HttpContext.TraceIdentifier`.
  - Sending `HttpClient` requests against the test host and asserting on status codes and response body content (presence of `Request ID:` etc.).

Why these tests matter

- They verify the global exception handler is wired correctly and that diagnostic information (RequestId) is produced for failures — critical for triage in production.
- They verify environment-specific behavior so development and production behave differently as intended.
- They check robustness when the Error view itself throws (the app should return a 5xx and a non-empty response rather than crashing silently).

Test tips & gotchas

- TestServer and HTTPS: the app config uses `UseHttpsRedirection()` and HSTS; TestServer runs in-memory and may log warnings like "Failed to determine the https port for redirect." Tests typically still work, but you may see that warning — it's benign for most integration tests.
- Status codes: In Production tests the exception handler may return `200 OK` (friendly Error view) or `500` if the error handler fails. The tests in this repo accept either outcome but assert the response is informative (contains a RequestId or non-empty body). If you prefer stricter assertions, adapt tests to expect only `200` and fail otherwise.
- RequestId source: the Error model uses `Activity.Current?.Id ?? HttpContext.TraceIdentifier`. Clearing `Activity.Current` in tests verifies the fallback path works. If you add distributed tracing or change how Activity is managed, update tests accordingly.
- Simulating throw routes: The tests append throwing middleware via `IStartupFilter`. As an alternative you can add a test-only controller (compiled conditionally in test builds) exposing `/throw` to simplify test injection.

How to run the tests

```powershell
dotnet restore
dotnet test .\Tests\DummyTests.csproj --filter ErrorPipelineTests
```

Troubleshooting

- If tests unexpectedly return 404 for `/throw`: ensure the startup filter is registered and that it appends middleware after the existing pipeline (the `AppendThrowingStartupFilter` used here calls `next(app)` then `app.Use(...)`).
- If tests show blank responses when the Error view fails: check `Views/Home/Error.cshtml` for code paths that may throw; consider making Error view defensive (null checks) or adjust tests to simulate alternative behavior.

Extensions you can add

- Add a test-only controller route (e.g., `/throw`) compiled under a test symbol to avoid startup filters. This is often cleaner for teams that prefer explicit test endpoints.
- Add assertions that the Error view includes structured markup for RequestId (for easier parsing), or expose the RequestId via a header for simpler assertion in tests.

Generated on: November 20, 2025
