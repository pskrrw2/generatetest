if (typeof ConferencePage === 'undefined') {
    var ConferencePage = (function () {

        let $searchInput = $('#ConfernceTable');

        let init = function () {

            if (!$.fn.DataTable.isDataTable($searchInput)) {
                let datatable = $searchInput.DataTable({
                    dom:
                        '<"card-header d-flex flex-column flex-md-row align-items-start align-items-md-center pb-md-0 pt-0"<f><"d-flex align-items-md-center justify-content-md-end gap-4"l<"dt-action-buttons"B>>' +
                        '>t' +
                        '<"row mx-1"' +
                        '<"col-sm-12 col-md-6"i>' +
                        '<"col-sm-12 col-md-6"p>' +
                        '>',
                    lengthMenu: [10, 40, 60, 80, 100], // Page length options
                    columnDefs: [
                        {
                            // First column (S. No.)
                            targets: 0,
                            searchable: false,
                            orderable: false,
                            responsivePriority: 1,
                            render: function (data, type, full, meta) {
                                // Show row numbers in desktop view
                                if (type === 'display' && $(window).width() >= 768) {
                                    return meta.row + 1;
                                }
                                // Return empty string for mobile view (to show control icon)
                                return '';
                            },
                            className: 'control', // Apply control class for mobile view
                        }],
                    responsive: {
                        details: {
                            display: $.fn.dataTable.Responsive.display.modal({
                                header: function (row) {
                                    return 'Details of Request';
                                }
                            }),
                            type: 'column',
                            renderer: function (api, rowIdx, columns) {
                                var data = $.map(columns, function (col, i) {
                                    return col.title !== ''
                                        ? '<tr data-dt-row="' + col.rowIndex + '" data-dt-column="' + col.columnIndex + '">' +
                                        '<td>' + col.title + ':' + '</td> ' +
                                        '<td>' + col.data + '</td></tr>' : '';
                                }).join('');

                                return data ? $('<table class="table"/><tbody />').append(data) : false;
                            }
                        }
                    },
                    language: {
                        sLengthMenu: '_MENU_',
                        search: '',
                        searchPlaceholder: 'Search Conference',
                        info: 'Displaying _START_ to _END_ of _TOTAL_ entries',
                        paginate: {
                            next: '<i class="ri-arrow-right-s-line"></i>',
                            previous: '<i class="ri-arrow-left-s-line"></i>'
                        }
                    },
                    buttons: [
                        {
                            extend: 'collection',
                            text: '<i class="ri-download-line me-1"></i> Export',
                            className: 'btn btn-outline-secondary dropdown-toggle',
                            buttons: [{ extend: 'print', text: '<i class="ri-printer-line me-1"></i> Print', className: 'dropdown-item', exportOptions: { columns: ':visible' } },
                            { extend: 'csv', text: '<i class="ri-file-text-line me-1"></i> CSV', className: 'dropdown-item', exportOptions: { columns: ':visible' } },
                            { extend: 'excel', text: '<i class="ri-file-excel-line me-1"></i> Excel', className: 'dropdown-item', exportOptions: { columns: ':visible' } },
                            { extend: 'pdf', text: '<i class="ri-file-pdf-line me-1"></i> PDF', className: 'dropdown-item', exportOptions: { columns: ':visible' } },
                            { extend: 'copy', text: '<i class="ri-file-copy-line me-1"></i> Copy', className: 'dropdown-item', exportOptions: { columns: ':visible' } }
                            ]
                        }
                    ]
                });
            };
        };

        return {
            init: init
        };
    })();
}