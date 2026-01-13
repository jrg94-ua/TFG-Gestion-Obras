# Tests

Esta carpeta contendrá las pruebas del proyecto:

## Estructura

```
tests/
├── GestionObras.UnitTests/          # Pruebas unitarias del dominio
├── GestionObras.IntegrationTests/   # Pruebas de integración de API
└── GestionObras.E2ETests/           # Pruebas end-to-end de UI
```

## Ejecutar Tests

```powershell
# Ejecutar todos los tests
dotnet test

# Ejecutar con cobertura de código
dotnet test /p:CollectCoverage=true
```

## Frameworks de Testing

- **xUnit**: Framework principal de testing
- **Moq**: Para mocking de dependencias
- **FluentAssertions**: Para assertions más legibles
- **Testcontainers**: Para tests de integración con BD real

## Ejemplo de Test Unitario

```csharp
public class CalculadorROITests
{
    [Fact]
    public void CalcularROI_ConPresupuestoYCostesReales_DebeCalcularCorrectamente()
    {
        // Arrange
        var proyecto = new Proyecto
        {
            Presupuesto = new Presupuesto { Total = 100000m },
            CostesReales = 80000m
        };
        var calculador = new CalculadorROI();
        
        // Act
        var roi = calculador.CalcularROIActual(proyecto);
        
        // Assert
        roi.Should().Be(25m); // (100k - 80k) / 80k * 100 = 25%
    }
}
```
