# Práctica 12: Patrón Specification

Encapsular el criterio para reconocer una orden especial.

## Implementación

Specification<T> expone una expresión y el método IsSatisfiedBy. SpecialOrderSpecification comprueba OrderDetails.Count > 3 después de consolidar productos.

## Archivos principales

- [Specification.cs](../src/1.%20Enterprise%20Business%20Rules/NorthWind.Entities/Abstractions/Specification.cs)
- [SpecialOrderSpecification.cs](../src/2.%20Application%20Business%20Rules/NorthWind.Sales.Backend.BusinessObjects/Specifications/SpecialOrderSpecification.cs)

## Comprobación

Ejecuta Specification_CountsDistinctProducts. Tres productos no disparan el evento; cuatro productos distintos sí.

La solución es acumulativa: esta funcionalidad forma parte del estado final de las 16 guías.
