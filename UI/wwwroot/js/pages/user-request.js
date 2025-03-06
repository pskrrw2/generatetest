if (typeof UserPage === 'undefined') {
    var UserPage = (function () {

        let $UserTable = $('#UserTable');
        let $isActiveExecutive = $('.jsIsActiveExecutive');
        let $jsBtnSendOtpMail = $('.jsBtnSendOtpMail');
        let $jsFillOtpTxt = $('.jsFillOtpTxt');
        let $jsEmailTxtbox = $('.jsEmailTxtbox');
        let $jsVerifyButton = $('.jsVerifyButton');
        let $jsTimeCountDown = $('.jsTimeCountDown');
        let $loginForm = $('#loginForm');
        let $OtpAuthentication = $('#OtpAuthentication');
        let $jsResendOtp = $('#jsResendOtp');
        let $errorMessage = $('#errorMessage');
        let $hdnMailInput = $('#OtpAuthentication input[name="Input.Email"]');
        let $hideShowResend = $('.hideShowResend');
        var countdownDuration = 300;
        var timerRunning = false;
        var countdownTimer;

        let init = function () {

            $loginForm.on('submit', function (e) {
                e.preventDefault();
                mailSendOtp(e);
            });

            $OtpAuthentication.on('submit', function (e) {
                e.preventDefault();
                verifyOtp(e);
            });

            $jsResendOtp.on('click', function (e) {
                e.preventDefault();
                resendOtp(e);
            });

            $jsFillOtpTxt.on('input', function () {
                $jsVerifyButton.prop('disabled', !$(this).val());
            });


            $isActiveExecutive.on('change', (e) => {
                isActiveChange(e);
            });

            if (!$.fn.DataTable.isDataTable($UserTable)) {
                let datatable = $UserTable.DataTable({
                    dom:
                        '<"card-header d-flex flex-column flex-md-row align-items-start align-items-md-center pb-md-0 pt-0"<f><"d-flex align-items-md-center justify-content-md-end gap-4"l<"dt-action-buttons"B>>' +
                        '>t' +
                        '<"row mx-1"' +
                        '<"col-sm-12 col-md-6"i>' +
                        '<"col-sm-12 col-md-6"p>' +
                        '>',
                    lengthMenu: [10, 40, 60, 80, 100],
                    columnDefs: [
                        {
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
                        searchPlaceholder: 'Search Executive',
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
            }

            const displayErrorMessage = (message) => {
                $errorMessage.text(message);
                Site.hideSpinner();
            };

            const handleSuccess = () => {
                $jsTimeCountDown.show();
                $jsFillOtpTxt.prop('disabled', false);
                $jsVerifyButton.prop('disabled', false);
                $jsEmailTxtbox.prop('disabled', true);
                $jsBtnSendOtpMail.prop('disabled', true);
                $hideShowResend.show();

                Site.successMessage('OTP sent successfully')
                startCountdown();
                Site.hideSpinner();
            };


            let mailSendOtp = (e) => {

                $errorMessage.text('');
                let email = $jsEmailTxtbox.val();
                if (!email) {
                    displayErrorMessage('Please enter a valid email address.');
                    return;
                }

                Site.showSpinner();
                $hdnMailInput.val(email);

                $.ajax({
                    url: e.target.dataset.action,
                    type: 'post',
                    data: {
                        '__RequestVerificationToken': $("[name='__RequestVerificationToken']").val(),
                        'email': email
                    },
                    success: function (response) {
                        if (response.success) {
                            handleSuccess();
                        } else if (response.errorMessage) {
                            displayErrorMessage(response.errorMessage);
                        } else {
                            displayErrorMessage('An unexpected error occurred. Please try again.');
                        }
                    },
                    error: function () {
                        displayErrorMessage('An error occurred while sending OTP. Please try again.');
                    },
                    complete: function () {
                       Site.unblockCard();
                    }
                });
            };


            let verifyOtp = (e) => {
                let otp = $jsFillOtpTxt.val();
                if (!otp) {
                    displayErrorMessage('Please enter OTP.');
                    return;
                }

                Site.showSpinner();
                $errorMessage.text('');

                $.ajax({
                    url: e.target.dataset.action,
                    type: 'post',
                    data: {
                        '__RequestVerificationToken': $("[name='__RequestVerificationToken']").val(),
                        'email': $jsEmailTxtbox.val(),
                        'otpNumber': otp
                    },
                    success: function (response) {
                        if (response.redirectUrl) {
                            window.location.href = response.redirectUrl;
                        } else {
                            displayErrorMessage(response.errorMessage || 'An unexpected error occurred. Please try again.');
                        }
                    },
                    error: function () {
                        displayErrorMessage('An error occurred while verifying OTP. Please try again.');
                    }
                });
            };


            let resendOtp = (e) => {

                clearInterval(countdownTimer); // Stop any existing timer
                remainingTime = countdownDuration; // Reset the time to 5 minutes
                timerRunning = false;

                let email = $jsEmailTxtbox.val();
                if (!email) {
                    displayErrorMessage('Please enter a valid email address.');
                    return;
                }

                Site.showSpinner();
                $errorMessage.text('');

                $.ajax({
                    url: e.target.dataset.action,
                    type: 'post',
                    data: {
                        '__RequestVerificationToken': $("[name='__RequestVerificationToken']").val(),
                        'email': email
                    },
                    success: function (response) {
                        if (response.success) {
                            handleSuccess();
                        } else if (response.errorMessage) {
                            displayErrorMessage(response.errorMessage);
                        } else {
                            displayErrorMessage('An unexpected error occurred. Please try again.');
                        }
                    },
                    error: function () {
                        displayErrorMessage('An error occurred while resending OTP. Please try again.');
                    }
                });
            };

            let startCountdown = function () {
               
                if (timerRunning) return;

                timerRunning = true;
                var remainingTime = countdownDuration;

                countdownTimer = setInterval(function () {
                    var minutes = Math.floor(remainingTime / 60);
                    var seconds = remainingTime % 60;

                    $jsTimeCountDown.text(formatTime(minutes) + ':' + formatTime(seconds));

                    if (remainingTime <= 0) {
                        clearInterval(countdownTimer);
                        $jsTimeCountDown.text('00:00');
                        timerRunning = false;
                    }

                    remainingTime--;
                }, 1000);
            }

            let formatTime = function (time) {
                return time < 10 ? '0' + time : time;
            }


            let isActiveChange = (e) => {
                e.preventDefault();
                Site.showSpinner();
                let isActive = e.target.checked;
                let userId = e.target.dataset.id;
                $.ajax({
                    url: e.target.dataset.action,
                    type: 'post',
                    data: {
                        '__RequestVerificationToken': $("[name='__RequestVerificationToken']").val(),
                        userId: userId,
                        'isActive': isActive
                    },
                    success: function (result, textStatus, jqXHR) {
                        if (textStatus === 'success')
                            Site.successMessage('Executive is Updated!');
                        else
                            Site.errorMessage(errors, 'An error occurred!');

                        Site.hideSpinner();
                    },
                    error: function (result) {
                        Site.errorMessage(errors, 'An error occurred!');
                        Site.hideSpinner();
                    }

                });
            };
        };

        return {
            init: init
        };
    })();
}