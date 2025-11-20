const { test, expect } = require('@playwright/test');

test.describe('Authentication (UI) flows', () => {
  test('Register - success (mocked backend)', async ({ page }) => {
    await page.route('**/', async route => {
      const req = route.request();
      if (req.method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'text/html',
          body: '<html><body><h1>Mocked Dashboard</h1><div>Congratulations Norris!</div></body></html>'
        });
        return;
      }
      await route.continue();
    });

    await page.goto('/Auth/RegisterBasic');

    await page.fill('#username', 'e2e-user');
    await page.fill('#email', 'e2e@example.com');
    await page.fill('#password', 'P@ssw0rd!');
    await page.check('#terms-conditions');

    await Promise.all([
      page.waitForResponse(response => response.request().method() === 'POST' && response.status() === 200),
      page.click('button:has-text("Sign up")')
    ]);

    await expect(page.locator('text=Congratulations Norris')).toBeVisible();
    await expect(page.locator('h1')).toHaveText('Mocked Dashboard');
  });

  test('Login - failure (mocked backend returns 400)', async ({ page }) => {
    await page.route('**/', async route => {
      const req = route.request();
      if (req.method() === 'POST') {
        await route.fulfill({
          status: 400,
          contentType: 'text/html',
          body: '<html><body><div class="error">Invalid credentials</div></body></html>'
        });
        return;
      }
      await route.continue();
    });

    await page.goto('/Auth/LoginBasic');

    await page.fill('#email', 'wrong@example.com');
    await page.fill('#password', 'bad-password');

    const [res] = await Promise.all([
      page.waitForResponse(r => r.request().method() === 'POST'),
      page.click('button:has-text("login")')
    ]);

    expect(res.status()).toBe(400);
    await expect(page.locator('.error')).toHaveText('Invalid credentials');
  });

  test('Login - success (mocked backend returns 200)', async ({ page }) => {
    await page.route('**/', async route => {
      const req = route.request();
      if (req.method() === 'POST') {
        await route.fulfill({
          status: 200,
          contentType: 'text/html',
          body: '<html><body><h1>Mocked Dashboard</h1><div>Welcome back, E2E User!</div></body></html>'
        });
        return;
      }
      await route.continue();
    });

    await page.goto('/Auth/LoginBasic');

    await page.fill('#email', 'e2e-user@example.com');
    await page.fill('#password', 'CorrectHorseBatteryStaple');

    await Promise.all([
      page.waitForResponse(r => r.request().method() === 'POST' && r.status() === 200),
      page.click('button:has-text("login")')
    ]);

    await expect(page.locator('h1')).toHaveText('Mocked Dashboard');
    await expect(page.locator('body')).toContainText('Welcome back, E2E User!');
  });
});
