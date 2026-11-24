# GameCheckers Project Guidelines

Welcome to the GameCheckers project. This repository is a monorepo consisting of:
- **GameWeb**: React 19 + TypeScript + Vite + Tailwind CSS frontend.
- **GameApi**: ASP.NET Core 10 Web API backend.
- **GameBase**: Pure C# domain logic for Checkers engine (.NET 10).
- **GameBase.Tests**: Unit test suite using NUnit 4 and Moq (.NET 10).
- **GameApp**: Console terminal client (.NET 10).

## Official Agent Skills Available

### Sourced from [vercel-labs/agent-skills](https://github.com/vercel-labs/agent-skills):
- `react-best-practices`: Vercel's performance optimization rules, eliminating waterfalls, bundle size, and re-renders.
- `composition-patterns`: React component composition, compound components, and modern React 19 APIs.
- `web-design-guidelines`: Web Interface Guidelines for UX, accessibility (a11y), and responsive design.

### Sourced from [dotnet/skills](https://github.com/dotnet/skills):
- `dotnet-aspnetcore`: Official Microsoft ASP.NET Core Web API standards, routing, DI, and middleware pipeline.
- `dotnet`: Official Microsoft C# and .NET core guidelines, clean domain modeling, and modern language features.
- `dotnet-test`: Official Microsoft .NET testing standards, AAA pattern, test isolation, and mocking.

## General Monorepo Rules
1. Maintain clear separation between pure game logic (`GameBase`) and presentation/API layers (`GameApi`, `GameApp`, `GameWeb`).
2. Do not introduce UI or HTTP dependencies into `GameBase`.
3. Follow the Result pattern in `GameApi` for all API responses.
4. Keep frontend coordinates (0-indexed `x`, `y`) synchronized with the backend Board model.
