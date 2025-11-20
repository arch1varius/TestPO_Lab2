<!-- Copilot / AI agent instructions for this repo -->

# Quick Guide for AI coding agents (concise)

This is an ASP.NET Core MVC admin template (Materio) with a separate Node-based asset pipeline. The notes below are focused, actionable items that let an AI agent become productive quickly.

## Big picture

- **Backend:** ASP.NET Core MVC app. Entry point: `Program.cs`. Controllers live in `Controllers/`, views in `Views/`. Default route: `{controller=Dashboards}/{action=Index}/{id?}`.
- **Frontend:** Source SCSS/JS in `src/`. Build pipeline uses `Gulpfile.js` + `webpack.config.js` and outputs compiled assets into `wwwroot/` (or the `dist` path from `build-config.js`).

## Essential workflows (PowerShell)

- Install / build frontend:
  - `npm install`
  - `npm run build` # development build (invokes `npx gulp build`)
  - `npm run build:prod` # production
- Watch/live-reload (frontend):
  - `npm run watch` # continuous rebuild
  - `npm run serve` # browser-sync dev server (Gulp)
- Backend (dotnet):
  - `dotnet restore; dotnet build; dotnet run`
- Typical dev loop: run `npm run watch` (or `npm run serve`) in one terminal and `dotnet run` in another.

## Project-specific conventions & quick pointers

- Main SCSS entry: `src/site.scss`. SCSS partials live under `src/scss/` and follow the `_*.scss` partial convention; Gulp compiles non-underscored entry files.
- JS: `src/js/` holds modules and page scripts; `webpack.config.js` + `Gulpfile.js` control how entries are bundled and copied to `wwwroot/js/`.
- Dist paths and artifact names: check `build-config.js` for the exact `dist` mapping used by Gulp tasks.
- Icons: `src/fonts/iconify/iconify.js` is run by a Gulp task (`buildIconifyTask`) to generate icon assets—edit there when changing icons.
- Razor layouts: global layout and shared partials are under `Views/Shared/` (notably `Views/Shared/_CommonMasterLayout.cshtml`).
- Server services: `IHttpContextAccessor` is registered in `Program.cs`; some view helpers rely on it.

## Where to make common changes (examples)

- Change theme variables: edit files in `src/scss/_custom-variables/` then run `npm run build`.
- Add a page: create `Controllers/MyPageController.cs` and `Views/MyPage/Index.cshtml`; add frontend JS in `src/js/` if needed and ensure webpack picks it up.
- Edit layout/markup: update `Views/Shared/_CommonMasterLayout.cshtml` or partials under `Views/_Partials/`.

## Integration points & dependencies

- Node/npm: Node (npx) is required to run Gulp/webpack tasks. Packages are in `package.json`.
- Static files: served by `app.UseStaticFiles()` in `Program.cs`—ensure compiled assets end up under the configured `wwwroot`/`dist` path.
- Third-party libs: bundled under `src/libs/` and copied to output by Gulp—do not edit `wwwroot` copies directly.

## Quick checklist for PRs/changes

- Keep edits scoped: prefer changing Views/Controllers for runtime behavior; update `src/` only for UI/asset changes.
- Run `npm run build` and `dotnet run` locally to verify visual changes.
- If adding JS/CSS entries, update `webpack.config.js` or ensure the file matches existing entry patterns so Gulp picks it up.

If you want, I can expand examples for `webpack` entries, the `build-config.js` mapping, or typical controller/view PR changes—tell me which area to flesh out.

<!-- Copilot / AI agent instructions for this repo -->

# Quick Guide for AI coding agents

This repo is an ASP.NET Core MVC template (Materio) with a separate frontend asset pipeline. The goal of these notes is to let an AI agent become productive quickly and avoid generic advice.

## Big picture

- **Backend:** ASP.NET Core MVC application. Entry point: `Program.cs`. Controllers are in `Controllers/` and Razor views are under `Views/` following the usual controller→view folder mapping (for example `DashboardsController` → `Views/Dashboards/Index.cshtml`). Default route is `{controller=Dashboards}/{action=Index}/{id?}`.
- **Frontend assets:** Source SCSS/JS live in `src/`. A Gulp + Webpack pipeline compiles assets into a distribution path (configured in `build-config.js`) and emits files into `wwwroot/` (or the `dist` path configured by `build-config.js`). Key files: `Gulpfile.js`, `webpack.config.js`, `package.json` (scripts).

## Typical developer workflows (commands)

- Restore & run backend (PowerShell):

```
dotnet restore
dotnet build
dotnet run
```

- Frontend build (uses `gulp` via `npx`):

```
npm install
npm run build         # development build (runs `npx gulp build`)
npm run build:prod    # production build
npm run watch         # watch sources and rebuild on change
npm run serve         # starts browser-sync dev server (see Gulpfile)
```

- Full dev cycle: run frontend watcher (`npm run watch`) while running the ASP.NET app (`dotnet run`). Use `npm run serve` for faster live reload of assets when developing UI.

## Project-specific conventions & patterns

- `src/site.scss` is the main SCSS entry; SCSS partials live under `src/scss/`. Gulp expects non-underscored SCSS to compile; partials are `_*.scss`.
- JS entry and page scripts are handled by `webpack.config.js` and the Gulp `webpackJsTask`. After webpack runs, `pageJsTask` may further minify files in the configured `dist` path.
- Icon generation: `src/fonts/iconify/iconify.js` is invoked by a Gulp task (`buildIconifyTask`). If you need to adjust icons, inspect that script and ensure the task runs in Node context.
- Server DI: `IHttpContextAccessor` is registered as a singleton in `Program.cs` — some view helpers or services may rely on it.
- Razor layout: shared layout files are under `Views/Shared/` (look at `_CommonMasterLayout.cshtml`, `_ContentNavbarLayout.cshtml`). Modify these when changing global markup.

## Integration points & external deps

- Node tools and packages controlled by `package.json`. The repo expects `npx`/`node` to run `gulp` tasks.
- Backend uses standard ASP.NET Core packages; static files are served by `app.UseStaticFiles()` in `Program.cs`.
- Third-party libs shipped via npm live under `src/libs/` and will be copied into the `dist` path by the Gulp tasks.

## What to change and where (examples)

- Change site-wide CSS variables or theme colors: edit `src/scss/_custom-variables/` and re-run `npm run build:css`.
- Add a new controller + view: add `Controllers/MyThingController.cs` and `Views/MyThing/Index.cshtml`. Route will work with default routing.
- Add frontend JS for a view: place module in `src/js/` and ensure `webpack.config.js` exposes it or is picked up by the existing entry points; then run `npm run build:js`.

## Useful files to inspect first

- `Program.cs` — app startup and middleware
- `Controllers/` — app behavior and action endpoints
- `Views/Shared/` — master layouts and partials
- `src/` — all source SCSS/JS/assets
- `Gulpfile.js`, `webpack.config.js`, `build-config.js` — asset pipeline and output paths
- `package.json` — npm scripts and dependencies

## Tasks AI agents are expected to perform

- Make minimal, focused edits. Respect existing Razor partials and SCSS architecture.
- When adding UI assets, update `src/` and the Gulp/webpack configuration only if necessary.
- For runtime changes, prefer editing controllers and views over touching `Program.cs` unless adding middleware.

If anything above is unclear or you want more specifics (for example sample webpack entry points or the `build-config.js` mapping), tell me which area to expand and I will update this file.
