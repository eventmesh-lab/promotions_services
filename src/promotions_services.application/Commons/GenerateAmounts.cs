using promotions_services.domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.application.Commons
{
    public static class GenerateAmounts
    {
            private static readonly Random _random = new Random();
            
            public static EnumAmountDiscount GetAmountDiscountRandom()
            {
                // Obtener todos los valores del enum
                var valores = Enum.GetValues(typeof(EnumAmountDiscount));

                // Seleccionar un índice aleatorio
                int index = _random.Next(valores.Length);

                // Retornar el valor correspondiente
                return (EnumAmountDiscount)valores.GetValue(index)!;
            }

            public static EnumAmountMin GetAmountMixRandom()
            {
                // Obtener todos los valores del enum
                var valores = Enum.GetValues(typeof(EnumAmountMin));

                // Seleccionar un índice aleatorio
                int index = _random.Next(valores.Length);

                // Retornar el valor correspondiente
                return (EnumAmountMin)valores.GetValue(index)!;
            }
        
    }
}
