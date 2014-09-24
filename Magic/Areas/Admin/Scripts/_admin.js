$(function () {
    window.admin = $.connection.adminHub;

    if ($.connection.hub && $.connection.hub.state == 4) {
        $.connection.hub.start().done(function () {
            $('#fetch-set-submit').on('click', function () {
                window.admin.server.makeSetRequest($('#fetch-set-name').val(), true);
                $('#fetch-set-info').append('Request sent...<br />');
            });
        });
    }

    window.admin.client.updateRequestProgress = function (message) {
        $('#fetch-set-info').append(message + '<br />');
    }

    $(document).ready(function () {
        $('.dataTable').DataTable({
            aoColumnDefs: [
                {
                    "aTargets": [-1],
                    "bSearchable": false,
                    "bSortable": false
                }
            ],
            fnDrawCallback: function () {
                if ($('#cms_listing').find('tr').length <= $('#cms_listing_length option:selected').val() + 1) {
                    $('#cms_listing_length').hide();
                    $('#cms_listing_paginate').hide();
                } else {
                    $('#cms_listing_length').show();
                    $('#cms_listing_paginate').show();
                }
            },
            dom: 'T<"clear">lfrtip',
            tableTools: {
                //sSwfPath: '/swf/copy_csv_xls_pdf.swf',
                aButtons: [
                    {
                        sExtends: 'copy',
                        sButtonText: 'Edit',
                    },
                    {
                        sExtends: 'text',
                        sButtonText: 'Delete',
                        fnClick: function (nButton, oConfig, oFlash) {
                            console.log(nButton, oConfig, oFlash);

                        }
                    },
                    {
                        sExtends: 'collection',
                        sButtonText: 'Export',
                        aButtons: ['xls', 'csv', 'pdf']
                    },
                    'print',
                    'select_all'
                ],
                sRowSelect: 'multi'
            }
        });
    });
});