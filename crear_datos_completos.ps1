# Script para crear proveedores, lotes y órdenes de compra ficticias
# Restaurante de comidas rápidas - Sistema de Inventario DDD

$baseUrl = "http://localhost:5261/api"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  CREACIÓN DE DATOS FICTICIOS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# ============================================
# 1. CREAR PROVEEDORES
# ============================================

Write-Host "1. CREANDO PROVEEDORES..." -ForegroundColor Yellow

$proveedor1 = @{
    nombre = "Carnes Premium Ltda"
    nit = "900123456-7"
    telefono = "+57 310 555 1234"
    email = "ventas@carnespremium.com"
    direccion = @{
        calle = "Cra 45 # 67-89"
        ciudad = "Medellin"
        pais = "Colombia"
        codigoPostal = "050001"
    }
    personaContacto = "Carlos Ramirez"
}
    @{
        nombre = "Distribuidora de Vegetales del Valle"
        nit = "800234567-8"
        telefono = "+57 320 555 2345"
        email = "pedidos@vegetalesdelvalle.com"
        direccion = @{
            calle = "Av. 6 Norte # 23-45"
            ciudad = "Cali"
            pais = "Colombia"
            codigoPostal = "760001"
        }
        personaContacto = "María González"
    },
    @{
        nombre = "Panadería Industrial El Trigo"
        nit = "900345678-9"
        telefono = "+57 315 555 3456"
        email = "ventas@panaderiaeltrigo.com"
        direccion = @{
            calle = "Calle 80 # 50-25"
            ciudad = "Bogotá"
            pais = "Colombia"
            codigoPostal = "110111"
        }
        personaContacto = "Pedro Martínez"
    },
    @{
        nombre = "Salsas y Aderezos SAS"
        nit = "900456789-0"
        telefono = "+57 318 555 4567"
        email = "comercial@salsasaderezos.com"
        direccion = @{
            calle = "Cra 30 # 45-12"
            ciudad = "Medellín"
            pais = "Colombia"
            codigoPostal = "050002"
        }
        personaContacto = "Laura Pérez"
    },
    @{
        nombre = "Bebidas y Refrescos Colombia"
        nit = "800567890-1"
        telefono = "+57 312 555 5678"
        email = "ventas@bebidasrefrescos.com"
        direccion = @{
            calle = "Zona Industrial # 12-34"
            ciudad = "Barranquilla"
            pais = "Colombia"
            codigoPostal = "080001"
        }
        personaContacto = "Andrés López"
    }
)

$proveedoresCreados = @()

foreach ($prov in $proveedores) {
    try {
        $json = $prov | ConvertTo-Json -Depth 10
        $response = Invoke-WebRequest -Uri "$baseUrl/proveedores" -Method Post -Body $json -ContentType "application/json" -UseBasicParsing
        $proveedorCreado = $response.Content | ConvertFrom-Json
        $proveedoresCreados += $proveedorCreado
        Write-Host "  ✓ Proveedor creado: $($prov.nombre)" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Error al crear proveedor: $($prov.nombre)" -ForegroundColor Red
        Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n  Total proveedores creados: $($proveedoresCreados.Count)" -ForegroundColor Cyan

# ============================================
# 2. OBTENER INGREDIENTES Y PROVEEDORES
# ============================================

Write-Host "`n2. OBTENIENDO INGREDIENTES Y PROVEEDORES..." -ForegroundColor Yellow

$responseIngredientes = Invoke-WebRequest -Uri "$baseUrl/ingredientes" -UseBasicParsing
$ingredientes = $responseIngredientes.Content | ConvertFrom-Json

$responseProveedores = Invoke-WebRequest -Uri "$baseUrl/proveedores" -UseBasicParsing
$proveedoresTotales = $responseProveedores.Content | ConvertFrom-Json

Write-Host "  ✓ Ingredientes disponibles: $($ingredientes.Count)" -ForegroundColor Green
Write-Host "  ✓ Proveedores disponibles: $($proveedoresTotales.Count)" -ForegroundColor Green

# ============================================
# 3. CREAR ÓRDENES DE COMPRA
# ============================================

Write-Host "`n3. CREANDO ÓRDENES DE COMPRA..." -ForegroundColor Yellow

# Función para obtener fecha futura
function Get-FechaFutura {
    param([int]$dias)
    return (Get-Date).AddDays($dias).ToString("yyyy-MM-dd")
}

$ordenesCompra = @(
    # Orden 1: Carne de Res - Carnes Premium
    @{
        proveedorId = $proveedoresTotales[0].id
        detalles = @(
            @{
                ingredienteId = $ingredientes[0].id  # Carne de Res
                cantidad = 50
                precioUnitario = 18000
                moneda = "COP"
            }
        )
        fechaEsperada = Get-FechaFutura 5
        observaciones = "Entrega urgente - Stock bajo"
    },
    # Orden 2: Pechuga de Pollo - Carnes Premium
    @{
        proveedorId = $proveedoresTotales[0].id
        detalles = @(
            @{
                ingredienteId = $ingredientes[1].id  # Pechuga de Pollo
                cantidad = 40
                precioUnitario = 14000
                moneda = "COP"
            }
        )
        fechaEsperada = Get-FechaFutura 3
        observaciones = "Producto refrigerado"
    },
    # Orden 3: Vegetales - Distribuidora del Valle
    @{
        proveedorId = $proveedoresTotales[1].id
        detalles = @(
            @{
                ingredienteId = $ingredientes[2].id  # Tomate
                cantidad = 30
                precioUnitario = 3500
                moneda = "COP"
            }
        )
        fechaEsperada = Get-FechaFutura 2
        observaciones = "Producto fresco - Revisar calidad"
    },
    # Orden 4: Lechuga - Distribuidora del Valle
    @{
        proveedorId = $proveedoresTotales[1].id
        detalles = @(
            @{
                ingredienteId = $ingredientes[3].id  # Lechuga
                cantidad = 25
                precioUnitario = 2800
                moneda = "COP"
            }
        )
        fechaEsperada = Get-FechaFutura 2
        observaciones = "Producto fresco"
    },
    # Orden 5: Pan Hot Dog - Panadería El Trigo
    @{
        proveedorId = $proveedoresTotales[2].id
        detalles = @(
            @{
                ingredienteId = $ingredientes[5].id  # Pan Hot Dog
                cantidad = 100
                precioUnitario = 800
                moneda = "COP"
            }
        )
        fechaEsperada = Get-FechaFutura 4
        observaciones = "Pan fresco - Entrega diaria"
    },
    # Orden 6: Salsas - Salsas y Aderezos SAS
    @{
        proveedorId = $proveedoresTotales[3].id
        detalles = @(
            @{
                ingredienteId = $ingredientes[6].id  # Mayonesa
                cantidad = 20
                precioUnitario = 8500
                moneda = "COP"
            }
        )
        fechaEsperada = Get-FechaFutura 7
        observaciones = "Empaques de 1kg"
    },
    # Orden 7: Ketchup - Salsas y Aderezos SAS
    @{
        proveedorId = $proveedoresTotales[3].id
        detalles = @(
            @{
                ingredienteId = $ingredientes[7].id  # Ketchup
                cantidad = 15
                precioUnitario = 7200
                moneda = "COP"
            }
        )
        fechaEsperada = Get-FechaFutura 7
        observaciones = "Botellas de 500ml"
    },
    # Orden 8: Bebidas - Bebidas y Refrescos
    @{
        proveedorId = $proveedoresTotales[4].id
        detalles = @(
            @{
                ingredienteId = $ingredientes[9].id  # Coca-Cola
                cantidad = 60
                precioUnitario = 2500
                moneda = "COP"
            }
        )
        fechaEsperada = Get-FechaFutura 3
        observaciones = "Botellas de 400ml"
    }
)

$ordenesCreadas = @()

foreach ($orden in $ordenesCompra) {
    try {
        $json = $orden | ConvertTo-Json -Depth 10
        $response = Invoke-WebRequest -Uri "$baseUrl/ordenescompra" -Method Post -Body $json -ContentType "application/json" -UseBasicParsing
        $ordenCreada = $response.Content | ConvertFrom-Json
        $ordenesCreadas += $ordenCreada
        
        $ingredienteNombre = ($ingredientes | Where-Object { $_.id -eq $orden.detalles[0].ingredienteId }).nombre
        $proveedorNombre = ($proveedoresTotales | Where-Object { $_.id -eq $orden.proveedorId }).nombre
        
        Write-Host "  ✓ Orden creada: $ingredienteNombre de $proveedorNombre" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Error al crear orden de compra" -ForegroundColor Red
        Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n  Total órdenes creadas: $($ordenesCreadas.Count)" -ForegroundColor Cyan

# ============================================
# 4. APROBAR Y RECIBIR ALGUNAS ÓRDENES
# ============================================

Write-Host "`n4. APROBANDO Y RECIBIENDO ÓRDENES..." -ForegroundColor Yellow

# Aprobar las primeras 4 órdenes
for ($i = 0; $i -lt [Math]::Min(4, $ordenesCreadas.Count); $i++) {
    try {
        $ordenId = $ordenesCreadas[$i].id
        $body = @{ usuarioId = "sistema" } | ConvertTo-Json
        Invoke-WebRequest -Uri "$baseUrl/ordenescompra/$ordenId/aprobar" -Method Post -Body $body -ContentType "application/json" -UseBasicParsing | Out-Null
        Write-Host "  ✓ Orden aprobada: $($i+1)" -ForegroundColor Green
        
        Start-Sleep -Milliseconds 500
        
        # Recibir las primeras 2 órdenes aprobadas para crear lotes
        if ($i -lt 2) {
            $fechaVencimiento = (Get-Date).AddDays(30).ToString("yyyy-MM-dd")
            $recepcionData = @{
                ordenId = $ordenId
                fechaRecepcion = (Get-Date).ToString("yyyy-MM-dd")
                cantidadRecibida = $ordenesCompra[$i].detalles[0].cantidad
                lote = @{
                    codigo = "LOTE-$(Get-Random -Minimum 1000 -Maximum 9999)"
                    fechaVencimiento = $fechaVencimiento
                    ubicacion = "Almacén Principal"
                }
                observaciones = "Recepción conforme"
            } | ConvertTo-Json -Depth 10
            
            Invoke-WebRequest -Uri "$baseUrl/ordenescompra/$ordenId/recibir" -Method Post -Body $recepcionData -ContentType "application/json" -UseBasicParsing | Out-Null
            Write-Host "  ✓ Orden recibida y lote creado: $($i+1)" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ✗ Error al procesar orden $($i+1)" -ForegroundColor Red
        Write-Host "    Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ============================================
# RESUMEN FINAL
# ============================================

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  RESUMEN DE CREACIÓN" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Proveedores creados:    $($proveedoresCreados.Count)" -ForegroundColor White
Write-Host "  Órdenes creadas:        $($ordenesCreadas.Count)" -ForegroundColor White
Write-Host "  Órdenes aprobadas:      4" -ForegroundColor White
Write-Host "  Lotes creados:          2" -ForegroundColor White
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "✅ Proceso completado exitosamente!`n" -ForegroundColor Green
