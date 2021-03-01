using Sistema.DUE.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Cargill.DUE.Web
{
    public partial class EnviarXmlCct : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnValidaToken_Click(object sender, EventArgs e)
        {
            string cpf = "27501846812";
            string url = "https://val.portalunico.siscomex.gov.br/cct/ext/carga/recepcao-nfe/ext/carga/recepcao-nfe";
            string xml = "<nfe>teste</nfe>";
            SisComexService.PostWithXmlData(cpf);
            //var Teste = SisComexService.EnviarXmlCct(cpf);
        }
    }
}