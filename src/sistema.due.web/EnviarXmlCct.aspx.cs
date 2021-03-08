﻿using Cargill.DUE.Web.Models;
using Sistema.DUE.Web.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Cargill.DUE.Web
{
    public partial class EnviarXmlCct : System.Web.UI.Page
    {
        private readonly HttpResponseMessage response;
        protected void Page_Load(object sender, EventArgs e)
        {
           
        }

        protected void btnValidaToken_Click(object sender, EventArgs e)
        {
            string file = "";
            //string cpf = "27501846812";
            //SisComexService.PostWithXmlData(cpf);
            //var Teste = SisComexService.EnviarXmlCct(cpf);
            string path = Server.MapPath("Uploads\\");

            if (txtUpload.HasFile)
            {
                String str = Server.HtmlEncode(txtUpload.FileName);
                String ext = Path.GetExtension(str);
                file = str;
                if ((ext == ".csv"))
                {
                    path += str;
                    txtUpload.SaveAs(path);
                    //lblResult.Text = "arquivo validado com sucesso";
                }
                else
                {
                    lblContentResult.Visible = true;
                    lblContentResult.Text = "FORMATO DE ARQUIVO INVÁLIDO!!!";
                }
            }
            if (string.IsNullOrEmpty(file))
            {
                lblContentResult.Text = "Escolha um arquivo!!!";
            }
            else
            {
                List<string> convertXmlText = ConvertXml(file);
                if (convertXmlText != null)
                {
                    List<RetornoXmlCCT> responseCct = new List<RetornoXmlCCT>();
                    string _xmlRootAttribute = new XmlRootAttribute().ToString();
                    
                    var rootAttributeSuccess = "retornoServico";
                    for (int i = 0; i < convertXmlText.Count(); i++)
                    {
                        string xml = convertXmlText[i];
                        if (xml.Contains("<error>")) { _xmlRootAttribute = "error"; } else { _xmlRootAttribute = "retornoServico"; }
                        var serializer = new XmlSerializer(typeof(RetornoXmlCCT), new XmlRootAttribute(_xmlRootAttribute));
                        //RetornoXmlCCT result;
                        using (TextReader reader = new StringReader(xml))
                        {
                            responseCct.Add((RetornoXmlCCT)serializer.Deserialize(reader));
                        }
                    }
                        GridView1.DataSource = responseCct;
                        GridView1.DataBind();
                    //((Label)GridView1.HeaderRow.FindControl("message")).Text = "Mensagem";
                    //GridView1.Columns[1].Visible = false;
                    //GridView1.Columns[2].Visible = false;


                    //lblContentResult.Visible = true;
                    //lblContentResult.Text = convertXmlText[0];
                }
            }
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            //e.Row.Attributes.Add("style", "cursor:help;");
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells[2].Text == "CCTR-ER0029")
                {
                    e.Row.BackColor = Color.FromArgb(255,238,186);
                }
                if (e.Row.Cells[2].Text == "CCTR-ER8899")
                {
                    e.Row.BackColor = Color.FromArgb(245, 198, 203);
                }
                if (e.Row.Cells[2].Text == "CCTR-ER8899")
                {
                    e.Row.BackColor = Color.FromArgb(245, 198, 203);
                }
                if (e.Row.Cells[2].Text == "CCTR-ER0122")
                {
                    e.Row.BackColor = Color.FromArgb(245, 198, 203);
                }
            }
        }
        
        private List<string> ConvertXml(string file)
        {
            List<string> xmlRetorno = new List<string>();
            string url = "/cct/api/ext/carga/recepcao-nfe";
            string cpfCertificado = "27501846812";
            string nomeArquivo = Server.MapPath("Uploads\\");
            string[] source = File.ReadAllLines(nomeArquivo + file);
            //pego a primeira linha do array e monto em uma string
            for (int i = 1; i < source.Count(); i++)
            {
                string xmlAtual = source[i];
                string[] split = xmlAtual.Split(';');
                #region Montando o XML
                XDocument doc = new XDocument(
            new XElement("ROOT",
             new XElement("recepcoesNFE",
                        new XElement("recepcaoNFE",
                        new XElement("identificacaoRecepcao", split[0]),
                        new XElement("cnpjResp", split[1]),
                            new XElement("local",
                            new XElement("codigoURF", split[2]),
                            new XElement("codigoRA", split[3])
                                        ),
                        new XElement("notasFiscais",
                        new XElement("notaFiscalEletronica",
                        new XElement("chaveAcesso", split[4])
                                    )),
            new XElement("transportador",
            split[5].ToString().Count() > 11 ? new XElement("cnpj", split[5]) : null,
            split[5].ToString().Count() < 14 ? new XElement("cpf", split[5]) : null,
            //new XElement("cnpj", fields[5]),
            split[6] == null ? new XElement("cpfCondutor", split[6]) : null
            //new XElement("cpfCondutor", fields[6])
            ),
            new XElement("pesoAferido", split[7]),
            new XElement("observacoesGerais", split[8])
            ))
            ));
                #endregion
                var xmlPost = doc.ToString();
                var xdoc = xmlPost.Replace("\r\n", "").Replace(" ", "");
                var xmlOk = xdoc.Replace("<ROOT>", "").Replace("</ROOT>", "").Replace("<recepcoesNFE>", "<recepcoesNFE xsi:schemaLocation=\"http://www.pucomex.serpro.gov.br/cct RecepcaoNFE.xsd\" xmlns=\"http://www.pucomex.serpro.gov.br/cct\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");

                //validando o TOKEN
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
                var response = SisComexService.CriarRequestKleiton(url, headers, xmlOk, cpfCertificado);
                if (response == null)
                    return null;
                xmlRetorno.Add(response.Content.ReadAsStringAsync().Result);
            }
            return xmlRetorno;
        }

    }
}