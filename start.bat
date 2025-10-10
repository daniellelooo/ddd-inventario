@echo off
echo 🍳 Iniciando Sistema de Inventario DDD...
echo.
echo 📝 Monorepo Structure:
echo   └── backend/  (.NET 9 API)
echo   └── frontend/ (React + TypeScript)
echo.
echo 🚀 Starting services...
echo.

start "Backend API" cmd /k "cd /d backend && dotnet run --project InventarioDDD.API"
timeout /t 3 >nul

start "Frontend React" cmd /k "cd /d frontend && npm start"

echo ✅ Services started!
echo.
echo 🌐 URLs:
echo   Frontend: http://localhost:3000
echo   Backend:  http://localhost:5261
echo   Swagger:  http://localhost:5261/swagger
echo.
echo 📚 Documentation available in backend/docs/
echo.
pause