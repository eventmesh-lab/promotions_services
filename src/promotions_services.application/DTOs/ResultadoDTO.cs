using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace promotions_services.application.DTOs
{
    /// <summary>
    /// Clase DTO que se encarga de mostrar un mensaje de exito o fracaso dependiendo del resultado de la operación.
    /// Este DTO es utilizado en la clase "ProductoController" en los endpoints de tipo Get, Delete y Put.
    /// </summary>
    public class ResultadoDTO
    {
        /// <summary>
        /// Atributo que corresponde al mensaje que devolvería el endpoint dependiendo del resultado de la operación.
        /// </summary>
        public string Mensaje { get; set; }
        /// <summary>
        /// Atributo que corresponde a la respuesta recibida por la operación solicitada (True, False).
        /// </summary>
        public bool Exito { get; set; }

    }
}
