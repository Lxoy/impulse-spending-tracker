---
name: entity-framework
description: Update EF Core models and relationships, generate and review migrations, and validate database-impacting changes safely in this ASP.NET MVC project.
argument-hint: "[change summary] [entity/model] [migration name]"
---

# Entity Framework Skill

Use this skill when a task changes EF Core domain classes, relationships, or persistence behavior.

## When to use

Use for requests like:
- Add or modify a property in an EF model.
- Add a new EF entity and wire it into DbContext.
- Change relationship/cardinality (1-N, N-N, optional FK, cascade behavior).
- Generate migration after model changes.
- Review migration for safety before applying it.

Do not use this skill for pure Razor/UI-only edits without EF changes.

## Repository-aware conventions

- Project path: impulse-spending-tracker/impulse-spending-tracker.csproj
- DbContext: Data/ImpulseSpendingDbContext.cs
- Models folder: Models/
- Migrations folder: Migrations/
- Prefer additive, backward-safe schema changes unless user explicitly requests destructive changes.

## Required workflow

1. Discover impact surface
- Read affected model files in Models/.
- Read Data/ImpulseSpendingDbContext.cs for DbSet and Fluent API rules.
- Check repositories/controllers/views that consume changed fields.

2. Implement model and DbContext changes
- Add/adjust properties with clear nullability and defaults.
- Update navigation properties and FK properties consistently.
- Add/update DbSet<T> and Fluent API configuration where needed.
- Preserve existing naming and coding style.

3. Generate migration
- Use a descriptive migration name in PascalCase, e.g. AddCooldownDecisionEntity.
- Preferred command from repository root:
  - dotnet ef migrations add <MigrationName> --project impulse-spending-tracker/impulse-spending-tracker.csproj --startup-project impulse-spending-tracker/impulse-spending-tracker.csproj

4. Review migration quality
- Verify Up/Down methods are symmetrical.
- Flag potential data loss (drop/rename/type narrowing) before proceeding.
- Ensure indexes, constraints, FK actions, and defaults match intent.

5. Optional database update (only when requested)
- dotnet ef database update --project impulse-spending-tracker/impulse-spending-tracker.csproj --startup-project impulse-spending-tracker/impulse-spending-tracker.csproj

6. Validate app integrity
- Build solution and resolve compile errors caused by schema/model changes.
- Update any affected mock/test data paths if needed.

## Safety rules

- Do not apply destructive schema operations silently.
- Do not remove existing columns/tables unless user asked explicitly.
- Prefer explicit migrations over auto-generated assumptions left unreviewed.
- If migration introduces risky operations, stop and summarize risk before applying DB update.

## Output contract

When done, report:
- Files changed (models, DbContext, migration files, and dependent code)
- Migration name
- Schema impact summary
- Whether database update was executed or skipped
- Any assumptions and risks

## Example invocations

- /entity-framework Dodaj polje DecisionReason u Purchase i generiraj migraciju AddPurchaseDecisionReason
- /entity-framework Dodaj novi entitet CooldownDecision s relacijom na UserProfile i napravi migraciju
- /entity-framework Promijeni WishlistItem link da bude optional i pregledaj sigurnost migracije
