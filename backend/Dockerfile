services:
  backend:
    build:
      context: ./backend/TimesheetApp.API
      dockerfile: Dockerfile
    container_name: timesheet-backend
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=${DB_HOST};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}
    networks:
      - timesheet-network
