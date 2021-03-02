using Sistema.DUE.Web.Helpers;
using Sistema.DUE.Web.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Cargill.DUE.Web.Models;
using Sistema.DUE.Web.Models;
using System.IO;
using RestSharp;
using NUnit.Framework;
using RestSharp.Authenticators;
using System.Xml;

namespace Sistema.DUE.Web.Services
{
    public class SisComexService
    {
        private static string PerfilSiscomex = ConfigurationManager.AppSettings["PerfilSiscomex"].ToString();
        private static string UrlSiscomex = ConfigurationManager.AppSettings["UrlSiscomexConsultas"].ToString();

        private static bool RemoteCertificateValidate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }
        public static XmlDocument SubmitXmlRequest(string apiUrl, string reqXml)
        {
            XmlDocument xmlResponse = null;
            HttpWebResponse httpWebResponse = null;
            Stream requestStream = null;
            Stream responseStream = null;
            Token token = null;
            string cpfCertificado = "27501846812";
            if (HttpContext.Current.Session["TOKEN"] == null)
            {
                token = ObterToken(cpfCertificado);
            }
            else
            {
                token = (Token)HttpContext.Current.Session["TOKEN"];
            }
            // Create HttpWebRequest for the API URL.
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(UrlSiscomex) + "cct/api/ext/carga/recepcao-nfe");

            try
            {
                // Set HttpWebRequest properties
                var bytes = Encoding.ASCII.GetBytes(reqXml);
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentLength = bytes.Length;
                httpWebRequest.ContentType = "application/xml";
                httpWebRequest.Headers["Authorization"] = token.SetToken;
                httpWebRequest.Headers["X-CSRF-Token"] = token.CsrfToken;

                //Get Stream object
                requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();

                // Post the Request.
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                // If the submission is success, Status Code would be OK
                if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    // Read response
                    responseStream = httpWebResponse.GetResponseStream();

                    if (responseStream != null)
                    {
                        var objXmlReader = new XmlTextReader(responseStream);

                        // Convert Response stream to XML
                        var xmldoc = new XmlDocument();
                        xmldoc.Load(objXmlReader);
                        xmlResponse = xmldoc;
                        objXmlReader.Close();
                    }
                }

                // Close Response
                httpWebResponse.Close();
            }
            catch (WebException webException)
            {
                throw new Exception(webException.Message);
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
            finally
            {
                // Release connections
                if (requestStream != null)
                {
                    requestStream.Close();
                }

                if (responseStream != null)
                {
                    responseStream.Close();
                }

                if (httpWebResponse != null)
                {
                    httpWebResponse.Close();
                }
            }

            // Return API Response
            return xmlResponse;
        }
        public static Token Autenticar(string cpfCertificado)
        {
            var token = new Token();

            using (var handler = new WebRequestHandler())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                handler.ClientCertificates.Add(ObterCertificado(cpfCertificado));
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Add("Role-Type", PerfilSiscomex);

                    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(UrlSiscomex + "/portal/api/autenticar"));
                    var response = client.SendAsync(request).Result;
                    //Task.Delay(1000);
                    response.EnsureSuccessStatusCode();

                    IEnumerable<string> valor;

                    if (response.Headers.TryGetValues("set-token", out valor))
                        token.SetToken = valor.FirstOrDefault();

                    if (response.Headers.TryGetValues("x-csrf-token", out valor))
                        token.CsrfToken = valor.FirstOrDefault();

                    if (response.Headers.TryGetValues("x-csrf-expiration", out valor))
                        token.CsrfExpiration = valor.FirstOrDefault();

                    return token;
                }
            }
        }

        public static Token ObterToken(string cpfCertificado)
        {
            var token = new Token();
            try
            {
                token = Autenticar(cpfCertificado);
            }
            catch (Exception)
            {
                try
                {
                    token = Autenticar(cpfCertificado);
                }
                catch (Exception)
                {
                    token = Autenticar(cpfCertificado);
                }
            }

            return token;
        }
        public static HttpResponseMessage CriarRequestKleiton(string url, IDictionary<string, string> headers, string xml, string certificado)
        {
            using (var handler = new WebRequestHandler())
            {
                handler.ClientCertificates.Add(ObterCertificado(certificado));
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

                using (var client = new HttpClient(handler))
                {

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);

                    xml = xml.Replace("\r", string.Empty)/*.Replace(" ", "")*/;

                    using (var stringContent = new StringContent(xml, Encoding.UTF8, "application/xml"))
                    {
                        return client.PostAsync(new Uri(UrlSiscomex + url), stringContent).Result;
                    }
                }
            }
        }
        public static string CriarRequestGet(string url, IDictionary<string, string> headers, string certificado)
        {
            using (var handler = new WebRequestHandler())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                handler.ClientCertificates.Add(ObterCertificado(certificado));
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

                using (var client = new HttpClient(handler))
                {
                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);

                    try
                    {
                        var retorno = client.GetAsync(new Uri(UrlSiscomex + url)).Result;

                        if (retorno.IsSuccessStatusCode)
                        {
                            return retorno.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            var msg = retorno.Content.ReadAsStringAsync().Result;

                            if (!string.IsNullOrEmpty(msg))
                            {
                                try
                                {
                                    var jsonObj = JsonConvert.DeserializeObject<ErroSiscomexResponse>(msg);

                                    if (jsonObj != null)
                                    {
                                        return jsonObj.message;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (retorno.StatusCode == HttpStatusCode.NotFound)
                                        return "DUE não encontrada!";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    return string.Empty;
                }
            }
        }

        public static IEnumerable<X509Certificate2> ListarCertificadosInstalados(StoreLocation storeLocation)
        {
            var stores = new X509Store(StoreName.My, storeLocation);

            stores.Open(OpenFlags.ReadOnly);

            var certificadosInstalados = stores.Certificates;

            certificadosInstalados.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
            certificadosInstalados.Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, false);

            stores.Close();

            var certificados = new List<X509Certificate2>();

            foreach (X509Certificate2 certificado in certificadosInstalados)
                yield return certificado;
        }

        public static X509Certificate2 ObterCertificado(string cpf)
        {
            var certificado = ListarCertificadosInstalados(StoreLocation.LocalMachine)
                .FirstOrDefault(a => a.SubjectName.Name.Contains(cpf));

            if (certificado == null)
            {
                certificado = ListarCertificadosInstalados(StoreLocation.CurrentUser)
                    .FirstOrDefault(a => a.SubjectName.Name.Contains(cpf));
            }

            if (certificado == null)
                throw new Exception($"Certificado Digital de CPF {cpf} não encontrado");

            return certificado;
        }

        public static Dictionary<string, string> ObterHeaders(Token token) => new Dictionary<string, string>
        {
            {"Authorization", token.SetToken},
            {"x-csrf-token", token.CsrfToken}
        };
        public static HttpStatusCode PostWithXmlData(string cpfCertificado)
        {
            string rawXml = "<recepcaoNFE>" +
                                "< identificacaoRecepcao > REC001 </ identificacaoRecepcao >" +
                                "< cnpjResp > 15573459000106 </ cnpjResp >" +
                                "< local >" +
                                "< codigoURF > 0817600 </ codigoURF >" +
                                "< codigoRA > 8911101 </ codigoRA >" +
                                "</ local >" +
                                "< referenciaLocalRecepcao > referenciaLocalRecepcao </ referenciaLocalRecepcao >" +
                                "< notasFiscais >" +
                                    "< notaFiscalEletronica >" +
                                    "< chaveAcesso > 20161016175341723460934170526686662814689781 </ chaveAcesso >" +
                                    "</ notaFiscalEletronica >" +
                                "</ notasFiscais >" +
                                "< transportador >" +
                                    "< cnpj > 00000000000272 </ cnpj >" +
                                    "< cpfCondutor > 10715312707 </ cpfCondutor >" +
                                    "</ transportador >" +
                                    "< pesoAferido > 100.000 </ pesoAferido >" +
                                    "< localArmazenamento > localArmazenamento </ localArmazenamento >" +
                                    "< codigoIdentCarga > CARGA 0001 TESTE </ codigoIdentCarga >" +

                                    "< avariasIdentificadas > avarias Identificadas </ avariasIdentificadas >" +

                                    "< divergenciasIdentificadas > divergencias Identificadas </ divergenciasIdentificadas >" +

                                    "< observacoesGerais > observacoes Gerais </ observacoesGerais >" +

                            "</ recepcaoNFE >" +
                            "</ recepcoesNFE > ";

            var client = new RestClient("https://val.portalunico.siscomex.gov.br");

            //string baseUrl = "https://val.portalunico.siscomex.gov.br";
            //client.BaseUrl = new Uri("https://val.portalunico.siscomex.gov.br");
            Token token = null;
            if (HttpContext.Current.Session["TOKEN"] == null)
            {
                token = ObterToken(cpfCertificado);
            }
            else
            {
                token = (Token)HttpContext.Current.Session["TOKEN"];
            }
            //var headers = ObterHeaders(token);
            var request = new RestRequest("/cct/api/ext/carga/recepcao-nfe", Method.POST);

            request.AddParameter("Authorization", token.SetToken, ParameterType.HttpHeader);
            request.AddHeader("X-CSRF-Token", token.CsrfExpiration);
            //request.AddParameter("X-CSRF-Token", token.CsrfToken, ParameterType.HttpHeader);
            
            request.AddParameter("application/xml", rawXml, ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            return response.StatusCode;
            
        }
        public static string postXMLData(string destinationUrl, string requestXml, string cpfCertificado)
        {
            Token token = null;
            if (HttpContext.Current.Session["TOKEN"] == null)
            {
                token = ObterToken(cpfCertificado);
            }
            else
            {
                token = (Token)HttpContext.Current.Session["TOKEN"];
            }
            //var headers = ObterHeaders(token);
            var headers = new WebHeaderCollection();
            headers.Add("Authorization", token.SetToken);
            headers.Add("x-csrf-token", token.CsrfToken);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
            byte[] bytes;
            bytes = Encoding.ASCII.GetBytes(requestXml);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.Headers = headers;
            //request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
           // requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                return responseStr;
            }
            return null;
        }
        public static HttpStatusCode EnviarXmlCct(string cpfCertificado)
        {
            string xml = "<recepcaoNFE>" +
                                "< identificacaoRecepcao > REC001 </ identificacaoRecepcao >" +
                                "< cnpjResp > 15573459000106 </ cnpjResp >" +
                                "< local >" +
                                "< codigoURF > 0817600 </ codigoURF >" +
                                "< codigoRA > 8911101 </ codigoRA >" +
                                "</ local >" +
                                "< referenciaLocalRecepcao > referenciaLocalRecepcao </ referenciaLocalRecepcao >" +
                                "< notasFiscais >" +
                                    "< notaFiscalEletronica >" +
                                    "< chaveAcesso > 20161016175341723460934170526686662814689781 </ chaveAcesso >" +
                                    "</ notaFiscalEletronica >" +
                                "</ notasFiscais >" +
                                "< transportador >" +
                                    "< cnpj > 00000000000272 </ cnpj >" +
                                    "< cpfCondutor > 10715312707 </ cpfCondutor >" +
                                    "</ transportador >" +
                                    "< pesoAferido > 100.000 </ pesoAferido >" +
                                    "< localArmazenamento > localArmazenamento </ localArmazenamento >" +
                                    "< codigoIdentCarga > CARGA 0001 TESTE </ codigoIdentCarga >" +

                                    "< avariasIdentificadas > avarias Identificadas </ avariasIdentificadas >" +

                                    "< divergenciasIdentificadas > divergencias Identificadas </ divergenciasIdentificadas >" +

                                    "< observacoesGerais > observacoes Gerais </ observacoesGerais >" +

                            "</ recepcaoNFE >" +
                            "</ recepcoesNFE > ";
            Token token = null;
            
            if (HttpContext.Current.Session["TOKEN"] == null)
            {
                token = ObterToken(cpfCertificado);
            }
            else
            {
                token = (Token)HttpContext.Current.Session["TOKEN"];
            }

            var xmlRetorno = string.Empty;

            var headers = ObterHeaders(token);

            var response = CriarRequestPost(string.Format("https://val.portalunico.siscomex.gov.br/cct/api/ext/carga/recepcao-nfe"), headers, xml, cpfCertificado);

            if (response == null)
                return response.StatusCode;

            return response.StatusCode;

           // xmlRetorno = response.Content.ReadAsStringAsync().Result;

            //Log.GravarLog($"Registro id: {idXml} - cpf: {Parametros.CPFCertificado} - Statuscode: {response.StatusCode.ToString()}");

            //if (response.StatusCode != HttpStatusCode.OK)
            //    SiscomexDAO.GravarInconsistencia(idXml, xmlRetorno);

            //if (response.StatusCode == HttpStatusCode.OK)
            //{
            //    var retorno = XmlHelper.Deserializar(response.Content.ReadAsStringAsync().Result);

            //    if (retorno.Operacao.Mensagens.Count > 0)
            //        SiscomexDAO.GravarRegistroEnviado(idXml);
            //}
        }
        //public static HttpStatusCode EnviarXmlCct(string cpfCertificado)
        //{
        //    Token token = null;
        //    if (HttpContext.Current.Session["TOKEN"] == null)
        //    {
        //        token = ObterToken(cpfCertificado);
        //    }
        //    else
        //    {
        //        token = (Token)HttpContext.Current.Session["TOKEN"];
        //    }
        //    if (token != null)
        //    {
        //        HttpContext.Current.Session["TOKEN"] = token;

        //        var headers = ObterHeaders(token);

        //        var xml = "<nfe>NFE</nfe>";

        //        var response = CriarRequest(string.Format("/cct/api/ext/carga/recepcao-nfe", xml), headers, cpfCertificado);
        //        if (!string.IsNullOrEmpty(response))
        //        {
                    
        //        }

        //    }

        //    return HttpStatusCode.OK;
        //}
        public static DadosNotaPreACD ConsultarDadosNotaPreACD(string chave, int item, string cpfCertificado)
        {
            Token token = null;

            if (HttpContext.Current.Session["TOKEN"] == null)
            {
                token = ObterToken(cpfCertificado);
            }
            else
            {
                token = (Token)HttpContext.Current.Session["TOKEN"];
            }

            if (token != null)
            {
                HttpContext.Current.Session["TOKEN"] = token;

                var headers = ObterHeaders(token);

                var response = CriarRequestGet(string.Format("/cct/api/ext/deposito-carga/estoque-nota-fiscal/{0}", chave), headers, cpfCertificado);

                if (!string.IsNullOrEmpty(response))
                {
                    var dadosNota = JsonConvert.DeserializeObject<DadosNotaPreACD>(response);

                    if (dadosNota != null)
                    {
                        var nota = dadosNota.estoqueNotasFiscais
                            .SelectMany(c => c.itens
                            .Where(d => d.item == item))
                            .FirstOrDefault();

                        if (dadosNota.estoqueNotasFiscais.Count == 0)
                        {
                            return new DadosNotaPreACD
                            {
                                Sucesso = false,
                                Mensagem = dadosNota.ObterMensagem()
                            };
                        }

                        return new DadosNotaPreACD
                        {
                            Sucesso = true,
                            Saldo = nota?.saldo ?? 0,
                            Recinto = dadosNota.Recinto
                        };
                    }
                }
            }

            return null;
        }


        public static ConsultaDueDadosResumidos ObterDetalhesDUE(string due, string cpfCertificado)
        {
            var token = ObterToken(cpfCertificado);

            if (token != null)
            {
                var headers = ObterHeaders(token);

                var response = CriarRequestGet(string.Format("/due/api/ext/due/consultarDadosResumidosDUE?numero={0}", due), headers, cpfCertificado);

                if (!string.IsNullOrEmpty(response))
                {
                    ConsultaDueDadosResumidos dadosDUE = new ConsultaDueDadosResumidos();

                    try
                    {
                        dadosDUE = JsonConvert.DeserializeObject<ConsultaDueDadosResumidos>(response);

                        if (dadosDUE != null)
                        {
                            var obj = new ConsultaDueDadosResumidos
                            {
                                Sucesso = true,
                                numeroDUE = due,
                                situacaoDUE = dadosDUE.situacaoDUE,
                                Mensagem = ""
                            };

                            DateTime result;

                            if (DateTime.TryParse(dadosDUE.dataSituacaoDUE, out result))
                            {
                                obj.dataSituacaoDUE = result.ToString("dd/MM/yyyy HH:mm");
                            }
                            else
                            {
                                obj.dataSituacaoDUE = dadosDUE.dataSituacaoDUE;
                            }

                            return obj;
                        }
                        else
                        {
                            return new ConsultaDueDadosResumidos
                            {
                                Sucesso = false,
                                numeroDUE = due,
                                Mensagem = "DUE não encontrada (Siscomex)"
                            };
                        }
                    }
                    catch (Exception)
                    {
                        return new ConsultaDueDadosResumidos
                        {
                            Sucesso = false,
                            numeroDUE = due,
                            Mensagem = "Portal Microled: Falha ao obter os dados da DUE - Detalhes: " + response
                        };
                    }
                }
            }

            return new ConsultaDueDadosResumidos
            {
                Sucesso = false,
                numeroDUE = due
            };
        }

        public static DueDadosCompletos ObterDUECompleta(string due, string cpfCertificado)
        {
            var token = ObterToken(cpfCertificado);

            if (token != null)
            {
                var headers = ObterHeaders(token);

                var response = CriarRequestGet(string.Format("/due/api/ext/due/numero-da-due/{0}", due), headers, cpfCertificado);

                if (!string.IsNullOrEmpty(response))
                {
                    if (response.ToLower().Contains("cpf logado não representa o cnpj do declarante") || response.Contains("DUE não encontrada!"))
                    {
                        var objErro = new DueDadosCompletos();
                        objErro.Sucesso = false;
                        objErro.Mensagem = response;

                        return objErro;
                    }

                    var objCons = JsonConvert.DeserializeObject<DueDadosCompletos>(response);

                    objCons.Sucesso = true;

                    return objCons;
                }
            }

            return null;
        }
        private static HttpResponseMessage CriarRequestPost(string url, IDictionary<string, string> headers, string xml, string cpfCertificado)
        {
            using (var handler = new WebRequestHandler())
            {
                handler.ClientCertificates.Add(ObterCertificado(cpfCertificado));
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

                using (var client = new HttpClient(handler))
                {

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);

                    //xml = xml.Replace("\r\n", string.Empty);

                    using (var stringContent = new StringContent(xml, Encoding.UTF8, "application/xml"))
                    {
                        return client.PostAsync(new Uri(url), stringContent).Result;
                    }
                }
               
            }
        }
        public static async Task<HttpResponseMessage> CriarRequest(string url, IDictionary<string, string> headers, string xml, string due, string cpfCertificado)
        {
            using (var handler = new WebRequestHandler())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                handler.ClientCertificates.Add(ObterCertificado(cpfCertificado));
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = new TimeSpan(0, 15, 00);

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                    foreach (var header in headers)
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);

                    client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
                    client.DefaultRequestHeaders.Add("Connection", "Keep-Alive");

                    xml = xml.Replace("\r\n", string.Empty);
                    xml = System.Text.RegularExpressions.Regex.Replace(xml, @"\t|\n|\r", "");
                    xml = xml.Replace("      ", " ");
                    xml = xml.Replace("    ", " ");
                    xml = xml.Replace("   ", " ");
                    xml = xml.Replace("  ", " ");
                    xml = xml.Replace(Environment.NewLine, "");

                    using (var stringContent = new StringContent(xml, Encoding.UTF8, "application/xml"))
                    {
                        if (!string.IsNullOrEmpty(due))
                        {
                            return await client.PutAsync(new Uri(string.Concat(UrlSiscomex, url)), stringContent);
                        }
                        else
                        {
                            return await client.PostAsync(new Uri(string.Concat(UrlSiscomex, url)), stringContent);
                        }
                    }
                }
            }
        }
    }

    public class Token
    {
        public string SetToken { get; set; }

        public string CsrfToken { get; set; }

        public string CsrfExpiration { get; set; }

        public bool Valido() => SetToken?.Length > 0 && CsrfToken?.Length > 0
                && TimeSpan.FromMilliseconds(Convert.ToDouble(CsrfExpiration ?? "0")).TotalMinutes > 0;
    }
}