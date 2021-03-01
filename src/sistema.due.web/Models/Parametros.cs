using System;

namespace Sistema.DUE.Web.Models
{
    public class Parametros
    {
        /// <summary>
        /// Parâmetros de sistema
        /// </summary>
        public string MensagemEmailRedefinicaoSenha { get; set; }

        public int ValidarAtributosCafe { get; set; }

        public int EnvioRetificacaoSemServico { get; set; }
        public static Uri Url { get; set; }
        public static string Perfil { get; set; }
        public static string MaxTentativas { get; set; }
        public static string CPFCertificado { get; set; }
       //public static string DiretorioLogs => Environment.CurrentDirectory + @"\logs";
    }
}