$(function () {
    window.admin = $.connection.adminHub;
    var cardsProcessed;

    if ($.connection.hub && $.connection.hub.state == 4) {
        $.connection.hub.start().done(function () {
            $('#fetch-set-submit').on('click', function () {
                window.admin.server.fetchSetWithCards($('#fetch-set-name').val());
                $('#fetch-set-info').prepend('Request sent...<br />');
                cardsProcessed = 0;
            });
        });
    }

    window.admin.client.updateCardsInSet = function (cardsInSet) {
        $('#fetch-set-info').append('Found set composed of ' + cardsInSet + ' cards.<br />');
    }

    window.admin.client.updateCardsTotal = function(cardsTotal) {
        $('#info-cards').show();
        $('#info-cards-total').text(cardsTotal);
    }

    window.admin.client.updateCardsProcessed = function () {
        cardsProcessed += 1;
        $('#info-cards-processed').text(cardsProcessed);
    }

    window.admin.client.updateRequestProgress = function (message) {
        $('#fetch-set-info').append(message + '<br />');
    }

    $('.dataTable').DataTable({
        aoColumnDefs: [
            {
                "aTargets": [-1],
                "bSearchable": false,
                "bSortable": false,
                "processing": true,
                "serverSide": true,
            }
        ],
        fnDrawCallback: function () {
            //if ($('#cms_listing').find('tr').length <= $('#cms_listing_length option:selected').val() + 1) {
            //    $('#cms_listing_length').hide();
            //    $('#cms_listing_paginate').hide();
            //} else {
            //    $('#cms_listing_length').show();
            //    $('#cms_listing_paginate').show();
            //}
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