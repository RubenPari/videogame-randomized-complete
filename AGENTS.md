# AGENTS.md - Coding Agent Instructions

## Project Overview

Full-stack video game discovery application with Vue 3 frontend and .NET 10 backend.

**Frontend**: Vue 3 + Vite + Tailwind CSS + Pinia (in `/videogame-randomized/`)  
**Backend**: .NET 10 Minimal APIs + Firestore (in `/videogame-randomized-back/`)

---

## Build Commands

### Frontend (Vue 3)
```bash
cd videogame-randomized
npm run dev          # Start dev server (http://localhost:5173)
npm run build        # Production build	npm run preview      # Preview production build
npm run lint         # Run ESLint with auto-fix
npm run format       # Run Prettier on src/
```

### Backend (.NET 10)
```bash
cd videogame-randomized-back
dotnet build         # Build project
dotnet run           # Run application
dotnet watch run     # Run with hot reload
```

### Docker (Optional)
```bash
docker-compose up    # Start all services
```

---

## Testing

**Tests:** Frontend uses Vitest (`npm run test`); backend uses xUnit (`dotnet test`, project `videogame-randomized-back.Tests`). Consider Playwright for E2E later.

---

## Code Style Guidelines

### JavaScript/Vue Files

**Formatting** (Prettier config):
- Indent: 2 spaces
- No semicolons
- Single quotes
- Max line length: 100
- UTF-8 encoding, LF line endings

**Vue 3 Conventions**:
- Use Composition API with `<script setup>`
- Import order: Vue → Pinia → services → components
- Use `@/` alias for src directory imports
- Component names: PascalCase
- Composables: useCamelCase (e.g., `useGameDiscovery`)
- Stores: useCamelCase ending in Store (e.g., `useToastStore`)

**Example**:
```javascript
<script setup>
import { ref, computed } from 'vue'
import { useVaultStore } from '@/stores/useVaultStore'
import api from '@/services/api'
import GameCard from './GameCard.vue'
</script>
```

### C# Files

**Conventions**:
- Use file-scoped namespaces
- Use nullable reference types (`<Nullable>enable</Nullable>`)
- Record types for DTOs
- Async methods suffixed with `Async`
- PascalCase for public members
- camelCase for private fields

**Endpoint Pattern**:
```csharp
public static void MapEndpoints(this IEndpointRouteBuilder app)
{
    app.MapGet("/api/items", GetItems);
}

private static async Task<Ok<List<ItemDto>>> GetItems(ItemService service)
{
    var items = await service.GetAsync();
    return TypedResults.Ok(items);
}
```

---

## Import Guidelines

**Frontend**:
- External libraries first (vue, pinia, axios)
- Internal aliases (`@/`) second
- Relative imports (`./`) last for components
- Group imports by category with blank lines

**Backend**:
- System namespaces first
- Third-party libraries second
- Project namespaces last
- Use explicit usings (avoid `using` inside namespaces)

---

## Error Handling

**Frontend**:
- Use try/catch with console.error for debugging
- Display user-friendly errors in UI
- Use toast notifications via `useToastStore`
- Handle API errors gracefully with fallbacks

**Backend**:
- Return typed Results (`Ok`, `NotFound`, `Conflict`)
- Use FluentValidation for input validation
- FluentValidation automatic validation for controllers
- Consistent error response format

---

## State Management

- Use Pinia stores in `src/stores/`
- Store naming: `use{Name}Store.js`
- Use Composition API pattern with `defineStore`
- Async actions should handle loading states

---

## Styling

- Use Tailwind CSS utility classes
- Custom CSS in `src/assets/main.css`
- Dark theme with zinc/cyan/fuchsia colors
- Support `prefers-reduced-motion`
- Mobile-first responsive design

---

## Git Conventions

- Commit message language: English
- Use conventional commits format
- Do NOT commit `.env` or `.env.local` files

---

## Environment Setup

**Required**:
- Node.js 16+ for frontend
- .NET 10 SDK for backend
- RAWG API key (set in root `.env`)
- Google Cloud credentials for Firestore backend

**Frontend env**:
```
VITE_RAWG_API_KEY=your_key_here
```

**Backend env** (appsettings.Development.json):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=videogames;Uid=root;Pwd=password;"
  }
}
```
