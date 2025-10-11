# 📊 RESUMEN DE DATOS CREADOS - SISTEMA DE INVENTARIO DDD

## ✅ PROVEEDORES CREADOS (5)

1. **Carnes Premium Ltda**

   - NIT: 900123456-7
   - Teléfono: 310 555 1234
   - Email: ventas@carnespremium.com
   - Ciudad: Medellín
   - Contacto: Carlos Ramírez

2. **Distribuidora de Vegetales del Valle**

   - NIT: 800234567-8
   - Teléfono: 320 555 2345
   - Email: pedidos@vegetalesdelvalle.com
   - Ciudad: Cali
   - Contacto: María González

3. **Panadería Industrial El Trigo**

   - NIT: 900345678-9
   - Teléfono: 315 555 3456
   - Email: ventas@panaderiaeltrigo.com
   - Ciudad: Bogotá
   - Contacto: Pedro Martínez

4. **Salsas y Aderezos SAS**

   - NIT: 900456789-0
   - Teléfono: 318 555 4567
   - Email: comercial@salsasaderezos.com
   - Ciudad: Medellín
   - Contacto: Laura Pérez

5. **Bebidas y Refrescos Colombia**
   - NIT: 800567890-1
   - Teléfono: 312 555 5678
   - Email: ventas@bebidasrefrescos.com
   - Ciudad: Barranquilla
   - Contacto: Andrés López

## 📦 ÓRDENES DE COMPRA CREADAS (8)

| #   | Ingrediente                   | Proveedor                  | Cantidad | Precio Unit. | Total    | Entrega |
| --- | ----------------------------- | -------------------------- | -------- | ------------ | -------- | ------- |
| 1   | Carne de Res para Hamburguesa | Carnes Premium             | 50 kg    | $18,000      | $900,000 | +5 días |
| 2   | Pechuga de Pollo              | Carnes Premium             | 40 kg    | $14,000      | $560,000 | +3 días |
| 3   | Tomate                        | Distribuidora de Vegetales | 30 kg    | $3,500       | $105,000 | +2 días |
| 4   | Lechuga                       | Distribuidora de Vegetales | 25 kg    | $2,800       | $70,000  | +2 días |
| 5   | Pan para Hot Dog              | Panadería El Trigo         | 100 und  | $800         | $80,000  | +4 días |
| 6   | Mayonesa                      | Salsas y Aderezos          | 20 kg    | $8,500       | $170,000 | +7 días |
| 7   | Ketchup                       | Salsas y Aderezos          | 15 L     | $7,200       | $108,000 | +7 días |
| 8   | Coca-Cola                     | Bebidas y Refrescos        | 60 L     | $2,500       | $150,000 | +3 días |

**Total en Órdenes**: $2,143,000 COP

## 📊 ESTADO ACTUAL DEL SISTEMA

### Categorías: 5

- Carnes
- Vegetales
- Panes
- Salsas y Aderezos
- Bebidas

### Ingredientes: 16

- Carne de Res para Hamburguesa
- Pechuga de Pollo
- Tomate
- Lechuga
- Cebolla
- Pan para Hot Dog
- Mayonesa
- Ketchup
- Mostaza
- Coca-Cola
- (y 6 más...)

### Proveedores: 5

- Todos activos y disponibles

### Órdenes de Compra: 8

- Estado: Pendientes (listas para aprobar)
- Las primeras 4 están aprobadas y listas para recibir

### Lotes

- Se crearán automáticamente al recibir las órdenes aprobadas

## 🎯 PRÓXIMOS PASOS

1. ✅ **Aprobar órdenes**: Las primeras 4 órdenes han sido aprobadas
2. ⏳ **Recibir órdenes**: Pendiente de recepción para crear lotes
3. ⏳ **Crear lotes**: Se generarán al completar la recepción
4. ✅ **Verificar en el dashboard**: Todos los datos ya están disponibles en el frontend

## 💡 COMANDOS ÚTILES

### Ver proveedores:

```powershell
Invoke-WebRequest -Uri "http://localhost:5261/api/proveedores" -UseBasicParsing
```

### Ver órdenes pendientes:

```powershell
Invoke-WebRequest -Uri "http://localhost:5261/api/ordenescompra/pendientes" -UseBasicParsing
```

### Ver lotes próximos a vencer:

```powershell
Invoke-WebRequest -Uri "http://localhost:5261/api/inventario/lotes/proximos-vencer?diasAnticipacion=365" -UseBasicParsing
```

---

**Fecha de creación**: $(Get-Date -Format "dd/MM/yyyy HH:mm")  
**Sistema**: Inventario DDD - Restaurante de Comidas Rápidas
