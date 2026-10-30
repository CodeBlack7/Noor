# Noor Web

Angular 20 storefront and atelier admin for **Noor — Modest Atelier**.

See the [project README](../../README.md) for architecture, setup, demo accounts, and API details.

## Run

From the repository root:

```powershell
.\scripts\run-api.ps1   # http://localhost:5080
.\scripts\run-web.ps1   # http://localhost:4200
```

Or run Angular directly with Node on your `PATH`:

```powershell
npm install
npm start
```

The dev server proxies `/api`, `/images`, and `/uploads` to `http://localhost:5080`.

## Build

```powershell
npm run build
```

The output is written to `dist/noor-web`.
