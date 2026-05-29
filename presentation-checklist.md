# Wolverine Study Case — Presentation Checklist
## Introduction
- Wolverine is a **.NET library for building distributed applications** (microservices, message-based apps, etc.)
- Focus on features that make Wolverine a **compelling alternative to NServiceBus**
- Both are feature-complete for general distributed computing — **choice is mostly a matter of taste**
- NServiceBus can be replaced by Wolverine without issues unless doing something exotic

## 1. Mixes Well with Current Setup
- Can run **alongside NServiceBus** — no need to rewrite everything
- Enables **gradual migration** (new projects on Wolverine, existing ones stay on NServiceBus)
- HTTP endpoint package supports **vertical slicing**
- Can run **side-by-side with ASP.NET Core**

## 2. Integration with NServiceBus
- Messages can be sent **both ways** between Wolverine and NServiceBus
- Demo: Wolverine endpoint → NServiceBus handler, and NServiceBus → Wolverine handler
- Works with **existing clean architecture** and **constructor injection**

## 3. What Can Wolverine Do More? (Saga demo)
- Built a saga: request → timeout → auto-reject if not approved in time
- Uses **Entity Framework** for saga state storage
- **Method injection** instead of constructor injection
    - Each handler method only gets what it needs
    - Huge improvement for **testability** — no setup of unused dependencies

- Handlers return a **Tuple** instead of just a Task:
    - Outgoing messages
    - Side effects (DB updates, file writes)
    - HTTP responses (in endpoints)

- Benefits: easier testing (no mocking DB/filesystem), more readable, **promotes reuse of side-effect code**
- **Caveats to mention:**
    - **Failing side effects (e.g. network calls)** → better as a dedicated handler listening for a command to avoid full retry loops
    - **Timed messages** can log errors if the saga is already gone → add an empty `NotFound` handler (recommended by docs)

## 4. Taking It Even Further
- `ApproveHandler` example — saga auto-stops on approval
- **Handler method execution pipeline** (in order):
    1. Load
    2. Validate
    3. Before
    4. Handle
    5. After
    6. Finally

- **Load** — fetches data (DB, files, network); returnable as tuple/deconstructable record, injected piece-by-piece
- **Validate** — uses loaded data (e.g., saga + current time) to validate
- **Finally** — always runs, like a `finally` block (great for cleanup)
- `DenyHandler` example: no `Load` method, but uses the **`[Entity]` attribute** to auto-load and inject the saga
- Trade-off: **less boilerplate + easier testing** vs. **learning new concepts** — benefits outweigh costs long-term

## 5. Help with Setup (JasperFX CLI)
- Replace `app.Run();` with `await app.RunJasperFxCommands(args);`
- Useful commands to demo:
    - `dotnet run -- help`
    - `dotnet run -- describe` — describes current Wolverine setup
    - `dotnet run -- diagnose` — runs diagnostics and suggests improvements
    - `dotnet run -- resources list` — lists registered resources

## 6. CritterWatch
- **Real-time dashboard** for monitoring Wolverine apps
- Shows messages, handlers, saga updates, debugging/troubleshooting info
- ⚠️ Still in **early/beta development** — not yet tested personally
- Current limitations:
    - Requires a **Postgres database** (expected to change by v1)
    - **Advanced features = paid license**, basic monitoring stays free

## 7. Conclusion
- Other points worth a brief mention:
    - Supports **many transports**
    - **LLM-friendly docs**
    - Has improved consistently over time

- Best way to evaluate it = **try it yourself**
- Easy setup, simple endpoint up quickly
- **Great documentation** with lots of examples

### Quick "Don't Forget" Reminders
- 🔑 Emphasize **gradual migration** story early (reduces audience anxiety)
- 🔑 Highlight **testability gains** — likely the most persuasive point for developers
- 🔑 Mention both the **caveats** in section 3 (failing side effects + NotFound handler) so it doesn't sound like a sales pitch
- 🔑 Be transparent about CritterWatch being **beta + partially paid**