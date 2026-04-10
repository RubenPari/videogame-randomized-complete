# HTTP API conventions

This document describes how the VideoGame Randomizer backend uses HTTP status codes and error bodies so clients can handle responses consistently.

## Success responses

- **200 OK** — Typical success for GET, PUT, DELETE, and POST when no resource URL is returned.
- **201 Created** — `POST /api/saved-games` when a new saved game is created; includes `Location`-style semantics via `CreatedAtAction`.

## Client errors (4xx)

- **400 Bad Request** — Validation failures (FluentValidation) or auth flows that return structured failures (e.g. login/register/reset). Bodies use **RFC 7807 Problem Details** (`application/problem+json`): `title`, `detail`, `status`, `type`, `instance`.
- **401 Unauthorized** — Missing or invalid JWT where `[Authorize]` is applied.
- **403 Forbidden** — Authenticated but not allowed (rare in this API).
- **404 Not Found** — Resource not found for the current user (e.g. saved game by id).
- **409 Conflict** — Duplicate saved game on create (`POST /api/saved-games`).

**Login policy:** Failed login (wrong email/password, unconfirmed email) returns **400** with Problem Details, not 401, so the client can show a specific message without treating it as “missing token”. Session expiry still uses **401** from the JWT middleware.

## Server errors (5xx)

- Unhandled exceptions are caught by `GlobalExceptionHandler` and returned as **500** Problem Details. In non-development environments, `detail` is generic; in Development, it may include the exception message.

## Frontend

Use `getApiErrorMessage()` in `videogame-randomized/src/utils/apiError.js` to read both Problem Details (`detail`) and legacy `{ error }` / `{ errors }` shapes.
