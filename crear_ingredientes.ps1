# Primero obtener las categorías
$categorias = Invoke-RestMethod -Uri "http://localhost:5261/api/categorias"

# Crear un mapa de categorías por nombre
$catMap = @{}
foreach ($cat in $categorias) {
    $catMap[$cat.nombre] = $cat.id
}

# Ingredientes típicos de restaurante de comidas rápidas
$ingredientes = @(
    @{nombre="Carne de Res para Hamburguesa"; descripcion="Carne molida de res 80/20"; unidadMedida="kg"; stockMinimo=10; stockMaximo=50; categoria="Carnes"},
    @{nombre="Pechuga de Pollo"; descripcion="Pechuga de pollo fresca"; unidadMedida="kg"; stockMinimo=8; stockMaximo=40; categoria="Carnes"},
    @{nombre="Tomate"; descripcion="Tomate fresco en rodajas"; unidadMedida="kg"; stockMinimo=5; stockMaximo=20; categoria="Vegetales"},
    @{nombre="Lechuga"; descripcion="Lechuga iceberg"; unidadMedida="kg"; stockMinimo=3; stockMaximo=15; categoria="Vegetales"},
    @{nombre="Cebolla"; descripcion="Cebolla blanca"; unidadMedida="kg"; stockMinimo=4; stockMaximo=20; categoria="Vegetales"},
    @{nombre="Pan para Hamburguesa"; descripcion="Pan con ajonjolí"; unidadMedida="unidad"; stockMinimo=50; stockMaximo=200; categoria="Panes"},
    @{nombre="Pan para Hot Dog"; descripcion="Pan alargado"; unidadMedida="unidad"; stockMinimo=30; stockMaximo=150; categoria="Panes"},
    @{nombre="Queso Cheddar"; descripcion="Queso americano en lonchas"; unidadMedida="kg"; stockMinimo=5; stockMaximo=25; categoria="Lácteos"},
    @{nombre="Mayonesa"; descripcion="Mayonesa industrial"; unidadMedida="L"; stockMinimo=2; stockMaximo=10; categoria="Salsas y Aderezos"},
    @{nombre="Ketchup"; descripcion="Salsa de tomate"; unidadMedida="L"; stockMinimo=2; stockMaximo=10; categoria="Salsas y Aderezos"},
    @{nombre="Mostaza"; descripcion="Mostaza amarilla"; unidadMedida="L"; stockMinimo=1; stockMaximo=5; categoria="Salsas y Aderezos"},
    @{nombre="Papas Pre-Fritas"; descripcion="Papas congeladas para freír"; unidadMedida="kg"; stockMinimo=15; stockMaximo=60; categoria="Papas y Acompañamientos"},
    @{nombre="Coca-Cola"; descripcion="Gaseosa 2.5L"; unidadMedida="unidad"; stockMinimo=20; stockMaximo=100; categoria="Bebidas"}
)

$creados = 0
$errores = 0

foreach ($ing in $ingredientes) {
    $categoriaId = $catMap[$ing.categoria]
    if (-not $categoriaId) {
        Write-Host " Categoría '$($ing.categoria)' no encontrada para $($ing.nombre)" -ForegroundColor Red
        $errores++
        continue
    }
    
    $body = @{
        nombre = $ing.nombre
        descripcion = $ing.descripcion
        unidadMedida = $ing.unidadMedida
        stockMinimo = $ing.stockMinimo
        stockMaximo = $ing.stockMaximo
        categoriaId = $categoriaId
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri "http://localhost:5261/api/ingredientes" -Method POST -ContentType "application/json" -Body $body | Out-Null
        Write-Host " $($ing.nombre)" -ForegroundColor Green
        $creados++
    } catch {
        Write-Host " Error: $($ing.nombre) - $($_.Exception.Message)" -ForegroundColor Red
        $errores++
    }
}

Write-Host "`n Resumen:" -ForegroundColor Cyan
Write-Host "   Creados: $creados" -ForegroundColor Green
Write-Host "   Errores: $errores" -ForegroundColor Red
