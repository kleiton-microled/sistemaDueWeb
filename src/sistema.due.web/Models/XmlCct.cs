using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cargill.DUE.Web.Models
{
    public class XmlCct
    {
        public string Identificador { get; set; }
        public string CnpjResponsavel { get; set; }
        public string CodigoUrf { get; set; }
        public string CodigoRa { get; set; }
        public string ChaveNfe { get; set; }
        public string CpfCnpjTransportador { get; set; }
        public string CpfCondutor { get; set; }
        public string PesoAferido { get; set; }
        public string ObservacoesGerais { get; set; }

        public XmlCct(string identificador, string cnpjResponsavel, string codigoUrf, string codigoRa, string chaveNfe, string cpfCnpjTransportador,
            string cpfCondutor, string pesoAferido, string observacoesGerais)
        {
            Identificador = identificador;
            CnpjResponsavel = cnpjResponsavel;
            CodigoUrf = codigoUrf;
            CodigoRa = codigoRa;
            ChaveNfe = chaveNfe;
            CpfCnpjTransportador = cpfCnpjTransportador;
            CpfCondutor = cpfCondutor;
            PesoAferido = pesoAferido;
            ObservacoesGerais = observacoesGerais;
        }
        public XmlCct()
        {

        }
    }
}