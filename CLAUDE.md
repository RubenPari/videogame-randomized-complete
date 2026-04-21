# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Full-stack video game discovery application. Users authenticate, discover random games via the RAWG API, save favorites to a vault, and track discovery history. Supports English, Italian, and Spanish.

- **Frontend:** Vue 3 + Vite + Tailwind CSS + Pinia — in `/videogame-randomized/`
- **Backend:** .NET 10 Minimal APIs + MySQL + EF Core — in `/videogame-randomized-back/`
- **Docker:** `docker-compose up` runs MySQL, backend (:8080), and frontend (:5173)

## Commands

### Frontend (`cd videogame-randomized`)
```
npm run dev       # Dev server on :5173
npm run build     # Production build
npm run lint      # ESLint with auto-fix
npm run format    # Prettier on src/
npm run test      # Vitest (unit tests)
```

### Backend (`cd videogame-randomized-back`)
```
dotnet build
dotnet test       # xUnit (from solution: videogame-randomized-back.Tests)
dotnet run
dotnet watch run  # Hot reload
```

See [API_HTTP.md](videogame-randomized-back/API_HTTP.md) for HTTP status and error body conventions.

## Architecture

### Frontend Layers
- **Views** (`src/views/`) — Route-level pages (HomeView, LoginView, etc.)
- **Components** (`src/components/`) — Reusable UI (GameCard, FilterSection, SaveGamesModal, etc.)
- **Composables** (`src/composables/`) — Business logic hooks. `useGameDiscovery` is the core composable managing game randomization, filters, history, and exclusion logic
- **Stores** (`src/stores/`) — Pinia stores: `useAuthStore` (JWT auth + localStorage), `useVaultStore` (saved games), `useToastStore` (notifications)
- **Services** (`src/services/`) — HTTP clients separated from UI: `api.js` (RAWG), `auth.js`, `database.js`, `httpClient.js` (Axios with JWT interceptor + 401 handling), `translation.js` (Google Translate)
- **i18n** — vue-i18n with `src/locales/{en,it,es}.json`, configured in `src/i18n.js`. Keys organized by feature (`nav`, `home`, `vault`, `game`, `filters`, `auth`, etc.)

### Backend Layers
- **Controllers** — `AuthController`, `SavedGamesController`, `DiscoveryLogController`
- **Services** — `AuthService` (JWT + Identity), `GamesService` (CRUD + statistics), `EmailService` (Mailtrap)
- **Models** — `AppUser`, `Game`, `Genre`, `Platform`, `DiscoveryLogEntry` with many-to-many joins (`GameGenre`, `GamePlatform`)
- **DTOs** — Record types with `init` properties. Separate create/update/response DTOs
- **Mappers** — Mapperly source generators (`GameMapper`)
- **Validators** — FluentValidation with automatic MVC validation (`AddFluentValidationAutoValidation`)

### Auth Flow
Registration → email confirmation (Mailtrap) → login returns JWT → stored in localStorage → httpClient attaches Bearer token → 401 triggers redirect to login.

### Route Guards
Vue Router guards redirect unauthenticated users to `/login` and authenticated users away from auth pages. Views are lazy-loaded with `defineAsyncComponent()`.

## Code Style

### Frontend (Prettier)
- No semicolons, single quotes, 100-char line width, 2-space indent
- Composition API with `<script setup>`, `@/` alias for `src/`
- Import order: Vue core → Pinia stores → services → components (relative)
- Components: PascalCase. Composables: `useCamelCase`. Stores: `use{Name}Store`

### Backend (C#)
- File-scoped namespaces, nullable reference types enabled
- Async methods suffixed with `Async`
- PascalCase public, camelCase private
- Returns `TypedResults` (`Ok`, `NotFound`, `Conflict`)

## Git Conventions
- English commit messages, conventional commits format
- Never commit `.env` or `.env.local` files

## Environment Variables
- **Canonical files:** root `.env` for local values and root `.env.example` as template
- **Frontend:** `VITE_API_BASE_URL`, `VITE_RAWG_API_KEY`, `VITE_YOUTUBE_API_KEY`, `VITE_GOOGLE_TRANSLATE_API_KEY`
- **Backend:** DB connection, JWT config, email/Mailtrap config, CORS origins
