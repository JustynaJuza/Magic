<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/CMS.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Autographer.Models.LandingPages.LandingPage>>" %>
<%@ Import Namespace="TheTin.Helpers.RenderViewToString" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Landing Pages Manager
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Landing Pages Manager</h1>
    <% Html.RenderPartial("DisplayMessageAndError"); %>

    <%: Html.ActionLink("Add new entry", "Create", null, new { @class = "bigBtn blueLink"})%>

    <div class="searchResultsPanel">
        <% if (Model.Count() == 0) { %>

            <p><em>No Matching Results Found</em></p>

        <% }  else { %>
        
            <table id="searchResults" style="width:100%">
                <thead>
                    <tr>
                        <th>Published</th>

                        <th>Name</th>

                        <th>Shortlink</th>

                        <th></th>
                    </tr>
                </thead>
                <tbody>
                <% foreach (Autographer.Models.LandingPages.LandingPage item in Model)
                   { %>
                    <tr>
                        <td style="width:25%;"><%: Html.DisplayFor(m => item.Published) %></td>

                        <td style="width:25%"><%: Html.DisplayFor(m => item.Name)%></td>

                        <td style="width:25%"><%: Html.DisplayFor(m => item.ShortUrl)%></td>

                        <td style="max-width:310px;min-width:310px"><% Html.RenderPartial("_TableControlsPartial", new Tuple<string, int, bool>("LandingPages", item.Id, item.Published.HasValue)); %></td>
                    </tr>
                <%  } %>
                </tbody>
            </table>
        <%  } %>
    </div>

    <script src="../../../../Scripts/DataTables-1.9.0/extras/ColReorderWithResize.js"></script>
    <script>
        $(function () {
            var table = $('#searchResults').dataTable({
                'bJQueryUI': true,
                "bPaginate": false,
                "bLengthChange": false,
                "bFilter": true,
                "bInfo": true,
                "bAutoWidth": false,
                //"sDom": "Rlfrtip",
                "aoColumnDefs": [
                    {
                        "aTargets": [-1],
                        "bSearchable": false,
                        "bSortable": false
                    }
                ]
            });

            //new FixedHeader(table);
        });
    </script>
</asp:Content>

