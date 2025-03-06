$(function () {
    const $document = $(document);

    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });


    let $Number = $('.number-type');

    $Number.on('input', function () {
        $(this).val($(this).val().replace(/[^0-9]/g, ''));
    });

    $document.on('click', '.jsDownloadPdf', function () {
        downloadPDF();
    });

    $document.on("submit", ".jsAttendeeForm", function (e) {
        e.preventDefault();

        const { key, iv } = generateKeyAndIv();
        $('#attendee-container .attendee-row').each(function (index) {
            let emailField = $(this).find(`input[name="attendeemodels[${index}].attendeeemail"]`);
            let mobileField = $(this).find(`input[name="attendeemodels[${index}].attendeemobilenumber"]`);
            let keyField = $(this).find(`input[name="attendeemodels[${index}].key"]`);
            let ivField = $(this).find(`input[name="attendeemodels[${index}].iv"]`);

            keyField.val(key);
            ivField.val(iv);

            if (emailField.val() != "") {
                const encryptedEmail = encryptText(emailField.val(), key, iv);
                emailField.val(encryptedEmail);
            }

            if (mobileField.val() != "") {
                const encryptedMobile = encryptText(mobileField.val(), key, iv);
                mobileField.val(encryptedMobile);
            }
        });

        this.submit();
    });

    function generateKeyAndIv() {
        const key = CryptoJS.lib.WordArray.random(16);
        const iv = CryptoJS.lib.WordArray.random(16);

        return {
            key: key.toString(CryptoJS.enc.Hex),
            iv: iv.toString(CryptoJS.enc.Hex)
        };
    }

    function encryptText(plainText, secretKey, ivKey) {
        const encrypted = CryptoJS.AES.encrypt(plainText, CryptoJS.enc.Hex.parse(secretKey), {
            keySize: 128 / 8,
            iv: CryptoJS.enc.Hex.parse(ivKey),
            mode: CryptoJS.mode.CBC,
            padding: CryptoJS.pad.Pkcs7
        });

        return encrypted.toString();
    }

    let downloadPDF = function () {

        const fileName = $('.jsDownloadPdf').data("filename");
        const { jsPDF } = window.jspdf;
        var doc = new jsPDF();

        var modalContent = document.getElementById('printRequestData'); // Your modal content div

        // Inject custom styles before rendering the content
        var customStyles = document.createElement('style');
        customStyles.innerHTML = `
            #printRequestData {
                /* Custom styles for downloading */
                padding: 60px;
                /* Other custom styles you want to apply for the print/export */
            }
        `;

        document.head.appendChild(customStyles); // Add the styles to the document

        // Use html2canvas to render the content into a canvas
        html2canvas(modalContent, {
            scale: 1, // Increase resolution of the canvas
            useCORS: true, // Ensure cross-origin images are included (if any)
        }).then(function (canvas) {
            var imgData = canvas.toDataURL('image/png');

            // Calculate the dimensions for the PDF page
            var imgWidth = 210; // Width of A4 in mm (210mm)
            var pageHeight = 297; // Height of A4 in mm (297mm)
            var imgHeight = (canvas.height * imgWidth) / canvas.width;
            var heightLeft = imgHeight;
            var position = 0;

            // Add the image to the PDF
            doc.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
            heightLeft -= pageHeight;

            // Handle multi-page PDF if the content exceeds one page
            while (heightLeft > 0) {
                position = heightLeft - imgHeight;
                doc.addPage();
                doc.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
                heightLeft -= pageHeight;
            }

            // Save the PDF
            doc.save(fileName);

            // Remove the custom styles after generating the PDF
            document.head.removeChild(customStyles);
        });
    }

    $document.on('click', '.jsPrintOut', function () {
        printModalContent();
    });

    let printModalContent = function () {
        var modalContent = document.getElementById('printRequestData').innerHTML;

        var newWindow = window.open('', '', 'width=800,height=600');
        newWindow.document.write(`
        <html>
            <head>
                <title>Print Modal Content</title>
                <link rel="stylesheet" href="/vendor/css/core.css" />
                <style>
                    /* Add any styles you want for printing */
                    @media print {
                        .print-it-bordered {
                            border-collapse: collapse;
                        }

                        .print-it-bordered th, 
                        .print-it-bordered td { 
                            padding: 8px;
                            border: 1px solid black;
                        }

                        .no-break-please {
                            page-break-inside: avoid;
                        }
                        table {
                            page-break-inside: avoid;
                        }
                        tr {
                            page-break-inside: avoid;
                            page-break-after: auto;
                        }
                        thead {
                            display: table-header-group;
                        }
                        tfoot {
                            display: table-footer-group;
                        }
                    }
                </style>
            </head>
            <body>
                ${modalContent}
            </body>
        </html>
    `);

        newWindow.document.close();  // Close the document to ensure all content is loaded.
        newWindow.focus();  // Focus on the new window.
        newWindow.print();  // Trigger the print dialog.
        newWindow.close();  // Close the window after printing.
    }

    // Spinner controls on ajax events
    //$document.ajaxStart(Site.showSpinner).ajaxStop(Site.hideSpinner);

    //// Event handlers for spinners and requests
    //$document.on('click', 'a:not(.footer-link, .print-btn, .dropdown-toggle, .layout-menu-toggle, .template-customizer-open-btn, .dropdown-item, .template-customizer-close-btn, .menu-link, .page-link, .dropdown-notifications-read, .dropdown-notifications-all, .nav-link, .paginate_button)', Site.showSpinner);

    //$('[data-spinner="show"]').on('click', Site.showSpinner);
    //$('[data-spinner="hide"]').on('click', Site.hideSpinner);

    let availableIndices = []; // Global array to track available indices for reuse
    let currentIndex = 0; // Initialize currentIndex to track the latest index

    $document.on('click', '.delete-attendee', function () {
        const deletedIndex = parseInt($(this).closest('.attendee-row').attr('data-attendee-index'));

        // Add the deleted index to the available indices array
        availableIndices.push(deletedIndex);

        // Remove the closest attendee-row
        $(this).closest('.attendee-row').remove();

        // Update remaining indices after deletion
        updateAttendeeIndices();
    });

    // Function to update data-attendee-index attributes and name attributes after a row is deleted
    function updateAttendeeIndices() {
        $('#attendee-container .attendee-row').each(function (index) {
            // Update data-attendee-index attribute
            $(this).attr('data-attendee-index', index);

            // Update name attributes for the inputs
            $(this).find('input, select').each(function () {
                const name = $(this).attr('name');
                if (name) {
                    // Replace the current index in the name with the new index
                    const newName = name.replace(/\[\d+\]/, `[${index}]`);
                    $(this).attr('name', newName);
                }
            });
        });

        // Ensure that if index 0 was deleted, it's available for re-insertion
        if (availableIndices.includes(0) && $('#attendee-container .attendee-row').length === 0) {
            currentIndex = 0; // Reset to 0
        }

        updateAddButtonState();
    }

    // Function to initialize attendees
    function initializeAttendees() {
        let attendees = $("#attendee-container").data('attendees') || []; // Use global variable or empty array if none
        const container = document.getElementById('attendee-container');
        const template = document.getElementById('attendee-template').innerHTML;

        // Function to create and append attendee row
        function createAttendeeRow(attendee, index) {
            let row = template
                .replace(/{index}/g, index)
                .replace(/{attendeeid}/g, attendee.attendeeId || '')
                .replace(/{attendeename}/g, attendee.attendeeName || '')
                .replace(/{attendeeemail}/g, attendee.attendeeEmail || '')
                .replace(/{attendeemobilenumber}/g, attendee.attendeeMobileNumber || '')
                .replace(/{attendeeticket}/g, attendee.attendeeTicket || '')
                .replace(/{seattypeselected}/g, !attendee.attendeeSeatType ? 'selected' : '')
                .replace(/{seattypeselected1}/g, attendee.attendeeSeatType == 1 ? 'selected' : '')
                .replace(/{seattypeselected2}/g, attendee.attendeeSeatType == 2 ? 'selected' : '')
                .replace(/{seattypeselected3}/g, attendee.attendeeSeatType == 3 ? 'selected' : '');

            container.insertAdjacentHTML('beforeend', row);
            reinitializeCleaveMasks();
            reinitializeNumberInputs();

            //for conference uses total seat
            let totalSuiteSeat = parseInt($('.jsTotalSeatSuite').val()) || 0;
            if (totalSuiteSeat != 0) {
                // Check if the number of attendee rows equals or exceeds the available seats
                if (index >= (totalSuiteSeat - 1)) {
                    // Disable the add attendee button
                    $('.jsAddAttendee').prop('disabled', true);
                    $('#ticket-message').text('No more seats available.').show();
                } else {
                    // Enable the add attendee button and hide the message if seats are available
                    $('.jsAddAttendee').prop('disabled', false);
                    $('#ticket-message').hide();
                }

            }
        }

        // Add existing attendees to the container
        attendees.forEach((attendee, index) => {
            createAttendeeRow(attendee, index);
        });

        // Set the starting index based on available indices or existing rows
        currentIndex = (availableIndices.length > 0) ? availableIndices.shift() : $('#attendee-container .attendee-row').length;

        // Add an initial empty attendee row if no attendees exist
        if ($('#attendee-container .attendee-row').length === 0) {
            createAttendeeRow({
                attendeeId: '',
                attendeeName: '',
                attendeeEmail: '',
                attendeeMobileNumber: '',
                attendeeTicket: '',
                attendeeSeatType: ''
            }, 0);
        }

        // Function to add a new attendee row
        function addNewAttendee() {


            // Check if index 0 is available
            if (availableIndices.includes(0)) {
                currentIndex = 0; // Set to 0 if it's available
                availableIndices.splice(availableIndices.indexOf(0), 1); // Remove 0 from availableIndices
            } else {
                currentIndex = (availableIndices.length > 0) ? availableIndices.shift() : $('#attendee-container .attendee-row').length; // Get next available index or use current length
            }

            // Check if currentIndex already exists in the DOM
            const existingRow = $('#attendee-container .attendee-row[data-attendee-index="' + currentIndex + '"]');
            if (existingRow.length > 0) {
                // If it exists, increment the index to find a free slot
                currentIndex = $('#attendee-container .attendee-row').length; // Add at the end if current index already exists
            }

            // Create new attendee row with the current index
            createAttendeeRow({
                attendeeId: '',
                attendeeName: '',
                attendeeEmail: '',
                attendeeMobileNumber: '',
                attendeeTicket: '',
                attendeeSeatType: ''
            }, currentIndex);
        }

        // Attach an event listener to the button for adding attendees
        document.querySelector('.jsAddAttendee').addEventListener('click', addNewAttendee);
    }

    function reinitializeCleaveMasks() {
        document.querySelectorAll('.phone-number-mask').forEach(function (input) {
            if (!input.dataset.cleaveInitialized) {
                new Cleave(input, {
                    phone: true,
                    phoneRegionCode: 'US'
                });
                input.dataset.cleaveInitialized = true;
            }
        });
    }

    function reinitializeNumberInputs() {
        document.querySelectorAll('.number-input-mask').forEach(function (input) {
            if (!input.dataset.numberInitialized) {
                input.addEventListener('input', function (e) {
                    let value = e.target.value.replace(/[^0-9.-]/g, '');
                    e.target.value = value;
                });

                input.dataset.numberInitialized = true;
            }
        });
    }


    document.addEventListener('DOMContentLoaded', initializeAttendees);


    $document.off('click', '.jsCreateRequest').on('click', '.jsCreateRequest', function (e) {
        e.stopImmediatePropagation();
        eventRequest(e);
    });
    $document.off('click', '.jsMailEventRequest, .jsExportEventRequest').on('click', '.jsMailEventRequest, .jsExportEventRequest', function (e) {
        e.stopImmediatePropagation();
        mailRequest(e);
    });

    $document.off('click', '.jsExportExcel').on('click', '.jsExportExcel', function (e) {
        e.stopImmediatePropagation();
        excelRequest(e);
    });

   $document.off('click', '.jsApproveRequest, .jsRejectRequest, .jsViewEventRequest, .jsFillCateringEventRequest , .jsFillGuestEventRequest, .jsFillEventRequest').on('click', '.jsApproveRequest, .jsRejectRequest, .jsViewEventRequest,  .jsFillCateringEventRequest , .jsFillGuestEventRequest, .jsFillEventRequest', function (e) {
        e.stopImmediatePropagation();
        approveRequest(e);
    });

    $document.off("change", ".jsEventName").on("change", ".jsEventName", function (e) {
        e.stopImmediatePropagation();
        handleEventSelection();
    });

    $document.on('change', '.jsSROType', function () {
        handleSROType();
    });

    $document.off('input', '.jsTxtApprovePasses, .jsTxtApproveParking, .jsTxtApproveSROSeat').on('input', '.jsTxtApprovePasses, .jsTxtApproveParking, .jsTxtApproveSROSeat', function (e) {
        e.stopImmediatePropagation();
        handleInputCheck();
    });
    $document.off('input', '.jsTxtRequestTicket, .jsTxtRequestParking, .jsTxtRequestSROTicket').on('input', '.jsTxtRequestTicket, .jsTxtRequestParking, .jsTxtRequestSROTicket', function (e) {
        e.stopImmediatePropagation();
        checkTicketAndParkingValidity();
    });

    $document.off('input', '.jsAppliedTicket').on('input', '.jsAppliedTicket', function () {
        updateAddButtonState();
        // removeExtraFieldsIfExceeding();
    });

    let updateAddButtonState = function () {
        var $appliedTicket = $('.jsAppliedTicket');
        var approvedTickets = parseInt($('.jsApproveTicket').val()) || 0;
        var totalAppliedTickets = 0;

        var hasZeroTicket = false;
        let invalidAttendee = false;
        let totalSuiteSeat = parseInt($('.jsTotalSeatSuite').val()) || 0;
        let attendeeRows = $('#attendee-container .attendee-row');

        // Iterate over each attendee row
        attendeeRows.each(function () {
            var ticketCount = parseInt($(this).find('.jsAppliedTicket').val()) || 0;
            totalAppliedTickets += ticketCount;

            let attendeeName = $(this).find('.jsAttendeeName').val();
            if (attendeeName && attendeeName.trim() !== '') {
                invalidAttendee = true;
            }

            if (ticketCount === 0 && totalSuiteSeat === 0) {
                hasZeroTicket = true;
            }
        });

        // If any attendee has zero tickets, show the message
        if (hasZeroTicket && invalidAttendee) {
            $('#ticket-message').text('You have to assign at least 1 ticket.').show();
            $('.jsAddAttendee, .btn-submit').prop('disabled', true);
            return;
        }

        // Disable or enable the Add button based on total applied tickets
        if (totalAppliedTickets >= approvedTickets && approvedTickets > 0) {
            $('.jsAddAttendee').prop('disabled', true);
            $('#ticket-message').text('The total number of tickets has reached the approved limit.').show();
        } else {
            $('.jsAddAttendee').prop('disabled', false);
            $('#ticket-message').text('').hide();
        }

        // Disable the submit button if total tickets exceed approved tickets
        if (totalAppliedTickets > approvedTickets) {
            $('.btn-submit').prop('disabled', true);
            $('#ticket-message').text('You cannot assign more tickets than you are approved for.').show();
        } else {
            $('.btn-submit').prop('disabled', false);
            $('#ticket-message').hide();
        }
    };


    let eventRequest = (e) => {
        e.preventDefault();
        Site.showSpinner();
        const title = e.currentTarget.title;
        const actionUrl = e.currentTarget.dataset.action;

        $.ajax({
            url: actionUrl,
            type: 'GET',
            success: function (result) {
                formModalComponent.show(title, result);

                if ($(".jsEventName").val() != 0 && title != "View") {
                    handleEventSelection();
                   // handleSROType();
                }
                // Attach event handler to modal shown event
                // $('#form-modal').on('shown.bs.modal', handleModalShown);

                $('#selectpickerLiveSearch').selectpicker();

                $.validator.unobtrusive.parse(".jsEventRequestForm");
                Site.hideSpinner();
            },
            error: function () {
                alert("Error loading partial view.");
                Site.hideSpinner();
            }
        });
    };

    let approveRequest = (e) => {
        e.preventDefault();
        Site.showSpinner();
        const title = e.currentTarget.title;
        const actionUrl = e.currentTarget.dataset.action;
        $.ajax({
            url: actionUrl,
            type: 'GET',
            success: function (result) {
                formModalComponent.show(title, result);

                if (title == "Guest Data" || title == "View Request" || title == "Fill Request" || title == "View Conference Request" || title == "Guest Conference Data" || title == "Fill Conference Request") {
                    //updateRepeaterInputNames();
                    //addAttendee();
                    initializeAttendees();
                    updateAddButtonState();
                }

                $.validator.unobtrusive.parse(".jsEventRequestForm");
                Site.hideSpinner();
            },
            error: function () {
                alert("Error loading partial view.");
                Site.hideSpinner();
            }
        });
    };

    let excelRequest = (e) => {
        e.preventDefault();
        const actionUrl = e.currentTarget.dataset.action;
        // Redirect to the URL to download the Excel file
        window.location.href = actionUrl;
    };

    let mailRequest = (e) => {
        e.preventDefault();
        const title = e.currentTarget.title;
        const actionUrl = e.currentTarget.dataset.action;
        Site.showSpinner();
        $.ajax({
            url: actionUrl,
            type: 'GET',
            success: function (result) {
                formModalComponent.show(title, result);
                Site.hideSpinner();
            },
            error: function () {
                Site.hideSpinner();
                alert("Error loading partial view.");
            }
        });
    };

    let handleSROType = function () {

        checkTicketAndParkingValidity();

        const $sroType = $('.jsSROType');
        const $hideShowDiv = $('.jsHideShowSRO');
        let $removeAttribute = $('.jsRemoveAttribute');
        const $txtRequestSROTicket = $('.jsTxtRequestSROTicket');
        const $hdnSROSeat = parseInt($('.jsHdnAvailableSROSeat').text().match(/\d+/), 10);
        //const requestSroTciketValue = parseInt($txtRequestSROTicket.val(), 10);

        if ($hdnSROSeat !== 0) {
            $txtRequestSROTicket.removeClass('disabled-input').prop('disabled', false);
        }
        else {
            $txtRequestSROTicket.addClass('disabled-input').prop('disabled', true);
        }

        if ($sroType.val() === "0") {
            $txtRequestSROTicket.val(0);
            $removeAttribute.removeAttr('required');
            $hideShowDiv.hide();
        } else {
           
            $removeAttribute.attr('required', 'required');
            $hideShowDiv.show();
        }

      
    }

    const handleEventSelection = () => {
        var selectedOption = $(".jsEventName").find('option:selected')
        $(".jsHdnEventId").val(selectedOption.val());
       // $(".jsTxtRequestSROTicket").val(0);
        var availableSeat = selectedOption.data("avialableseat") || 0; // Defaults to 0 if no data found
        var availableSROSeat = selectedOption.data("availblesroseat") || 0; // Defaults to 0 if no data found
        var availableParking = selectedOption.data("availableparking") || 0; // Defaults to 0 if no data found
        var sROperTicketPrice = selectedOption.data("sroperticketprice") || 0; // Defaults to 0 if no data found
        $('.jsSROPerTicket').text(`(SRO Per Ticket Price: ${sROperTicketPrice}$)`);

        var seatColorClass = availableSeat > 0 ? "available-seat" : "not-available";
        var seatSROColorClass = availableSROSeat > 0 ? "available-seat" : "not-available";
        var parkingColorClass = availableParking > 0 ? "available-seat" : "not-available";

        const $availSeatInfo = $('#available-seat-info');
        const $availSROSeatInfo = $('#available-seat-sro-info');
        const $availParkingInfo = $('#available-parking-info');
        const $requestPasses = $('input[name="AppliedTickets"]');
        //$(".jsHdnRequestPasses").val($requestPasses.val());
        const $requestParking = $('input[name="AppliedParkingPasses"]');
        //$(".jsHdnParkingPasses").val($requestParking.val());

        $availSeatInfo.removeClass('available-seat not-available').addClass(seatColorClass)
            .html(`<small class="jsHdnAvailableSeat"> (${availableSeat} Available) </small>`);
        $availSROSeatInfo.removeClass('available-seat not-available').addClass(seatSROColorClass)
            .html(`<small class="jsHdnAvailableSROSeat"> (${availableSROSeat} Available) </small>`);
        $availParkingInfo.removeClass('available-seat not-available').addClass(parkingColorClass)
            .html(`<small class="jsHdnAvailableParking"> (${availableParking} Available) </small>`);

        if (availableSeat > 0) {
            $requestPasses.removeClass('disabled-input').prop('disabled', false);
        } else {
            $requestPasses.addClass('disabled-input').prop('disabled', true);
        }

        if (availableParking > 0) {
            $requestParking.removeClass('disabled-input').prop('disabled', false);
        } else {
            $requestParking.addClass('disabled-input').prop('disabled', true);
        }

        handleSROType();
    };

    //Create or Update request event check avialable ticket and parking 
    const checkTicketAndParkingValidity = () => {

        const $srotype = $('.jsSROType').val();
        // Get values from input fields
        const $requestTicket = $('.jsTxtRequestTicket').val();
        const $requestSROTicket = $('.jsTxtRequestSROTicket').val();
        const $requestParkingValue = $('.jsTxtRequestParking').val();

        // Get available seat and parking values from <small> tags
        const $hdnSeat = parseInt($('.jsHdnAvailableSeat').text().match(/\d+/), 10);
        const $hdnSROSeat = parseInt($('.jsHdnAvailableSROSeat').text().match(/\d+/), 10);
        const $hdnParking = parseInt($('.jsHdnAvailableParking').text().match(/\d+/), 10);

        // Select elements for displaying validation messages
        const $displayMessageSeat = $('.jsSpanDisplaySeat');
        const $displayMessageSROSeat = $('.jsSpanDisplaySROSeat');
        const $displayMessageParking = $('.jsSpanDisplayParking');

        // Select the submit button
        const $btnSubmitApprove = $('.btnSubmitCreateRequest');

        // Initialize validation flags
        let isPassesValid = true;
        let isSROPassesValid = true;
        let isParkingValid = true;
        // Check ticket passes validity

        if (!isNaN($requestTicket) && $requestTicket !== '') {
            const requestTicketValue = parseInt($requestTicket, 10);
            if (requestTicketValue < 0) {
                $displayMessageSeat.css("display", "block").text(`Event tickets cannot be a negative number.`);
                isPassesValid = false;
            } else if (requestTicketValue > $hdnSeat) {
                $displayMessageSeat.css("display", "block").text(`You are trying to request ${$requestTicket} event tickets , but only ${$hdnSeat} are available.`);
                isPassesValid = false;
            } else  if (requestTicketValue === 0) {
               // $displayMessageSeat.css("display", "block").text(`Event tickets is required.`);
                isPassesValid = false;
            } else {
                $displayMessageSeat.css("display", "none").text('');
                isPassesValid = true;
            }
        }

        if ($srotype == "1") {
            // Check SRO ticket passes validity
            if (!isNaN($requestSROTicket) && $requestSROTicket !== '') {
                const requestSROTicketValue = parseInt($requestSROTicket, 10);

                if (requestSROTicketValue < 0) {
                    $displayMessageSROSeat.css("display", "block").text(`SRO tickets cannot be a negative number.`);
                    isSROPassesValid = false;
                } else if (requestSROTicketValue > $hdnSROSeat) {
                    $displayMessageSROSeat.css("display", "block").text(`You are trying to request ${$requestSROTicket} SRO tickets, but only ${$hdnSROSeat} are available.`);
                    isSROPassesValid = false;
                } else if (requestSROTicketValue === 0 && $hdnSROSeat !== 0)
                {
                    isSROPassesValid = false;
                } else {
                    $displayMessageSROSeat.css("display", "none").text('');
                    isSROPassesValid = true;
                }
            } else {
                isSROPassesValid = true; // Consider it invalid if empty
            }
        }

        // Check parking validity
        if (!isNaN($requestParkingValue) && $requestParkingValue !== '') {
            const parkingIntValue = parseInt($requestParkingValue, 10);
            if (parkingIntValue < 0) {
                $displayMessageParking.css("display", "block").text(`Parking value cannot be a negative number.`);
                isParkingValid = false;
            } else if (parkingIntValue > $hdnParking) {
                $displayMessageParking.css("display", "block").text(`You are trying to request ${$requestParkingValue} parking passes, but only ${$hdnParking} are available.`);
                isParkingValid = false;
            } else {
                $displayMessageParking.css("display", "none").text('');
                isParkingValid = true;
            }
        }

        // Enable or disable the submit button based on the validation flags
        if (isPassesValid && isSROPassesValid && isParkingValid) {
            $btnSubmitApprove.removeClass('disabled-input');
        } else {
            $btnSubmitApprove.addClass('disabled-input');
        }
    };


    //Approve check avialable ticket and parking 
    const handleInputCheck = () => {

        const $passesValue = $('.jsTxtApprovePasses').val();
        const $sroPassesValue = $('.jsTxtApproveSROSeat').val();
        const $parkingValue = $('.jsTxtApproveParking').val();
        //const $adminMessage = $('.jsTexAreaMessage').val();

        const $hdnSROType = $('.jsHdnSROType').val();
        const $hdnSeat = parseInt($('.jsHdnAvailableSeat').val(), 10);
        const $hdnSROSeat = parseInt($('.jsHdnAvailableSROSeat').val(), 10);
        const $hdnParking = parseInt($('.jsHdnAvailableParking').val(), 10);
        const $btnSubmitApprove = $('.btnSubmitApprove');
        const $displayMessageSeat = $('.jsSpanDisplaySeat');
        const $displayMessageSROSeat = $('.jsSpanDisplaySROSeat');
        const $displayMessageParking = $('.jsSpanDisplayParking');

        const $hdnRequestSeat = $('.jsHdnRequestSeat').val();
        const $hdnRequestSROSeat = $('.jsHdnRequestSROSeat').val();
        const $hdnRequestParking = $('.jsHdnRequestParking').val();

        //let isAdminMessageValid = $adminMessage !== '';
        let isPassesValid = true;
        let isSROPassesValid = true;
        let isParkingValid = true;


        // Check passes
        if (!isNaN($passesValue) && $passesValue !== '') {
            const passesIntValue = parseInt($passesValue, 10);

            // Check if passes exceed available seats
            if (passesIntValue > $hdnSeat) {
                $displayMessageSeat.css("display", "block").text(`You are trying to approve ${$passesValue} event tickets, but only ${$hdnSeat} are available.`);
                isPassesValid = false;
            }
            // Check if passes exceed requested seats
            else if (passesIntValue > $hdnRequestSeat) {
                $displayMessageSeat.css("display", "block").text(`You are trying to approve ${$passesValue} event tickets, but only ${$hdnRequestSeat} were requested.`);
                isPassesValid = false;
            }
            // Passes are valid
            else {
                $displayMessageSeat.css("display", "none").text('');
                isPassesValid = true;
            }
        } else {
            // Consider it valid if empty
            isPassesValid = false;
        }

        if ($hdnSROType == "Yes") {
            // Check SRO passes
            if (!isNaN($sroPassesValue) && $sroPassesValue !== '') {
                const sroPassesIntValue = parseInt($sroPassesValue, 10);

                // Check if passes exceed available seats
                if (sroPassesIntValue > $hdnSROSeat) {
                    $displayMessageSROSeat.css("display", "block").text(`You are trying to approve ${$sroPassesValue} SRO tickets, but only ${$hdnSROSeat} are available.`);
                    isSROPassesValid = false;
                }
                // Check if passes exceed requested seats
                else if (sroPassesIntValue > $hdnRequestSROSeat) {
                    $displayMessageSROSeat.css("display", "block").text(`You are trying to approve ${$sroPassesValue} SRO tickets, but only ${$hdnRequestSROSeat} were requested.`);
                    isSROPassesValid = false;
                }
                // Passes are valid
                else {
                    $displayMessageSROSeat.css("display", "none").text('');
                    isSROPassesValid = true;
                }
            } else {
                // Consider it valid if empty
                isSROPassesValid = false;
            }
        }
        else {
            isSROPassesValid = true;
        }

        // Check parking
        if (!isNaN($parkingValue) && $parkingValue !== '') {

            const parkingIntValue = parseInt($parkingValue, 10);

            // Check if parking spots exceed available parking
            if (parkingIntValue > $hdnParking) {
                $displayMessageParking.css("display", "block").text(`You are trying to approve ${$parkingValue} parking passes, but only ${$hdnParking} are available.`);
                isParkingValid = false;
            }
            // Check if parking spots exceed requested seats (assuming you have such a requirement)
            else if (parkingIntValue > $hdnRequestParking) {
                $displayMessageParking.css("display", "block").text(`You are trying to approve ${$parkingValue} parking passes, but only ${$hdnRequestParking} were requested.`);
                isParkingValid = false;
            }
            // If parking is valid
            else {
                $displayMessageParking.css("display", "none").text('');
                isParkingValid = true;
            }
        } else {
            // Consider it valid if empty
            isParkingValid = false;
        }


        // Enable the submit button only if all fields are valid
        if (isPassesValid && isParkingValid && isSROPassesValid) {
            $btnSubmitApprove.removeAttr('disabled');
        } else {
            $btnSubmitApprove.attr('disabled', true);
        }
    };


    const phoneMask = document.querySelector('.phone-number-mask');

    if (phoneMask) {
        new Cleave(phoneMask, {
            phone: true,
            phoneRegionCode: 'US'
        });
    }

});

'use strict';
$(document).ready(function () {
    // Full Toolbar
    const fullToolbar = [
        [
            { font: [] },
            { size: [] }
        ],
        ['bold', 'italic', 'underline', 'strike'],
        [
            { color: [] },
            { background: [] }
        ],
        [
            { script: 'super' },
            { script: 'sub' }
        ],
        [
            { header: '1' },
            { header: '2' },
            'blockquote',
            'code-block'
        ],
        [
            { list: 'ordered' },
            { list: 'bullet' },
            { indent: '-1' },
            { indent: '+1' }
        ],
        [{ direction: 'rtl' }],
        ['link', 'formula'],
        ['clean']
    ];

    //$(document).ready(function () {

        const $wizardNumbered = $('.wizard-numbered'),
            $wizardNumberedBtnNextList = $wizardNumbered.find('.btn-next'),
            $wizardNumberedBtnPrevList = $wizardNumbered.find('.btn-prev'),
            $wizardNumberedBtnSubmit = $wizardNumbered.find('.btn-submit');

        if ($wizardNumbered.length) { // Check if the element exists
            const numberedStepper = new Stepper($wizardNumbered[0], {
                linear: false
            });

            // Handle next button clicks
            if ($wizardNumberedBtnNextList.length) {
                $wizardNumberedBtnNextList.each(function () {
                    $(this).on('click', function () {
                        numberedStepper.next();
                    });
                });
            }

            // Handle previous button clicks
            if ($wizardNumberedBtnPrevList.length) {
                $wizardNumberedBtnPrevList.each(function () {
                    $(this).on('click', function () {
                        numberedStepper.previous();
                    });
                });
            }
        }

        const fullEditorElement = $('#full-editor')[0];

        if (fullEditorElement && typeof fullEditor === "undefined") {
            var fullEditor = new Quill('#full-editor', {
                bounds: '#full-editor',
                placeholder: 'Type Something...',
                modules: {
                    formula: true,
                    toolbar: fullToolbar
                },
                theme: 'snow'
            });
        }

        $('.jsMailAttendeeForm').on('submit', function (e) {
            const attendeeNotesInput = $('#AttendeeNotes')[0];
            attendeeNotesInput.value = fullEditor.root.innerHTML;

            const fileInput = $('#fileInput')[0];
            const fileError = $('#fileError');
            const file = fileInput.files[0];

            if (file) {
                const maxFileSize = 10 * 1024 * 1024;

                if (file.type !== 'application/pdf') {
                    e.preventDefault();
                    fileError.text('Please select a valid PDF file.').show();
                    return;
                }

                if (file.size > maxFileSize) {
                    e.preventDefault();
                    fileError.text('File size exceeds the allowed limit.').show();
                    return;
                }
            }

            fileError.hide();
        });
    //});
});

if (typeof Site === 'undefined') {
    var Site = (function () {

        //let $spinner = $('#spinner');
        //let $spinnerPrompter = $('#spinnerPrompter');
        let cardBlockSpinner = $('.btn-card-block-spinner');
        let cardSection = $('#card-block');
        let $fromRegis = $('#formAuthentication');

        let showSpinner = () => {
            blockCard();
            disableScrolling();
          //  $spinner.show();
        }

        let hideSpinner = () => {
            unblockCard();
            enableScrolling();
           // $spinner.hide();
           // promptSpinner('');
        }

        //let promptSpinner = (text) => {
        //    $spinnerPrompter.text(text);
        //}

        let disableScrolling = () => {
            $('html, body').addClass('overflow-hidden');
        }

        let enableScrolling = () => {
            $('html, body').removeClass('overflow-hidden');
        }

        let successMessage = function (message) {
            toastr.success(message, 'success', {
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right',
                timeOut: 5000,
                extendedTimeOut: 2000
            });
        }

        let errorMessage = function (errors, meesage) {
            toastr.error(errors, meesage, {
                closeButton: true,
                progressBar: true,
                positionClass: 'toast-top-right',
                timeOut: 5000,
                extendedTimeOut: 1000
            });
        }

        const blockCard = () => {
            if (cardBlockSpinner.length && cardSection.length) {
                cardSection.block({
                    message:
                        '<div class="spinner-container">' +
                        '<div class="sk-wave"><div class="sk-rect sk-wave-rect"></div>' +
                        '<div class="sk-rect sk-wave-rect"></div><div class="sk-rect sk-wave-rect"></div>' +
                        '<div class="sk-rect sk-wave-rect"></div><div class="sk-rect sk-wave-rect"></div></div>' +
                        '</div>',
                    css: {
                        backgroundColor: 'transparent',
                        border: '0'
                    },
                    overlayCSS: {
                        opacity: 0.7
                    }
                });
            }
        };

        const unblockCard = () => {
            if (cardSection.length) {
                cardSection.unblock();
            }
        };

        return {
            showSpinner: showSpinner,
            hideSpinner: hideSpinner,
           // promptSpinner: promptSpinner,
            successMessage: successMessage,
            errorMessage: errorMessage,
            blockCard: blockCard,
            unblockCard: unblockCard
        };
    })();
}