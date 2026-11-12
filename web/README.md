# LearnWithMe Web

Browser-based React + TypeScript client for learning the Polish alphabet
together.

## Run locally

Requires Node.js 20.19+ or 22.12+.

```sh
npm install
npm run dev
```

Vite prints a local URL, normally `http://localhost:5173`.

## Build for hosting

```sh
npm run build
npm run preview
```

Upload the contents of `dist/` to a static web host. The site does not need
a backend: profiles and learning progress are kept in the current browser's
local storage. They are separate from the MAUI app and are not synchronized
between devices.
