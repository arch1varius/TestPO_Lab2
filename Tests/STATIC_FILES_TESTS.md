# Static Files Integration Tests — Documentation

Files:

- `Tests/StaticFilesTests.cs` — the integration tests that validate the static file middleware behavior.

What these tests check

- Static-file middleware (`app.UseStaticFiles()`) correctly serves files from `wwwroot/`.
- Correct Content-Type detection for JavaScript and image assets.
- Proper handling of missing assets (404 responses).
- Binary content is delivered (non-empty payload for images).

Why this is a valuable integration boundary

- The static-file middleware connects the HTTP pipeline to on-disk build artifacts. If files are missing, are emitted to the wrong folder, or have incorrect MIME types, the front-end will break (missing JS/CSS/images). These tests catch those issues early.

Tests included

- `Get_MainJs_Returns_200_And_JavaScript_Content`

  - Verifies `/js/main.js` returns `200 OK`, has a `Content-Type` containing `javascript`, and returns non-empty text. This confirms the JS bundle is present and readable.

- `Get_Nonexistent_File_Returns_404`

  - Verifies that requesting a non-existent asset returns `404 Not Found`, ensuring the middleware does not silently return unexpected content.

- `Get_Trophy_Image_Returns_Image_Content_And_Has_Length`
  - Verifies an image under `wwwroot/img/illustrations/trophy.png` is served with an `image/*` media type and non-zero byte length. This checks binary serving and that the asset exists in the output.

Test implementation notes

- The tests use `WebApplicationFactory<AspnetCoreMvcFull.TestHostEntryPoint>` to start the application in-memory and send real HTTP requests to the app pipeline.
- No test-only middleware is required for these tests; they exercise the app as-is.
- The tests run quickly and are safe to run in CI as part of a build verification stage (ensure `npm run build` or frontend build step has executed and produced `wwwroot` files before running these tests in CI).

How to run

```powershell
dotnet restore
dotnet test .\Tests\DummyTests.csproj
```

Interpretation

- Passing: Basic static assets are present and served correctly. Good signal that frontend build output is correct and middleware is configured.
- Failing (404 for known file): The build pipeline may have not run, assets emitted to a different folder, or `UseStaticFiles()` is misconfigured.
- Failing (incorrect Content-Type): The runtime may lack mapping for certain extensions or the file may be corrupted; browser behavior could be affected.

CI recommendation

- Add a CI job step to run the frontend build (`npm ci && npm run build`) before running these tests so `wwwroot` is populated with the latest artifacts.

Generated on: November 20, 2025
