<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/CMS.Master" Inherits="System.Web.Mvc.ViewPage<Autographer.Models.LandingPages.LandingPage>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Delete <%: Model.GetType().Name %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% var objectTypeName = Model.GetType().Name; %>
<h2>Delete entry</h2>
    
    <% Html.RenderPartial("DisplayMessageAndError"); %>

<p><em>The following item will be irreversibly removed from the database. <br>
    Use the 'Unpublish' option instead if you only want to hide it.</em></p>

    <fieldset>
        <legend><%: objectTypeName %> #<%: Html.DisplayFor(m => m.Id)%></legend>
        <div class="editor-label">
            <%: Html.LabelFor(m => m.Name) %>
        </div>
        <div class="editor-field">
            <%: Html.DisplayFor(m => m.Name)%>
        </div>
    </fieldset>


    <% using (Html.BeginForm("Delete", "LandingPages", new { id = Model.Id })) { %>
    <p>
        <input type="submit" id="delete" name="delete" value="Delete" class="hugeBtn blueButton" />
        <%: Html.ActionLink("Cancel", "Index", null, new { @class = "cancelBtnLarge" })%>
    </p>
    <% } %>
</asp:Content>