# Laboratorio 05 — Pruebas unitarias y dobles de prueba

Trabajo basado en **NimblePros/eShopOnWeb**, commit original `f21d6233b851897371c25c5e7ffa67bb8552dba8`, con licencia MIT conservada. El aporte del laboratorio está en `tests/Lab05.Tests` y en la protección contra desbordamiento de `BasketItem.AddQuantity`.

## Alcance y resultados

- Dominio: `Basket` y `BasketItem` (19 pruebas).
- Repositorio: especificaciones por ID/comprador (4 pruebas unitarias) y persistencia EF Core (5 pruebas de integración).
- Aplicación: `BasketService` (12 pruebas unitarias con NSubstitute y 2 pruebas de integración con repositorio real).
- Total: **42 pruebas aprobadas**, sin fallos ni omisiones.
- Cobertura del módulo seleccionado: **107/107 líneas y 18/18 ramas (100%)** en cinco clases.
- Cobertura del conjunto medido `ApplicationCore` + `Infrastructure`, excluyendo migraciones y código generado: **206/648 líneas (31,7%) y 22/70 ramas (31,4%)**. No se afirma cobertura completa de la tienda.
- `EfRepository<T>` hereda sus operaciones de Ardalis; su 100% cubre las 3 líneas propias del adaptador, no toda esa biblioteca externa.

## Ejecutar

Requisito: SDK .NET 10. Desde la raíz del repositorio:

```powershell
dotnet test tests/Lab05.Tests/Lab05.Tests.csproj --settings lab05.runsettings --collect "XPlat Code Coverage" --logger "trx;LogFileName=lab05.trx" --results-directory artifacts/lab05
dotnet test tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~Basket
```

Para visualizar cobertura:

```powershell
dotnet tool install dotnet-reportgenerator-globaltool --tool-path .tools/reportgenerator --version 5.5.11
.tools/reportgenerator/reportgenerator -reports:"artifacts/lab05/**/coverage.cobertura.xml" -targetdir:artifacts/lab05/html -reporttypes:Html
```

Para abrir la tienda en una terminal independiente:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:UseOnlyInMemoryDatabase="true"
dotnet run --project src/Web/Web.csproj --no-launch-profile --urls http://localhost:5106
```

La tienda usa datos de demostración en memoria. Las pruebas de integración usan una conexión SQLite independiente por caso. `SqliteCatalogContext` conserva los mapeos reales y adapta únicamente la generación de claves específica de SQL Server. Estas pruebas no verifican migraciones ni comportamiento específico de SQL Server.

## TDD y organización

1. RED: agregar 1 a una cantidad `int.MaxValue` debía lanzar `ArgumentOutOfRangeException`; la implementación original no la lanzaba.
2. GREEN: se añadió la validación de capacidad antes de modificar la cantidad.
3. REFACTOR: se integró la validación en `Guard.Against.OutOfRange`, usando `int.MaxValue - Quantity` como límite.

Se usan Arrange, Act y Assert, pruebas parametrizadas (`Theory`), inicialización por constructor y limpieza con `IDisposable`. NSubstitute proporciona respuestas controladas y verificaciones `Received` / `DidNotReceive`; los dobles no sustituyen las pruebas de persistencia reales.

Ramas publicadas: `main`, `development`, `feature/lab05-basket`. Se integró la rama de trabajo en development y luego en main con commits de merge.

## Seguimiento en GitHub

Publicación: 27 de octubre de 2026. Las pruebas se ejecutaron el 26 de octubre de 2026.

- Repositorio público: https://github.com/GinoSebastianD/lab5-eshoponweb
- Tablero Kanban privado de la cuenta: https://github.com/users/GinoSebastianD/projects/1
- Etapas: TO-DO, CURRENT ITERATION, IN PROGRESS, FIX VALIDATION y DONE.
- Issues públicos: #1 dominio (60 min, enhancement), #2 repositorio (60 min, unit test), #3 servicios (70 min, unit test), #4 evidencias e informe (50 min, unit test).
- Responsable de las cuatro tareas: GinoSebastianD. El detalle de aceptación y las evidencias figuran en cada issue.
- La implementación y validación local se completaron antes de crear el tablero. No se atribuye a GitHub una cronología anterior a su creación.
- Los workflows heredados están desactivados únicamente en este repositorio. Los reportes corresponden a ejecuciones locales verificadas.
- Los commits RED ec3e286, GREEN c750b6c y REFACTOR 77a373c se conservan en el historial. El commit de publicación vincula los cuatro issues mediante Fix #1, Fix #2, Fix #3 y Fix #4.

## Evidencias

- `resultados.html`: detalle de los 42 casos, leído de `lab05.trx`.
- `tdd.html`: fragmentos de registros reales RED/GREEN y código final.
- `coverage/index.html`: cobertura del conjunto medido.
- `coverage-basket/index.html`: cobertura filtrada a las cinco clases del carrito.
- `casos-de-prueba.md`: especificación resumida de todas las pruebas.
- `evidence/`: registros, TRX, XML de cobertura y capturas.

La restauración detecta avisos de vulnerabilidades en dependencias del proyecto base y del proveedor SQLite. Esta entrega verifica el módulo del carrito en un entorno local; no incluye una actualización integral de dependencias ni una validación de seguridad para producción.

## Referencias

- Proyecto original: https://github.com/NimblePros/eShopOnWeb
- Arquitectura: https://nimblepros.github.io/eShopOnWeb/explore/architecture.html
- Guía del curso: Lab05 Unit Testing Test Doubles, actividades 1 a 7.
