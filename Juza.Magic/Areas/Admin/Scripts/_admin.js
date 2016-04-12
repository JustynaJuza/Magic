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
        processing: true,
        serverSide: true,
        deferRender: true,
        ajax: {
            url: window.basePath + '/Admin/Cards/GetCardData',
            type: 'POST',
            //dataSrc : ''
        }, //"scripts/server_processing.php",
        columns: [
            { name: 'SetId' },
            { name: 'Name' },
            { name: 'Rarity' },
            { name: 'Types' },
            { name: 'Mana' },
            { name: 'Colors' },
            { name: 'Controls' }
            
            //{ name: 'SetId', data: 'SetId' },
            //{ name: 'Name', data: 'Name' },
            //{ name: 'Rarity', data: 'Rarity' },
            //{ name: 'Types', data: 'Types' },
            //{ name: 'Mana', data: 'Mana' },
            //{ name: 'Colors', data: 'Colors' },
            //{ name: 'Controls', data: 'Controls' }
        ],
        columnDefs: [
            {
                targets : [-1],
                searchable : false,
                sortable : false
            }
        ],
        drawCallback: function () {
            //if ($('#cms_listing').find('tr').length <= $('#cms_listing_length option:selected').val() + 1) {
            //    $('#cms_listing_length').hide();
            //    $('#cms_listing_paginate').hide();
            //} else {
            //    $('#cms_listing_length').show();
            //    $('#cms_listing_paginate').show();
            //}
        },
        dom: 'T<"clear">lfrtip',
        //tableTools: {
        //    //sSwfPath: '/swf/copy_csv_xls_pdf.swf',
        //    aButtons: [
        //        {
        //            sExtends: 'copy',
        //            sButtonText: 'Edit',
        //        },
        //        {
        //            sExtends: 'text',
        //            sButtonText: 'Delete',
        //            fnClick: function (nButton, oConfig, oFlash) {
        //                console.log(nButton, oConfig, oFlash);

        //            }
        //        },
        //        {
        //            sExtends: 'collection',
        //            sButtonText: 'Export',
        //            aButtons: ['xls', 'csv', 'pdf']
        //        },
        //        'print',
        //        'select_all'
        //    ],
        //    sRowSelect: 'multi'
        //}
    });
});