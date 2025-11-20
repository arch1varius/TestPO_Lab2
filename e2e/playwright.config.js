// Playwright test configuration for local E2E runs.
const { devices } = require('@playwright/test');
/** @type {import('@playwright/test').PlaywrightTestConfig} */
module.exports = {
  timeout: 30_000,
  expect: { timeout: 5000 },
  fullyParallel: false,
  retries: 0,
  reporter: [['list'], ['html', { outputFolder: 'playwright-report' }]],
  testDir: './tests',
  use: {
    headless: true,
    viewport: { width: 1280, height: 720 },
    baseURL: process.env.E2E_BASEURL || 'http://localhost:5055',
    ignoreHTTPSErrors: true
  },
  // Automatically start the ASP.NET app before running tests.
  webServer: {
    command: 'dotnet run --project ../AspnetCoreMvcFull.csproj',
    // The project uses applicationUrl 5055 in launchSettings.json — match that port.
    port: 5055,
    timeout: 120000,
    reuseExistingServer: true
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }]
};
