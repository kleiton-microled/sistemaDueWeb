using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Serialization;

namespace Cargill.DUE.Web
{
    public partial class Test : System.Web.UI.Page
    {
        const string XML = @"<?xml version=""1.0""?>
                        <DietPlan>
                            <Health>
                                <Fruit>Maçã</Fruit>
                                <Fruit>Mamão</Fruit>
                                <Veggie>Brocolis</Veggie>
                                <Veggie>Rucula</Veggie>
                            </Health>
                        </DietPlan>";

        const string Modelo = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<error>
	<message>Problemas encontrados: </message>
	<tag>[CCTR-APCBVO2929]</tag>
	<date>2021-03-09T08:21:51</date>
	<status>422</status>
	<detail>
		<error>
		<message>A NF-E 50210302003402000922550070000497451000497343 já se encontra recepcionada e sua recepção não poderá ser realizada.Entre em contato com o representante do exportador relatando o problema.</message>
		<code>CCTR-ER0512</code>
		<tag>[CCTR-NDWDSI2929]</tag>
		<date>2021-03-09T08:21:51</date>
		<status>422</status>
		<severity>ERROR</severity>
		</error>
	</detail>
	<severity>ERROR</severity>
	<info>
		<ambiente>PRO</ambiente>
		<mnemonico>CCTR</mnemonico>
		<sistema>Controle de Carga e Trânsito</sistema>
		<trackerId>4kzlHAjr6q</trackerId>
		<url>/cct-ext/api/ext/carga/recepcao-nfe</url>
		<usuario>27501846812</usuario>
		<visao>PRIV</visao>
	</info>
</error>";


        [XmlRoot(ElementName = "error")]
        public class TestSerialization
        {
            [XmlArray("detail")]
            [XmlArrayItem("error", Type = typeof(Message))]
            public Resultado[] resultado;

        }

        //[XmlInclude(typeof(Fruit))]
        //[XmlInclude(typeof(Veggie))]
        //[XmlInclude(typeof(Erro))]
        [XmlRoot(ElementName = "error")]
        public class Resultado
        {
            [XmlArray("error")]
            [XmlArrayItem("message", Type = typeof(ResultadoOK))]
            public ResultadoOK[] resultadoOK;
        }
        public class ResultadoOK
        {
            [XmlText]
            public string message { get; set; }
        }

        public class Message : Resultado { }
        //public class Veggie : Food { }
        //public class Erro : Food
        //{
        //    [XmlArray("error")]
        //    public string[] error { get; set; }
        //}

        public static void Teste()
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(Modelo));
            XmlSerializer xs = new XmlSerializer(typeof(Resultado));
            Resultado obj = (Resultado)xs.Deserialize(ms);
            foreach (var item in obj.resultadoOK)
            {
                Console.WriteLine("{0}: {1}", item.GetType().Name, item.message);
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnValidaToken_Click(object sender, EventArgs e)
        {
            Teste();
        }
    }
}