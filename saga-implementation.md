# Timed Approval Saga Implementation

## What this implementation demonstrates

- A saga type inheriting from `Wolverine.Saga`
- A static `Start` method on the saga type
- Transition checks implemented through `Validate` methods
- Completion paths calling `MarkAsCompleted` (wrapper over `MarkCompleted`)
- Explicit `System.Console` output through Wolverine side effects
- Saga state persisted through Entity Framework Core InMemory
- One HTTP endpoint using enum model binding (`action=Start|Approve|Deny|Stop`)
- `409 Conflict` problem details for invalid transitions

## Flow

1. `POST /saga?action=Start` starts a saga and schedules timeout
2. Timeout is scheduled with default `00:02:00`
3. If still `Started`, timeout sets status to `TimedOut` and completes saga
4. `Approve` / `Deny` are only valid from `Started`
5. `Stop` is only valid from `Approved` or `Denied`
6. Invalid transitions are returned as RFC7807 problem details with `409`

## Runtime configuration

- `TimedApprovalSaga:Timeout` in `appsettings.json`
- Test environment overrides timeout to `00:00:01`

## HTTP examples

See `WolverineCaseStudyElia.Host/WolverineCaseStudyElia.Host.http` for ready-to-run requests.

