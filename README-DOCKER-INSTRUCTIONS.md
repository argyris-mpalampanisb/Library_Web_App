# 🐳 Docker Instructions

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- Docker set to **Linux containers** (right-click the whale icon → "Switch to Linux containers")
- At least **2 GB RAM** available for Docker

---

## Quick Start

```bash
docker compose up --build

Open http://localhost:8080 in your browser.
Common Commands
Action	Command
Build & start (foreground)	docker compose up --build
Build & start (background)	docker compose up -d --build
Stop & remove containers (keep data)	docker compose down
Stop & remove everything (including data)	docker compose down -v
Rebuild image only (don't start)	docker compose build
Restart (without rebuilding)	docker compose restart
View live logs (all services)	docker compose logs -f
View live logs (app only)	docker compose logs -f app
View live logs (postgres only)	docker compose logs -f postgres
Check container status	docker compose ps
Stop one service	docker compose stop app
Start one service	docker compose start app
Pull latest base images	docker compose pull

Database
Connect to PostgreSQL

docker compose exec postgres psql -U postgres -d library_web_app
 

Run a single SQL command

docker compose exec postgres psql -U postgres -d library_web_app -c "SELECT * FROM \"Books\";"
 

Reset the database (drop & recreate)

docker compose exec postgres psql -U postgres -c "DROP DATABASE library_web_app;"
docker compose exec postgres psql -U postgres -c "CREATE DATABASE library_web_app;"
docker compose restart app
 

    The app re-applies all migrations and re-seeds books on startup.

Troubleshooting
Port already in use

If port 8080 is taken, change it in docker-compose.yml:

ports:
  - "8081:8080"   # ← host:container
 

Then open http://localhost:8081.
"dependency failed to start"

docker compose down -v
docker compose up --build
 

Stale build cache

docker compose build --no-cache
docker compose up -d
 

Remove ALL Docker artifacts (images, volumes, networks)

docker compose down -v --rmi all
 

What's Happening Under the Hood
Service	Image	Port	Purpose
app	Built from Dockerfile (multi-stage)	8080 → 8080	ASP.NET Core MVC app
postgres	postgres:18-alpine	5432 → 5432	PostgreSQL database

    Data persists in the postgres_data volume (survives docker compose down, gone with -v)

    Migrations run automatically on app startup

    Books are seeded automatically if the Books table is empty