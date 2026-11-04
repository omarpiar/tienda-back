-- Población de prueba funcional: 2,000 productos (idempotente).
-- Ejecutar después de db/schema.sql o de la migración:
--   psql -h localhost -U postgres -d puntopyme -f db/seed/seed_2000_productos.sql

BEGIN;

INSERT INTO "Categorias" ("Nombre") VALUES
  ('Bebidas'), ('Panadería'), ('Limpieza'), ('Abarrotes'), ('Lácteos'), ('Botanas')
ON CONFLICT ("Nombre") DO NOTHING;

INSERT INTO "Productos" (
  "Nombre", "Sku", "CategoriaId", "PrecioVenta", "CostoCompra", "Stock", "StockMinimo", "Eliminado"
)
SELECT
  'Producto de prueba ' || lpad(g::text, 4, '0'),
  'TEST-' || lpad(g::text, 6, '0'),
  c."Id",
  (costo + 5 + (g % 30))::numeric(12,2),
  costo,
  CASE
    WHEN g % 17 = 0 THEN 0
    WHEN g % 5 = 0 THEN g % 4
    ELSE 10 + (g % 90)
  END,
  5 + (g % 10),
  false
FROM generate_series(1, 2000) AS g
CROSS JOIN LATERAL (
  SELECT (ARRAY['Bebidas', 'Panadería', 'Limpieza', 'Abarrotes', 'Lácteos', 'Botanas'])[((g - 1) % 6) + 1] AS nombre
) cat
JOIN "Categorias" c ON c."Nombre" = cat.nombre
CROSS JOIN LATERAL (SELECT (10 + (g % 90))::numeric(12,2) AS costo) precios
ON CONFLICT ("Sku") WHERE "Eliminado" = false DO NOTHING;

ANALYZE "Productos";
ANALYZE "Categorias";

COMMIT;
