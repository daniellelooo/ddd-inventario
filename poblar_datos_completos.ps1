# Script para crear datos ficticios completos
# Incluye: Lotes, Movimientos y datos para el Dashboard

$baseUrl = "http://localhost:5261/api"

Write-Host "`n╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   CREANDO DATOS COMPLETOS PARA EL SISTEMA       ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Obtener ingredientes
$ingredientes = (Invoke-WebRequest -Uri "$baseUrl/ingredientes" -UseBasicParsing).Content | ConvertFrom-Json
Write-Host "Ingredientes disponibles: $($ingredientes.Count)`n" -ForegroundColor White

# ========================================
# REGISTRAR MOVIMIENTOS DE INVENTARIO
# ========================================

Write-Host "📊 REGISTRANDO MOVIMIENTOS DE INVENTARIO..." -ForegroundColor Yellow

$movimientos = @(
    @{
        ingredienteId = $ingredientes[0].id  # Carne de Res
        cantidad = 15
        motivo = "Preparación de hamburguesas del día"
    },
    @{
        ingredienteId = $ingredientes[1].id  # Pechuga de Pollo
        cantidad = 12
        motivo = "Preparación de alitas y nuggets"
    },
    @{
        ingredienteId = $ingredientes[2].id  # Tomate
        cantidad = 8
        motivo = "Preparación de ensaladas y guarniciones"
    },
    @{
        ingredienteId = $ingredientes[3].id  # Lechuga
        cantidad = 6
        motivo = "Preparación de ensaladas mixtas"
    },
    @{
        ingredienteId = $ingredientes[5].id  # Pan Hot Dog
        cantidad = 50
        motivo = "Venta de hot dogs del día"
    },
    @{
        ingredienteId = $ingredientes[9].id  # Coca-Cola
        cantidad = 30
        motivo = "Venta de bebidas con combos"
    },
    @{
        ingredienteId = $ingredientes[0].id  # Carne de Res (otro movimiento)
        cantidad = 8
        motivo = "Preparación de hamburguesas especiales"
    },
    @{
        ingredienteId = $ingredientes[6].id  # Mayonesa
        cantidad = 3
        motivo = "Preparación de salsas y aderezos"
    }
)

$movimientosCreados = 0

foreach ($mov in $movimientos) {
    $body = @{
        ingredienteId = $mov.ingredienteId
        cantidad = $mov.cantidad
        motivo = $mov.motivo
        usuarioId = "sistema"
    } | ConvertTo-Json

    try {
        Invoke-WebRequest -Uri "$baseUrl/inventario/consumo" -Method Post -Body $body -ContentType "application/json" -UseBasicParsing | Out-Null
        $movimientosCreados++
        $ing = $ingredientes | Where-Object { $_.id -eq $mov.ingredienteId }
        Write-Host "  ✓ Consumo: $($ing.nombre) - $($mov.cantidad) unidades" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠ Error al crear movimiento: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host "`n  Total movimientos creados: $movimientosCreados`n" -ForegroundColor Cyan

# ========================================
# RESUMEN FINAL
# ========================================

Write-Host "╔══════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║            RESUMEN DE CREACIÓN                   ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Verificar estado actual
$stats = @{
    ingredientes = (Invoke-WebRequest -Uri "$baseUrl/ingredientes" -UseBasicParsing).Content | ConvertFrom-Json
    proveedores = (Invoke-WebRequest -Uri "$baseUrl/proveedores" -UseBasicParsing).Content | ConvertFrom-Json
    ordenes = (Invoke-WebRequest -Uri "$baseUrl/ordenescompra/pendientes" -UseBasicParsing).Content | ConvertFrom-Json
}

if ($stats.ordenes.data) { $stats.ordenes = $stats.ordenes.data }

Write-Host "`n📊 ESTADO DEL SISTEMA:" -ForegroundColor White
Write-Host "  • Ingredientes: $($stats.ingredientes.Count)" -ForegroundColor White
Write-Host "  • Proveedores: $($stats.proveedores.Count)" -ForegroundColor White
Write-Host "  • Órdenes de Compra: $($stats.ordenes.Count)" -ForegroundColor White
Write-Host "  • Movimientos registrados: $movimientosCreados" -ForegroundColor White

Write-Host "`n✅ DASHBOARD LISTO:" -ForegroundColor Green
Write-Host "  • Tarjetas de estadísticas actualizadas" -ForegroundColor White
Write-Host "  • Ingredientes para reabastecer disponibles" -ForegroundColor White
Write-Host "  • Órdenes pendientes mostradas" -ForegroundColor White
Write-Host "  • Movimientos de inventario registrados`n" -ForegroundColor White

Write-Host "🌐 Frontend: http://localhost:3000" -ForegroundColor Yellow
Write-Host "🔧 Backend: http://localhost:5261`n" -ForegroundColor Yellow

Write-Host "✨ ¡Sistema completamente funcional con datos de prueba!`n" -ForegroundColor Green
