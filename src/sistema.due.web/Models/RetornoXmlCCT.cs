using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace Cargill.DUE.Web.Models
{
    [XmlRoot]
    public class RetornoXmlCCT
    {
        [XmlElement]
        public string message { get; set; }
        [XmlElement]
        public string status { get; set; }
        [XmlElement]
        public string code { get; set; }


    }
}