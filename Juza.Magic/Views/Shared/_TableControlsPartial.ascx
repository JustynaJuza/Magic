<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Tuple<string, int, bool>>" %>

<%: Html.ActionLink("View", "Details", Model.Item1, new { Area = "", id = Model.Item2 }, new { @class="btn btn-info", target="_blank"})%>

<%: Html.ActionLink("Edit", "Edit", new { id = Model.Item2 }, new { @class="btn btn-warning" })%>

<%: Html.ActionLink("Delete", "PreviewDelete", new { id = Model.Item2 }, new { @class="btn btn-danger" })%> 

<% if (Model.Item3) { %>
    <%: Html.ActionLink("Unpublish", "Publish", new { id = Model.Item2, publish = false }, new { @class="btn btn-inverse" }) %>
<% } else { %> 
    <%: Html.ActionLink("Publish", "Publish", new { id = Model.Item2 }, new { @class="btn btn-inverse" }) %>
<% } %>