const { test, expect } = require('@playwright/test');



test.describe('Dashboard - key actions', () => {
  test('Load dashboard and click Details button', async ({ page }) => {
    await page.goto('/');

    await expect(page.locator('text=Transactions')).toBeVisible();
    await expect(page.locator('text=Weekly Overview')).toBeVisible();

    const details = page.locator('button:has-text("Details")').first();
    if ((await details.count()) > 0) {
      await expect(details).toBeVisible();
      await details.click();
      await expect(page.locator('text=Weekly Overview')).toBeVisible();
    }
  });

  test('Slow asset edge case: delay main.js and ensure page still renders', async ({ page }) => {
    await page.route('**/js/main.js', async route => {
      await new Promise(r => setTimeout(r, 500));
      await route.continue();
    });

    await page.goto('/');

    await expect(page.locator('text=Congratulations Norris')).toBeVisible();
  });
});
