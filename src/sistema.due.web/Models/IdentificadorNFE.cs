using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cargill.DUE.Web.Models
{
    public class IdentificadorNFE
    {
        public string Identificador { get; set; }

        public IdentificadorNFE(string identificador)
        {
            Identificador = identificador;
        }
        public IdentificadorNFE()
        {

        }
    }

}