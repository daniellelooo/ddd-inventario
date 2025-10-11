$categorias = @(
    @{nombre="Carnes"; descripcion="Carnes para hamburguesas, pollo, etc."},
    @{nombre="Vegetales"; descripcion="Lechugas, tomates, cebollas, etc."},
    @{nombre="Panes"; descripcion="Panes para hamburguesas y hot dogs"},
    @{nombre="Lácteos"; descripcion="Quesos, leche, cremas"},
    @{nombre="Salsas y Aderezos"; descripcion="Ketchup, mayonesa, mostaza, etc."},
    @{nombre="Papas y Acompañamientos"; descripcion="Papas, aros de cebolla, etc."},
    @{nombre="Bebidas"; descripcion="Gaseosas, jugos, etc."}
)

foreach ($cat in $categorias) {
    $body = @{
        nombre = $cat.nombre
        descripcion = $cat.descripcion
    } | ConvertTo-Json
    
    try {
        Invoke-RestMethod -Uri "http://localhost:5261/api/categorias" -Method POST -ContentType "application/json" -Body $body
        Write-Host " Categoría '$($cat.nombre)' creada" -ForegroundColor Green
    } catch {
        Write-Host " Error creando '$($cat.nombre)': $($_.Exception.Message)" -ForegroundColor Red
    }
}
