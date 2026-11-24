import { expect, test } from "@playwright/test";

test("starts a game and moves a valid black piece", async ({ page }) => {
  let moveRequestCount = 0;
  page.on("request", (request) => {
    if (
      request.method() === "POST" &&
      request.url().endsWith("/api/game/move")
    ) {
      moveRequestCount++;
    }
  });

  await page.goto("/");

  const names = page.getByPlaceholder("Enter name");
  await names.nth(0).fill("Alice");
  await names.nth(1).fill("Bob");
  await page.getByRole("button", { name: "Start New Game" }).click();

  await expect(page).toHaveURL(/\/game$/);
  await expect(page.getByText("Alice")).toBeVisible();
  await expect(page.getByRole("grid", { name: "Checkers Board" })).toBeVisible();

  const source = page.getByRole("button", { name: "Cell A6 (dark)" });
  const target = page.getByRole("button", { name: "Cell B5 (dark) - available target" });

  await source.click();
  await expect(target).toBeVisible();
  await target.click();

  await expect(
    page.getByRole("button", { name: "Cell A6 (dark)" }).getByRole("img"),
  ).toHaveCount(0);
  await expect(
    page.getByRole("button", { name: "Cell B5 (dark)" }).getByRole("img", {
      name: "Black Man",
    }),
  ).toBeVisible();
  expect(moveRequestCount).toBe(1);
});

test("rejects players with the same name", async ({ page }) => {
  await page.goto("/");

  const names = page.getByPlaceholder("Enter name");
  await names.nth(0).fill("Sam");
  await names.nth(1).fill("Sam");
  await page.getByRole("button", { name: "Start New Game" }).click();

  await expect(page.getByText("Players must have different names")).toBeVisible();
  await expect(page).toHaveURL(/\/$/);
});
