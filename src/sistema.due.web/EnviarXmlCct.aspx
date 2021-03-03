<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EnviarXmlCct.aspx.cs" Inherits="Cargill.DUE.Web.EnviarXmlCct" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CSS" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <asp:FileUpload ID="FileUpload1" runat="server" />
    
    <asp:Button ID="btnValidaToken" runat="server" OnClick="btnValidaToken_Click" Text="UPLOAD CSV" Width="178px" />
    
    <asp:Label ID="lblResult" runat="server" Text="Label"></asp:Label>
    
    <div class="panel panel-primary">
                                <div class="panel-heading">
                                    <h3 class="panel-title">Pesquisa notas no banco de dados interno. ATENÇÃO: Consulta rápida, somente as notas não encontradas serão pesquisadas diretamente no CCT.</h3>
                                </div>
                                <div class="panel-body">
                                    <div class="row">
                                        <div class="form-group">
                                            <div class="col-md-10">
                                                <label for="txtValorTotalNota" class="control-label">Arquivo:</label>
                                                <asp:FileUpload ID="txtUpload" ClientIDMode="Static" runat="server" CssClass="btn btn-default" />
                                                <p class="help-block">
                                                    Clique no botão "Escolher arquivo" e selecione o arquivo .csv ou .txt de notas fiscais
                                                </p>
                                            </div>
                                            <div class="col-md-2">
                                                <div class="pull-right">
                                                    <a href="Consulta_CCT.pdf" target="_blank">
                                                        <img src="Content/imagens/btnInstrucoes.png" /></a>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    <br />
                                    <div class="row">
                                        <div class="form-group">
                                            <div class="col-md-6">
                                                <asp:Button runat="server" Text="Sair" CssClass="btn btn-default"  />
                                                <asp:Button Visible="false" runat="server" Text="Importar" CssClass="btn btn-warning" />
                                                <input type='button' value='Importar' class="btn btn-warning" >
                                            </div>

                                        </div>
                                    </div>
                              </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
</asp:Content>
