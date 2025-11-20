# Playwright E2E tests for Materio template

Prerequisites:

- Node.js (LTS recommended) and npm
- The ASP.NET app running locally (e.g. `dotnet run`) on `http://localhost:5000` or set `E2E_BASEURL`.

Quickstart (PowerShell):

```powershell
cd e2e
npm ci
# Install Playwright browsers (required once per machine)
npx playwright install --with-deps
# In a separate terminal run the app:
dotnet run --project ..\AspnetCoreMvcFull.csproj
# Then run tests:
npx playwright test
```

Notes:

- Tests use route interception to mock backend responses for registration/login flows so they can run without modifying the server.
- If you prefer to run the server on another port, set `E2E_BASEURL` environment variable before running tests.
  E2E tests using Playwright

Prerequisites

- Node 18+ and npm installed.
- The ASP.NET app must be running and listening on `http://localhost:5000`.

Install & run tests

```powershell
cd e2e
npm ci
npx playwright install
# Start the ASP.NET app in another terminal:
# dotnet run --urls=http://localhost:5000

# Run tests
npm test
```

Notes

- Tests mock server responses for the auth flows because this template does not include actual server-side auth endpoints. The mocks are implemented using Playwright request routing so tests remain deterministic.
- If you implement real auth endpoints, remove the route interception in `auth.spec.js` to exercise real end-to-end server behavior.
