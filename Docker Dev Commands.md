cd C:\Users\prophet\Documents\VS26F1\Library_Web_app

# Full rebuild & start
docker compose up --build

# Background
docker compose up -d --build

# Stop (keep data)
docker compose down

# Stop (wipe data)
docker compose down -v

# Logs
docker compose logs -f app

# psql from Docker
docker compose exec postgres psql -U postgres -d library_web_app   
