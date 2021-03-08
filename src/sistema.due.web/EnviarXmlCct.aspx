<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EnviarXmlCct.aspx.cs" Inherits="Cargill.DUE.Web.EnviarXmlCct" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CSS" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="panel panel-primary">
                                <div class="panel-heading">
                                    <h3 class="panel-title">Envios de nota para CCT. ATENÇÃO: As notas serão enviadas diretamente para o CCT.<asp:Label ID="lblResult" CssClass="alert alert-info" runat="server" Text="Label" Visible="False"></asp:Label>
                                    </h3>
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
                                                <asp:Button ID="btnImport" runat="server" OnClick="btnValidaToken_Click" Text="IMPORTAR" Width="136px" />
                                            </div>
                                        </div>
                                    </div>
                                    <div id="result" style="margin-top:50px; border:1px dashed #cccccc">
                                            <asp:GridView ID="GridView1" runat="server" CssClass="table table-light" BorderWidth="0" OnRowDataBound="GridView1_RowDataBound">     
                                            </asp:GridView>
                                    </div>
                              </div>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
    <asp:Label ID="lblContentResult" runat="server" Text="Label" Visible="False"></asp:Label>
</asp:Content>
