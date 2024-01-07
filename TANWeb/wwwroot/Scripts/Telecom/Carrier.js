function validateCarrierForm() {
    if ($('#carrierName').val() != "" || $('.errormsg').text() != "") {
        return true;
    }
    else {
        $('#carrierName_Error').text('Carrier Name is required');
        return false;
    }
}

function saveCarrierInfo() {
    let token = $('input[name="__RequestVerificationToken"]').val();
    let carrier = {
        CarrierName: $('#carrierName').val()
    };
    showLoader();
    $.ajax({
        url: urls.CarrierManagement.SaveCarrier,
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: carrier,
        async: false,
        success: function (result) {
            hideLoader();
            if (result == 'success') {
                SuccessMsg("Success", "Record Added successfully", "success");
                $('#CarrrierInfoModal').modal('hide');
            }
            else if (result == "failed") {
                ErrorMsg("Failed", "error", "error")
                $('#CarrrierInfoModal').modal('hide');
            }
            let table = $('#carrierInfoTable').DataTable();
            table.destroy();
            loadCarrierData();
            clearCarrierData();
        },
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
            else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                $('#CarrrierInfoModal').modal('hide');
                let table = $('#carrierInfoTable').DataTable();
                table.destroy();
                loadCarrierData();
                clearCarrierData();
            }
            else {
                ErrorMsg("Failed", error, "error");
            }
        }
    });
}

function loadCarrierData() {
    $('#carrierInfoTable').DataTable(
        {
            ajax: {
                url: urls.CarrierManagement.GetCarriersList,
                type: "POST",
                data: function (data) {
                    // You can pass additional data to the server if needed
                    data.searchValue = $('#carrierInfoTable_filter input').val();
                    data.start = data.start || 0; // start parameter
                    data.length = data.length || 10; // length parameter
                    data.draw = data.draw || 1; // draw parameter
                    data.sortColumn = data.columns[data.order[0].column].data; // sort column
                    data.sortDirection = data.order[0].dir;
                },
                error: function (xhr, status, error) {
                    if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                        ErrorMsg("Failed", "Unauthorized Access");
                    }
                },
            },
            processing: true,
            filter: true,
            serverSide: true,
            pagingType: "full_numbers",
            language: {
                paginate: {
                    first: '<i class="fa fa-angle-double-left"></i>',
                    previous: '<i class="fa fa-angle-left"></i>',
                    next: '<i class="fa fa-angle-right"></i>',
                    last: '<i class="fa fa-angle-double-right"></i>',
                }
            },
            columns: [
                { data: "carrierName", name: "Carrier Name" },
                { data: "createdDate", name: "Created Date" },
                {
                    data: null, orderable: false,
                    "render": function (data, type, full, row) {
                        return "<a href='#' onclick=editCarrier('" + full.carrierId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Edit' class='tooltip-black'><i class='ri-edit-2-line fs-6 pe-2 text-info'></i></a>"
                            + " " + "<a href='#' onclick=deleteCarrier('" + full.carrierId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
                    }
                }
            ]
        }
    );

}

function clearCarrierData() {
    $('#carrierId').val('');
    $('#carrierName').val('');
    $('.errormsg').text('');
    $('#carrierName_Error').text('');

    $('#CarrrierInfoModalLabel').text("Add Carrier");
    $('#btnSaveCarrier').show();
    $('#btnUpdateCarrier').hide();
}

function editCarrier(carrierId) {
    clearCarrierData();
    showLoader();
    $.ajax({
        url: urls.CarrierManagement.EditCarrier,
        type: 'GET',
        data: { carrierId: carrierId },
        success: function (result) {
            hideLoader();
            $('#CarrrierInfoModal').modal('show');
            $('#carrierId').val(result.carrierId);
            $('#carrierName').val(result.carrierName);
            $('#CarrrierInfoModalLabel').text("Update Organization");
            $('#btnSaveCarrier').hide();
            $('#btnUpdateCarrier').show();
        },
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
            else {
                ErrorMsg("Failed", error, "error")
            }
        }
    });
}

function validateInputOnBlur(inputElement) {
    let inputValue = inputElement.value;
    let saveButton = $('#btnSaveCarrier');
    let updateButton = $('#btnUpdateCarrier');
    let inputCarrierName = document.getElementById('carrierName');
    let hasCharacter = /[a-zA-Z]/.test(inputValue); // Check for at least one character
    if (!(hasCharacter)) {
        $('#carrierName_Error').text("Input is invalid. It should contain at least one character");
        inputCarrierName.focus();
        saveButton.prop('disabled', true); // Disable the button
        updateButton.prop('disabled', true); // Disable the button
    }
    else if ($('#carrierName_Error').text() != "") {
        inputCarrierName.focus();
        saveButton.prop('disabled', true); // Disable the button
        updateButton.prop('disabled', true); // Disable the button
    }
    else {
        $('#carrierName_Error').text('');
        saveButton.prop('disabled', false); // Enable the button
        updateButton.prop('disabled', false); // Enable the button
        checkForUniqueCarrierName();
    }
}

function checkForUniqueCarrierName() {
    let carrierName = $('#carrierName').val();
    let carrierId = $('#carrierId').val();
    let saveButton = $('#btnSaveCarrier');
    let updateButton = $('#btnUpdateCarrier');
    let inputCarrierName = document.getElementById('carrierName');
    $.ajax({
        url: urls.CarrierManagement.CheckForCarrierName,
        type: 'GET',
        data: { carrierName: carrierName, carrierId: carrierId },
        success: function (result) {
            if (result) {
                $('#carrierName_Error').text("Carrier Name already exists, please use another Name");
                inputCarrierName.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else {
                $('#carrierName_Error').text('');
                saveButton.prop('disabled', false); // Disable the button
                updateButton.prop('disabled', false); // Disable the button
            }
        },
        error: function (xhr, status, error) {
            ErrorMsg("Failed", error, "error")
        }
    });
}

function updateCarrier() {
    let token = $('input[name="__RequestVerificationToken"]').val();
    let carrier = {
        CarrierId: $('#carrierId').val(),
        CarrierName: $('#carrierName').val()
    };
    showLoader();
    $.ajax({
        url: urls.CarrierManagement.UpdateCarrier,
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: carrier,
        async: false,
        success: function (result) {
            hideLoader();
            if (result == 'success') {
                SuccessMsg("Success", "Record Updated successfully", "success");
                $('#CarrrierInfoModal').modal('hide');
            }
            else {
                $('#CarrrierInfoModal').modal('hide');
                ErrorMsg("Failed", error, "error")
            }
            let table = $('#carrierInfoTable').DataTable();
            table.destroy();
            loadCarrierData();
            clearCarrierData();
        },
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
            else {
                ErrorMsg("Failed", error, "error")
            }
        }
    });
}
function deleteCarrier(carrierId) {
    let token = $('input[name="__RequestVerificationToken"]').val();
    ConfirmMsg('Carrier Management', 'Are you sure want to delete this carrier?', 'Continue', event, function () {
        $.ajax({
            url: urls.CarrierManagement.DeleteCarrier,
            type: 'POST',
            headers: { 'RequestVerificationToken': token },
            data: { carrierId: carrierId },
            async: false,
            success: function (result) {
                if (result == 'success') {
                    SuccessMsg("Success", "Record Deleted successfully", "success");
                }
                else {
                    $('#CarrrierInfoModal').modal('hide');
                    ErrorMsg("Failed", error, "error")
                }
                let table = $('#carrierInfoTable').DataTable();
                table.destroy();
                loadCarrierData();
            },
            error: function (xhr, status, error) {
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Oops", "Unauthorized Access", "error")
                }
                else {
                    ErrorMsg("Failed", error, "error")
                }
            }
        });
    });
}