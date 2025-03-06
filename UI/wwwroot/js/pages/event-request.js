if (typeof EventPage === 'undefined') {
    var EventPage = (function () {

        let $hdnEventId = $('.jsHdnEventId');
        let $changeImage = $('.jsChangeImage');
        let $inputFile = $('.jsInputFile');
        let $inputFileSpan = $('.jsInputFileSpan');
        let $searchInput = $('#matchEventTable');
        let $deleteButton = $('.jsDeleteEvent');
        let $isActiveEvent = $('.jsIsActiveEvent');

        let init = function () {

            if ($hdnEventId.val() != 0) {
                $("#user_img").css("display", "block");
            }

            $changeImage.on('change', (e) => {
                changeImage(e);
            });

            $inputFileSpan.on('click', (e) => {
                $changeImage.trigger('click');
            });

            $inputFile.on('click', (e) => {
                $changeImage.trigger('click');
            });

            $deleteButton.on('click', (e) => {
                deletePage(e);
            });

            $isActiveEvent.on('change', (e) => {
                isActiveChange(e);
            });

        };

        let deletePage = (e) => {
            if (confirm('Are you sure you want to delete this item?')) {
                let deletePagePrivate = () => {
                    $.ajax({
                        url: e.currentTarget.dataset.action,
                        type: 'post',
                        data: { '__RequestVerificationToken': $("[name='__RequestVerificationToken']").val() },
                        success: function (result, textStatus, jqXHR) {
                            if (textStatus === 'success') {
                                location.reload();
                            } else {
                                console.error('An error occurred: ', result.errors);
                            }
                        },
                        error: function (result) {
                            if (result.status === 403) {
                                console.error('Permission denied: ', result.errors);
                            } else {
                                console.error('An error occurred: ', result.errors);
                            }
                        }
                    });
                };

                deletePagePrivate();
            }
        };

        let isActiveChange = (e) => {
            e.preventDefault();
            Site.showSpinner();
            let isActive = e.target.checked;
            let eventId = e.target.dataset.id;
            $.ajax({
                url: e.target.dataset.action,
                type: 'post',
                data: {
                    '__RequestVerificationToken': $("[name='__RequestVerificationToken']").val(),
                    'eventId': eventId,
                    'isActive': isActive
                },
                success: function (result, textStatus, jqXHR) {
                    if (textStatus === 'success')
                        Site.successMessage('Event is Updated!');
                    else
                        Site.errorMessage(errors, 'An error occurred!');

                    Site.hideSpinner();
                },
                error: function (result) {
                    Site.hideSpinner();
                    Site.errorMessage(errors, 'An error occurred!');
                }

            });
        };


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
                            if (type === 'display' && $(window).width() >= 768) {
                                return meta.row + 1;
                            }
                            return '';
                        },
                        className: 'control',
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
                    searchPlaceholder: 'Search Event',
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

        let changeImage = function (e) {
            let $img = $("#user_img");

            if (!(e.target.files && e.target.files[0])) {
                $inputFile.val(null);
                $img.css('display', 'none');
                $img.attr('src', null);
                $("#CreateEditForm").validate().element("#text_input_id");
                return;
            }

            let allowedTypes = $changeImage.attr('accept');
            let allowedTypeArr = allowedTypes.split(',');
            let fileName = e.target.files[0].name;
            let extension = '.' + fileName.split('.')[1];
            if (!allowedTypeArr.includes(extension)) {
                let errorArray = {};
                errorArray["EventModels.Image"] = 'Only image files are allowed: ' + allowedTypes;
                $('#CreateEditForm').validate().showErrors(errorArray);
                return;
            }

            $inputFile.val(e.target.files[0].name);
            let reader = new FileReader();
            reader.onload = function (e) {
                $img.css('display', 'block');
                $img.attr('src', e.target.result);
            };
            reader.readAsDataURL(e.target.files[0]);
        };

        return {
            init: init
        };
    })();
}