# Country Album Reviews

A web app for reviewing country music albums.

My buddy and I currently use a spreadsheet to review albums, rank songs from best to worst, assign album ratings, and track favorites over time. This project is my attempt to turn that process into a real application with a better UI, better stats, and a shared experience.

## Why I’m Building This

I wanted to build something that is both personally useful and a good software engineering project. This app gives me a way to practice building a real full-stack application while replacing a process I already care about.

## Current Features

- Angular frontend
- Angular Material UI
- Home page dashboard
- Next Review section
- Recently Reviewed section
- Quick Stats section
- Latest Review Notes section

## Planned Features

- Albums page with searchable/sortable table
- Album detail page
- Add Album form
- Shared reviews for multiple users
- .NET Web API backend
- SQL database
- Stats and rankings
- Hosted deployment

## Tech Stack

### Frontend
- Angular
- Angular Material
- TypeScript
- CSS

### Backend (planned)
- .NET 10 Web API
- C#

### Database (planned)
- SQL Server or PostgreSQL

## Project Status

Early development.

The app now has a working Angular frontend with core pages scaffolded (home, albums, add album, detail, and stats), plus an initial .NET backend project with database models and migrations.

## Roadmap

### Phase 1 (Complete)

- Build core frontend pages
- Set up routes and navigation
- Create albums page UI
- Create add album page UI

### Phase 2 (In Progress)

- Set up backend API project
- Define initial database schema (albums, artists, songs)
- Build album/review CRUD endpoints
- Connect frontend to backend data

### Phase 3 (Planned)

- Add shared reviews for multiple users
- Add stats, rankings, and comparison views
- Deploy online for shared use


## Getting Started

### Prerequisites
- Node.js
- Angular CLI
- Git

### Run the frontend
```bash
cd frontend
npm install
ng serve
```

Open http://localhost:4200 in your browser.

