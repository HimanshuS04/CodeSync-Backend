#!/bin/bash

echo "🚀 Starting CodeSync Backend Services..."
echo "========================================="

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Base directory
BASE_DIR="$(cd "$(dirname "$0")" && pwd)"

# Kill any existing dotnet processes on our ports
echo "🧹 Cleaning up old processes..."
lsof -ti:5000 | xargs kill -9 2>/dev/null
lsof -ti:5001 | xargs kill -9 2>/dev/null
lsof -ti:5002 | xargs kill -9 2>/dev/null
lsof -ti:5003 | xargs kill -9 2>/dev/null
lsof -ti:5004 | xargs kill -9 2>/dev/null
lsof -ti:5005 | xargs kill -9 2>/dev/null

sleep 2

# Start AuthService (Port 5001)
echo -e "${BLUE}Starting AuthService on port 5001...${NC}"
cd "$BASE_DIR/CodeSync.AuthService"
dotnet run &
sleep 3

# Start ProjectService (Port 5002)
echo -e "${BLUE}Starting ProjectService on port 5002...${NC}"
cd "$BASE_DIR/CodeSync.ProjectService"
dotnet run &
sleep 3

# Start CollabService (Port 5003)
echo -e "${BLUE}Starting CollabService on port 5003...${NC}"
cd "$BASE_DIR/CodeSync.CollabService"
dotnet run &
sleep 3

# Start ExecutionService (Port 5004)
echo -e "${BLUE}Starting ExecutionService on port 5004...${NC}"
cd "$BASE_DIR/CodeSync.ExecutionService"
dotnet run &
sleep 3

# Start NotificationService (Port 5005)
echo -e "${BLUE}Starting NotificationService on port 5005...${NC}"
cd "$BASE_DIR/CodeSync.NotificationService"
dotnet run &
sleep 3

# Start API Gateway (Port 5000)
echo -e "${BLUE}Starting API Gateway on port 5000...${NC}"
cd "$BASE_DIR/CodeSync.ApiGateway"
dotnet run &
sleep 3

echo ""
echo "========================================="
echo -e "${GREEN}✅ All services started!${NC}"
echo ""
echo "Services:"
echo "  🔐 AuthService          → http://localhost:5001"
echo "  📁 ProjectService       → http://localhost:5002"
echo "  👥 CollabService        → http://localhost:5003"
echo "  ▶️  ExecutionService     → http://localhost:5004"
echo "  🔔 NotificationService  → http://localhost:5005"
echo "  🌐 API Gateway          → http://localhost:5000"
echo ""
echo "Swagger UIs:"
echo "  http://localhost:5001/swagger"
echo "  http://localhost:5002/swagger"
echo "  http://localhost:5003/swagger"
echo "  http://localhost:5004/swagger"
echo "  http://localhost:5005/swagger"
echo ""
echo "Press Ctrl+C to stop all services"
echo "========================================="

# Wait for all background processes
wait