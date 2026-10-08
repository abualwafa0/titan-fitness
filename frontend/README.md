# Titan Fitness - Staff Portal (Angular 20)

Front-end of the Titan Fitness Staff Portal. It talks to the `TitanFitness.API` backend.

## Requirements

- Node.js 20.19 or newer (22.12+ also works)
- The backend running with the **https** profile: `https://localhost:7259`
- Open `https://localhost:7259/swagger` once in the browser and accept the development
  certificate (or run `dotnet dev-certs https --trust`)

## Run

```
npm install
npm start
```

Then open http://localhost:4200

The backend only allows CORS from `http://localhost:4200`, so keep this port.

## Configuration

`src/environments/environment.ts`

| Key | Meaning |
|---|---|
| `apiUrl` | Base URL of the API (`https://localhost:7259/api`) |
| `filesUrl` | Base URL used to show uploaded member photos |

## Structure

```
src/app/
  core/       shell layout, app-wide services, interceptor, small pages, models, utils
  shared/     reusable components, directives, pipes
  dashboard/  members/  classes/  trainers/  plans/  check-ins/    (feature folders)
```

Every component has three files: `name.component.ts`, `name.component.html`, `name.component.css`.

## Routes

`/login` · `/dashboard` (default) · `/members`, `/members/:id`, `/members/:id/freeze` · `/classes` · `/trainers`, `/trainers/new`, `/trainers/:id`, `/trainers/:id/edit` (manager only) · `/plans`, `/plans/new`, `/plans/:id`, `/plans/:id/edit` (manager only) · `/access-denied` · `**` not found.
