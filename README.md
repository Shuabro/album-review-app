# Track Miners

A web app for reviewing country music albums.

My buddy and I currently use a spreadsheet to review albums, rank songs from best to worst, assign album ratings, and track favorites over time. This project is my attempt to turn that process into a real application with a better UI, better stats, and a shared experience.

## Why I'm Building This

I wanted to build something that is both personally useful and a good software engineering project. This app gives me a way to practice building a real full-stack application while replacing a process I already care about.

## Current Features

**Frontend**
- Angular with Angular Material UI
- Authenticated sidebar + navbar (auth-aware navigation)
- Home page dashboard with next review and recent activity sections
- Albums page with sortable gallery view (by artist, newest, highest rated)
- Login page with JWT-based authentication
- Add Album, Album Detail, and Stats pages (scaffolded)

**Backend**
- .NET 10 Web API
- JWT authentication (login + registration)
- Album listing endpoint
- Static file serving for album cover images
- PostgreSQL database with Entity Framework Core

## Planned Features

- Song ranking per album
- Album detail page with full review
- Add Album form connected to backend
- Stats and rankings across users
- Shared reviews between multiple users
- Hosted deployment

## Tech Stack

### Frontend
- Angular (standalone components, @if control flow)
- Angular Material
- TypeScript

### Backend
- .NET 10 Web API
- ASP.NET Core Identity
- Entity Framework Core
- JWT Bearer authentication

### Database
- PostgreSQL

## Project Status

Active development. The full-stack foundation is in place — authentication works end to end, albums are served from the database, and the frontend connects to the backend via a local proxy. Core pages are scaffolded and the auth-aware UI (sidebar, navbar, user session) is complete.

## Roadmap

### Phase 1 (Complete)
- Build core frontend pages and routes
- Set up Angular Material navigation (navbar + sidebar)
- Auth-aware UI — conditional nav based on login state

### Phase 2 (Complete)
- .NET 10 Web API with PostgreSQL
- Database schema: albums, artists, songs, users, song rankings
- JWT authentication (login + registration)
- Connect frontend to backend via proxy
- User Secrets for local credential management

### Phase 3 (In Progress)
- Album detail page
- Add Album form (connected to API)
- Song ranking UI

### Phase 4 (Planned)
- Stats and ranking views
- Shared reviews for multiple users
- Hosted deployment

## Getting Started

### Prerequisites
- Node.js
- Angular CLI
- .NET 10 SDK
- PostgreSQL

### Run the backend

```bash
cd backend
dotnet run
```

The API runs on http://localhost:5058.

**Note:** Sensitive config (DB password, JWT secret) is managed via .NET User Secrets. Run `dotnet user-secrets set` to configure your local values.

### Run the frontend

```bash
cd frontend
npm install
npm start
```

Open http://localhost:4200 in your browser. API requests are proxied to the backend automatically.
