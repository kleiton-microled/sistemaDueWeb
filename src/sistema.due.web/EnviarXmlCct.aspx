<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EnviarXmlCct.aspx.cs" Inherits="Cargill.DUE.Web.EnviarXmlCct" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CSS" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <asp:FileUpload ID="FileUpload1" runat="server" />
    
    <asp:Button ID="btnValidaToken" runat="server" OnClick="btnValidaToken_Click" Text="UPLOAD CSV" Width="178px" />
    
    <asp:Label ID="lblResult" runat="server" Text="Label"></asp:Label>
    
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
</asp:Content>
