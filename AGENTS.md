# GameCheckers Project Guidelines

GameCheckers is a monorepo containing:
- **GameBase**: .NET 10 Checkers domain library.
- **GameBase.Tests**: NUnit 4 and Moq tests for the domain library.
- **GameApi**: ASP.NET Core 10 API, using OpenAPI/Scalar and Serilog.
- **GameApp**: .NET 10 console client.
- **GameWeb**: React 19, TypeScript 5.9, Vite 7, Tailwind CSS 4, and React Router 7.

## Project Boundaries
1. Keep game rules and domain models in `GameBase`; keep HTTP concerns in `GameApi`, terminal interaction in `GameApp`, and browser UI in `GameWeb`.
2. `GameBase` must not depend on UI, HTTP, or ASP.NET Core. Its current logging dependency is limited to `Microsoft.Extensions.Logging.Abstractions`.
3. Keep `GameApi` service operations in the `Result<T>` pattern and have controllers map success to `200 OK` and failure to `400 Bad Request`.
4. Board coordinates use zero-based `x` and `y`. Keep the frontend mapping consistent with `GameApi` DTOs and `GameBase` board positions.
5. `GameApi.Data.GameStore` is registered as a singleton and holds the active game in process memory. Preserve this state/lifetime behavior unless the task explicitly changes it.

## Keeping Guidance Current
- For every code fix, review the applicable `AGENTS.md` files and update the relevant guidance in the same change so it continues to match the implementation. This applies especially when code changes behavior, architecture, dependencies, commands, or constraints.
- Keep guidance factual and scoped to the project it governs; do not describe planned behavior or unsupported test coverage as existing behavior.

## Validation Commands
- .NET solution and domain tests: `dotnet test GameCheckers.sln`
- Frontend type-check/build: from `GameWeb`, run `npm run build`.
- Frontend lint: from `GameWeb`, run `npm run lint`.
- Frontend end-to-end tests: from `GameWeb`, run `npm run test:e2e` (Playwright starts the API and Vite development server).

## Local Agent Skills
Repository skills are indexed by `.agents/skills.json` and stored under `.agents/skills/`.
Use the relevant available skill when a task matches it, including `dotnet`, `dotnet-aspnetcore`, `dotnet-test`, `react-best-practices`, `composition-patterns`, or `web-design-guidelines`.
