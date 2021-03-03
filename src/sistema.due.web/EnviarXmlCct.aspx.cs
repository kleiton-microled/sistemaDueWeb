﻿using Sistema.DUE.Web.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;

namespace Cargill.DUE.Web
{
    public partial class EnviarXmlCct : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnValidaToken_Click(object sender, EventArgs e)
        {
            string file = "";
            string cpf = "27501846812";
            SisComexService.PostWithXmlData(cpf);
            //var Teste = SisComexService.EnviarXmlCct(cpf);
            String path = AppDomain.CurrentDomain.BaseDirectory;

            if (txtUpload.HasFile)
            {
                String str = Server.HtmlEncode(txtUpload.FileName);
                String ext = Path.GetExtension(str);
                file = str;
                if ((ext == ".csv"))
                {
                    path += str;
                    txtUpload.SaveAs(path);
                    lblResult.Text = "arquivo validado com sucesso";
                }
                else
                {
                    lblResult.Text = "nao foi possivel validar o arquivo";
                }
            }
            ConvertXml(file);
        }
        private void ConvertXml(string file)
        {
            string[] source = File.ReadAllLines(@"C:\Users\carva\Documents\projects\sistemaDueWeb\" + file);

            //String xml = "";
            XNamespace xmlns = XNamespace.Get("http://www.pucomex.serpro.gov.br/cct RecepcaoNFE.xsd");
            XNamespace xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace schemaLocation = XNamespace.Get("http://www.pucomex.serpro.gov.br/cct");
            XDocument doc = new XDocument(
            //new XDeclaration("1.0", "utf-8", "yes"),
            //new XComment("This is a comment"),
            new XElement("recepcoesNFE",
                        //new XAttribute(xsi + "schemaLocation", schemaLocation),
                        //new XAttribute(XNamespace.Xmlns + "xsi", xsi),

            from items in source

            let fields = items.Split(';')

            select new XElement("recepcaoNFE",

            new XElement("identificacaoRecepcao", fields[0]),

            new XElement("cnpjResp", fields[1]),
            new XElement("local",
                new XElement("codigoURF", fields[2]),
                new XElement("codigoRA", fields[3])
                ),
            new XElement("notasFiscais",
                new XElement("notaFiscalEletronica",
                new XElement("chaveAcesso", fields[4])
                )),
            new XElement("transportador",
            new XElement("cnpj", fields[5]),
            new XElement("cpfCondutor", fields[6])
            ),
            new XElement("pesoAferido", fields[7]),
            new XElement("observacoesGerais", fields[8])
            ))

            );
            string url = "/cct/api/ext/carga/recepcao-nfe";
            string cpfCertificado = "27501846812";
            var xmlPost = doc.ToString();
            var xdoc = xmlPost.Replace("\r\n", "").Replace(" ", "");
            var xmlOk = xdoc.Replace("<recepcoesNFE>", "<recepcoesNFE xsi:schemaLocation=\"http://www.pucomex.serpro.gov.br/cct RecepcaoNFE.xsd\" xmlns=\"http://www.pucomex.serpro.gov.br/cct\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"); 
            //var xmlPost1 = "<recepcoesNFE xsi:schemaLocation=\"http://www.pucomex.serpro.gov.br/cct RecepcaoNFE.xsd\" xmlns=\"http://www.pucomex.serpro.gov.br/cct\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><recepcaoNFE><identificacaoRecepcao>4005329</identificacaoRecepcao><cnpjResp>02003402000760</cnpjResp><local><codigoURF>0817800</codigoURF><codigoRA>8931321</codigoRA></local><notasFiscais><notaFiscalEletronica><chaveAcesso>35210302003402000841550110001170781033306303</chaveAcesso></notaFiscalEletronica></notasFiscais><transportador><cnpj>20756544000608</cnpj><cpfCondutor>00286683679</cpfCondutor></transportador><pesoAferido>33250.000</pesoAferido><observacoesGerais>Não se aplica</observacoesGerais></recepcaoNFE></recepcoesNFE>";



            //SisComexService.SubmitXmlRequest(url, xmlPost);
            //XmlPost(XmlPost);
            Token token = null;

            if (HttpContext.Current.Session["TOKEN"] == null)
            {
                token = SisComexService.ObterToken(cpfCertificado);
            }
            else
            {
                token = (Token)HttpContext.Current.Session["TOKEN"];
            }
            var headers = SisComexService.ObterHeaders(token);
            var response = SisComexService.CriarRequestKleiton(url, headers, xmlPost, cpfCertificado);
            var xmlRetorno = string.Empty;
            if (response == null)
                return;

            //xmlRetorno = response.Content.ReadAsStringAsync().Result;
            //xmlRetorno = response.StatusCode.ToString();
            xmlRetorno = response.Content.ReadAsStringAsync().Result;
        }
        private void XmlPost(string xml)
        {
            HttpWebRequest request = null;
            byte[] bytes = Encoding.UTF8.GetBytes(xml);

            request = (HttpWebRequest)WebRequest.Create("https://val.portalunico.siscomex.gov.br/cct/ext/carga/recepcao-nfe/ext/carga/recepcao-nfe");
            request.ContentType = "application/xml";
            request.Method = "POST";
            request.ContentLength = bytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    var _response = reader.ReadToEnd().Trim();
                    reader.Close();
                }
                response.Close();
            }
        }
        
    }
}