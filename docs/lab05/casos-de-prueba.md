# Especificación de casos de prueba

Los nombres identifican los métodos de `tests/Lab05.Tests`. Cada fila parametrizada representa varias ejecuciones. Resultado final: 42/42 aprobadas; véase `evidence/lab05.trx`.

## Dominio — Basket — 6 ejecuciones

| Método de prueba | Operación y valores | Resultado esperado |
| --- | --- | --- |
| NewBasketIsEmptyAndKeepsBuyer | Constructor, buyer-1 | Comprador conservado, colección vacía y total 0 |
| AddItemUsesDefaultQuantity | AddItem(10, 25) | Un elemento, producto 10, precio 25, cantidad 1 |
| RepeatedProductAccumulatesQuantityWithoutDuplicatingItem | Producto 10, cantidades 2 y 3 | Un elemento con cantidad 5 |
| TotalItemsSumsQuantitiesOfDifferentProducts | Productos 10 y 20, cantidades 2 y 3 | Dos elementos y total 5 |
| RemoveEmptyItemsPreservesPositiveQuantities | Cantidades 0 y 2 | Se conserva únicamente el producto con cantidad 2 |
| SetNewBuyerIdTransfersOwnership | anonymous → buyer-1 | Nuevo comprador buyer-1 |

## Dominio — BasketItem — 13 ejecuciones

| Método de prueba | Operación y valores | Resultado esperado |
| --- | --- | --- |
| ConstructorAcceptsNonNegativeQuantity | Producto 7, precio 15; cantidades 0, 1 y 2147483647 | Conserva cada cantidad, producto y precio (3 casos) |
| ConstructorRejectsNegativeQuantity | Cantidades -1 y -2147483648 | ArgumentOutOfRangeException (2 casos) |
| AddQuantityAcceptsValuesWithinRemainingCapacity | Cantidad inicial 2; incrementos 0, 3 y 2147483645 | Cantidades 2, 5 y 2147483647 (3 casos) |
| AddQuantityRejectsNegativeIncrementWithoutChangingState | Cantidad 2, incremento -1 | Excepción y cantidad permanece en 2 |
| SetQuantityReplacesPreviousQuantity | Cantidad inicial 2; nueva cantidad 0 o 5 | Cantidad reemplazada por el valor indicado (2 casos) |
| SetQuantityRejectsNegativeValueWithoutChangingState | Cantidad 2; SetQuantity(-1) | Excepción y cantidad permanece en 2 |
| AddQuantityRejectsOverflowAndPreservesQuantity | Cantidad 2147483647; AddQuantity(1) | Excepción y cantidad intacta; caso del ciclo TDD |

## Repositorio — especificaciones — 4 ejecuciones

| Método de prueba | Operación y valores | Resultado esperado |
| --- | --- | --- |
| FilterByIdMatchesOnlyTheRequestedBasket | Filtro ID 7; entidad ID 7 o 8 | true / false; incluye Items (2 casos) |
| FilterByBuyerMatchesOnlyTheRequestedOwner | Filtro buyer-1; entidad buyer-1 o buyer-2 | true / false; incluye Items (2 casos) |

## Aplicación — BasketService — 12 ejecuciones con dobles

| Método de prueba | Operación y valores | Resultado esperado |
| --- | --- | --- |
| AddCreatesBasketWhenBuyerHasNone | Repositorio devuelve null; agregar producto 10, cantidad 2 | Carrito buyer-1 con cantidad 2; AddAsync y UpdateAsync una vez |
| AddUpdatesExistingBasketWithoutCreatingAnother | Carrito con 2; agregar 3 del mismo producto | Misma instancia, cantidad 5, UpdateAsync una vez; no AddAsync |
| AddPropagatesRepositoryFailure | Consulta lanza InvalidOperationException | Propaga el error; no actualiza |
| DeleteRemovesExistingBasket | Repositorio devuelve carrito ID 7 | DeleteAsync una vez para esa instancia |
| DeleteRejectsMissingBasket | ID 999 inexistente | ArgumentNullException; no DeleteAsync |
| SetQuantitiesReturnsNotFoundWithoutWriting | ID 999; repositorio devuelve null | ResultStatus.NotFound; no UpdateAsync |
| SetQuantitiesUpdatesRequestedItemsAndPreservesOthers | Ítems 101=2 y 102=3; diccionario 101=5 | Cantidades 5 y 3; persiste una vez |
| SetQuantitiesRemovesZeroQuantityEvenWithoutLogger | Carrito con cantidad 2; asignar 0; logger null | Colección vacía; persiste una vez |
| SetQuantitiesRejectsNegativeQuantityWithoutPersisting | Cantidad 2; nueva cantidad -1 | ArgumentOutOfRangeException; cantidad intacta y no actualización |
| TransferDoesNothingWhenAnonymousBasketIsMissing | Carrito anónimo inexistente | No agrega, actualiza ni elimina |
| TransferCreatesDestinationAndDeletesAnonymousBasket | Anónimo con cantidad 2; destino inexistente | Crea buyer-1, copia cantidad 2 y elimina anónimo |
| TransferMergesIntoExistingDestination | Anónimo con 2 y comprador con 3 del mismo producto | Un elemento con 5; actualiza destino y elimina anónimo sin crear otro |

## Integración — EfRepository y BasketService — 7 ejecuciones

| Método de prueba | Operación y valores | Resultado esperado |
| --- | --- | --- |
| AddPersistsBasketAndItemsAcrossContexts | Guardar buyer-1, producto 10, cantidad 2; leer desde otro DbContext | ID generado, comprador, cantidad e identidad de relación conservados |
| QueryByBuyerDoesNotReturnAnotherBuyersBasket | Guardar buyer-1 y buyer-2; consultar buyer-2 | Devuelve solo el comprador solicitado |
| GetByIdReturnsNullWhenBasketDoesNotExist | GetByIdAsync(999) | null |
| UpdatePersistsQuantityChange | Guardar cantidad 2, cambiar a 5; limpiar seguimiento y leer | Cantidad 5 persistida |
| DeleteRemovesBasketAndDependentItems | Guardar carrito con un ítem; eliminar | No existe carrito ni ítems dependientes |
| ServiceAndRepositoryPersistUpdatedQuantityTogether | Agregar 2 mediante servicio; actualizar a 4 y leer desde otro contexto | Cantidad 4 persistida |
| ServiceTransferPersistsMergeAndRemovesAnonymousBasket | Crear anónimo con 2 y comprador con 3; transferir | Comprador con cantidad 5 y anónimo eliminado |

SetUp: constructor nuevo por prueba. TearDown de integración: Dispose del contexto y de la conexión. Cada conexión `Data Source=:memory:` mantiene una base SQLite aislada hasta cerrarse. Las pruebas con dobles no acceden a una base de datos.
