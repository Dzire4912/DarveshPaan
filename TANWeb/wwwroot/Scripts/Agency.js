
function clearAgencyData() {

    $('#agencyId').val('');
    $('#agencyName').val('');
    $('#agencyAddress1').val('');
    $('#agencyAddress2').val('');
    $('#agencyCity').val('');
    $('#agencyState').val(null).trigger('change');
    $('#agencyZipCode').val('');
    $('#agencyFacilityName').val(null).trigger('change');

    $('.errormsg').text('');

    $('#AgencyModalLabel').text("Add Agency");
    $('#btnSaveAgency').show();
    $('#btnUpdateAgency').hide();
    $('#agencyId').prop('disabled', false);
    $('#agencyFacilityName').prop('disabled', false);
}

function saveAgency() {
    let token = $('input[name="__RequestVerificationToken"]').val();
        let Agency = {
            AgencyId: $('#agencyId').val(),
            AgencyName: $('#agencyName').val(),
            Address1: $('#agencyAddress1').val(),
            Address2: $('#agencyAddress2').val(),
            City: $('#agencyCity').val(),
            State: $('#agencyState').val(),
            ZipCode: $('#agencyZipCode').val(),
            FacilityId: $('#agencyFacilityName').val()
        };
    showLoader();
        $.ajax({
            url: urls.AgencyManagement.AddAgency,
            type: 'POST',
            headers: { 'RequestVerificationToken': token }, 
            data: Agency,
            async: false,
            success: function (result) {
                hideLoader();
                if (result == 'success') {
                    SuccessMsg("Success", "Record Added successfully", "success");
                    $('#AgencyModal').modal('hide');
                }
                else if (result == 'failed')
                {
                    $('#AgencyModal').modal('hide');
                    ErrorMsg('Error', "Failed to Add New Agency", '');
                }
                let table = $('#agencyTable').DataTable();
                table.destroy();
                loadAgenciesData();
                clearAgencyData();
            },
            failure: function (response) {
                hideLoader();
                ErrorMsg('Error', response, '');
            },
            error: function (xhr, status, error) {
                hideLoader();
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Failed", "Unauthorized Access");
                }
                else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                    ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                    $('#AgencyModal').modal('hide');
                    let table = $('#agencyTable').DataTable();
                    table.destroy();
                    loadAgenciesData();
                    clearAgencyData();
                }
                else {
                    ErrorMsg("Failed", error, "error")
                }
            }
        });
}

function validateAgencyForm() {
    return !($('#agencyId').val() == "" || $('#agencyName').val() == "" || $('#agencyAddress1').val() == "" || $('#agencyCity').val() == "" || $('#agencyState').val() == "" || $('#agencyZipCode').val() == "" || $('#agencyFacilityName').val() == "" || $('.errormsg').text() != "");
}

function loadAgenciesData() {
    let facilityName = $('#agencyfacilityNameFilter').val();
    if (facilityName == 'Select Facility Name') {
        facilityName = null;
    }
    let agencyId = $('#agencyIdFilter').val();
    if (agencyId == 'Select Agency Id') {
        agencyId = null;
    }

    let isThinkAnewUser = getLoggedInUserInfo();
    let organizationId = "";
    if (!isThinkAnewUser) {
        organizationId = getOrganizationUserDetails();
    }

    let formData = {
        facilityName: facilityName,
        agencyId: agencyId,
        organizationId: organizationId
    };
    $('#agencyTable').DataTable(
        {
            ajax: {
                url: urls.AgencyManagement.GetAllAgenciesList,
                type: "POST",
                data: function (data) {
                    // You can pass additional data to the server if needed
                    data.searchValue = $('#agencyTable_filter input').val();
                    data.start = data.start || 0; // start parameter
                    data.length = data.length || 10; // length parameter
                    data.draw = data.draw || 1; // draw parameter
                    data.sortColumn = data.columns[data.order[0].column].data; // sort column
                    data.sortDirection = data.order[0].dir;
                    data.facilityName = formData.facilityName;
                    data.agencyId = formData.agencyId;
                    data.organizationId = formData.organizationId;
                },
                error: function (xhr, status, error) {
                    if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                        ErrorMsg("Failed", "Unauthorized Access");
                    }
                },
            },
            processing: true,
            serverSide: true,
            filter: true,
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
                { data: "agencyId", name: "Agency Id" },
                { data: "agencyName", name: "Agency Name" },
                { data: "facilityName", name: "Facility Name" },
                { data: "state", name: "State" },
                {
                    data: null, orderable: false,
                    "render": function (data, type, full, row) {
                        return "<a href='#' onclick=editAgency('" + full.agencyId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Edit'><i class='ri-edit-2-line fs-6 pe-2 text-info'></i></a>"
                            + " " + "<a href='#' onclick=deleteAgency('" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
                    }
                }
            ]
        }
    );
}

function clearFilter() {
    $('#agencyfacilityNameFilter').val(null).trigger('change');
    $('#agencyIdFilter').val(null).trigger('change');
}

function editAgency(agencyId) {
    clearAgencyData();
    showLoader();
    $.ajax({
        url: urls.AgencyManagement.EditAgency,
        type: 'GET',
        data: { agencyId: agencyId },
        success: function (result) {
            hideLoader();
            $('#AgencyModal').modal('show');
            draggableModel('#AgencyModal');
            $('#Id').val(result.id);
            $('#agencyId').val(result.agencyId);
            $('#agencyName').val(result.agencyName);
            $('#agencyAddress1').val(result.address1);
            $('#agencyAddress2').val(result.address2);
            $('#agencyCity').val(result.city);
            $('#agencyState').val(result.state).trigger('change');
            $('#agencyZipCode').val(result.zipCode);
            $('#agencyFacilityName').val(result.facilityId).trigger('change');
            $('#AgencyModalLabel').text("Update Agency");
            $('#btnSaveAgency').hide();
            $('#btnUpdateAgency').show();
            $('#agencyId').prop('disabled', true);
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

function updateAgency() {
    let token = $('input[name="__RequestVerificationToken"]').val();
    let Agency = {
        Id: $('#Id').val(),
        AgencyId: $('#agencyId').val(),
        AgencyName: $('#agencyName').val(),
        Address1: $('#agencyAddress1').val(),
        Address2: $('#agencyAddress2').val(),
        City: $('#agencyCity').val(),
        State: $('#agencyState').val(),
        ZipCode: $('#agencyZipCode').val(),
        OrganizationId: $('#agencyOrganizationName').val(),
        FacilityId: $('#agencyFacilityName').val()
    };
    showLoader();
    $.ajax({
        url: urls.AgencyManagement.UpdateAgency,
        type: 'POST',
        headers: { 'RequestVerificationToken': token }, 
        data: Agency,
        async: false,
        success: function (result) {
            hideLoader();
            if (result == 'success') {
                SuccessMsg("Success", "Record Updated successfully", "success");
                $('#AgencyModal').modal('hide');
            }
            else {
                $('#AgencyModal').modal('hide');
                ErrorMsg("Failed", error, "error")
            }
            let table = $('#agencyTable').DataTable();
            table.destroy();
            loadAgenciesData();
            clearAgencyData();
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

function deleteAgency(agencyId) {
    let token = $('input[name="__RequestVerificationToken"]').val();
    ConfirmMsg('Agency Management', 'Are you sure want to delete this agency?', 'Continue', event, function () {
        $.ajax({
            url: urls.AgencyManagement.DeleteAgency,
            type: 'POST',
            headers: { 'RequestVerificationToken': token }, 
            data: { agencyId: agencyId },
            success: function (result) {
                if (result == 'success') {
                    SuccessMsg("Success", "Record Deleted successfully", "success");
                }
                else {
                    ErrorMsg("Failed", error, "error")
                }
                let table = $('#agencyTable').DataTable();
                table.destroy();
                loadAgenciesData();
            },
            error: function (xhr, status, error) {
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Failed", "Unauthorized Access");
                }
                else {
                    ErrorMsg("Failed", error, "error")
                }
            }
        });
    });

}

function initSelect2ForAgency() {
    $('#agencyState').select2({
        placeholder: "Select State Name",
        dropdownParent: $('#selectDivState'),
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#agencyFacilityName').select2({
        placeholder: 'Select Facility Name',
        dropdownParent: $('#facilityIdSelectDiv'),
        multiple: true,
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#agencyfacilityNameFilter').select2({
        placeholder: 'Select Facility Name',
        width: '100%'
    }).on('select2:close', function () {
        let placeholderText = 'Select options'; // Replace with your desired placeholder text
        if ($(this).val() == null) {
            $(this).val('').trigger('change');
            $(this).next('.select2-container').find('.select2-search__field').attr('placeholder', placeholderText);
        }
    });

    $('#agencyIdFilter').select2({
        placeholder: 'Select Agency Id',
        width: '100%'
    }).on('select2:close', function () {
        let placeholderText = 'Select options'; // Replace with your desired placeholder text
        if ($(this).val() == null) {
            $(this).val('').trigger('change');
            $(this).next('.select2-container').find('.select2-search__field').attr('placeholder', placeholderText);
        }
    });
}

function allowOnlyDigits(event) {
    let key = event.key;
    if (/^\d$/.test(key) || key === 'Backspace' || key === 'Tab') {
        return;
    }
    event.preventDefault();
}

function isAlphaNumericOnly(evt) {
    let charCode = (evt.which) ? evt.which : evt.keyCode;
    return !!((charCode > 47 && charCode < 58) || (charCode > 64 && charCode < 91) || (charCode > 96 && charCode < 123));
}


function isAlphaNumeric(evt) {
    let charCode = (evt.which) ? evt.which : evt.keyCode;
    return !!((charCode > 47 && charCode < 58) || (charCode > 64 && charCode < 91) || (charCode > 96 && charCode < 123) || charCode === 32);
}

function validateInputOnBlur(inputElement) {
    let inputValue = inputElement.value;
    let saveButton = $('#btnSaveAgency');
    let updateButton = $('#btnUpdateAgency');
    let inputAgencyName = document.getElementById('agencyName');
    let hasCharacter = /[a-zA-Z]/.test(inputValue); // Check for at least one character
    if (!(hasCharacter)) {
        $('#AgencyName_Error').text("Input is invalid. It should contain at least one character");
        inputAgencyName.focus();
        saveButton.prop('disabled', true); // Disable the button
        updateButton.prop('disabled', true); // Disable the button
    }
    else if ($('#AgencyName_Error').text() != "")
    {
        inputAgencyName.focus();
        saveButton.prop('disabled', true); // Disable the button
        updateButton.prop('disabled', true); // Disable the button
    }
    else {
        $('#AgencyName_Error').text('')
        saveButton.prop('disabled', false); // Disable the button
        updateButton.prop('disabled', false); // Disable the button
    }
    checkForUniqueAgencyName();
}

function restrictInput(event) {
    let pattern = /^[A-Za-z\-,''\s]+$/;
    let key = event.key;
    if (pattern.test(key) || key === 'Backspace') {
        return;
    }
    event.preventDefault();
}

function restrictSpecialChars(event) {
    let pattern = /^[a-zA-Z0-9\s\-,'.']+$/;
    let key = event.key;
    if (pattern.test(key) || key === 'Backspace') {
        return;
    }
    event.preventDefault();
}

function handleClick(event) {

    let accordion = document.getElementById('accordionFlushExample');
    let myDiv = document.getElementById('flush-collapseOne');
    let target = event.target;
    if (!accordion.contains(target)) {
        myDiv.classList.remove('show');
    }
}

function checkForUniqueAgencyName() {
    let agencyName = $('#agencyName').val();
    let agencyId = $('#agencyId').val();
    let saveButton = $('#btnSaveAgency');
    let updateButton = $('#btnUpdateAgency');
    let inputAgencyName = document.getElementById('agencyName');
    $.ajax({
        url: urls.AgencyManagement.CheckForAgencyName,
        type: 'GET',
        data: { agencyName: agencyName, agencyId: agencyId },
        success: function (result) {
            if (result) {
                $('#AgencyName_Error').text("Agency Name already exists, please use another Name");
                inputAgencyName.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else if ($('#AgencyName_Error').text() != "") {
                inputAgencyName.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else {
                $('#AgencyName_Error').text('');
                saveButton.prop('disabled', false); // Disable the button
                updateButton.prop('disabled', false); // Disable the button
            }
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
        }
    });
}

function checkForUniqueAgencyId() {
    let agencyId = $('#agencyId').val();
    let saveButton = $('#btnSaveAgency');
    let updateButton = $('#btnUpdateAgency');
    let inputAgencyId = document.getElementById('agencyId');
    $.ajax({
        url: urls.AgencyManagement.CheckForAgencyId,
        type: 'GET',
        data: { agencyId: agencyId },
        success: function (result) {
            if (result) {
                $('#agencyId_Error').text("Agency Id " + agencyId + " already exists, please use another Id");
                inputAgencyId.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else if ($('#agencyId_Error').text() != "")
            {
                inputAgencyId.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else {
                $('#agencyId_Error').text('');
                saveButton.prop('disabled', false); // Disable the button
                updateButton.prop('disabled', false); // Disable the button
            }
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
        }
    });
}

function getAgencies() {
    let filterFacilityName = $('#agencyfacilityNameFilter').val();
    if (filterFacilityName == 'Select Facility Name') {
        filterFacilityName = null;
    }
    showLoader();
    if ($("#agencyfacilityNameFilter").val() != "") {
        $.ajax({
            type: 'Get',
            data: { facilityName: filterFacilityName },
            async: false,
            cache:false,
            url: urls.AgencyManagement.GetAgencyIdListByFacilityName,
            success: function (response) {
                hideLoader();
                let agencyIdFilter = $("#agencyIdFilter");
                agencyIdFilter.empty();
                agencyIdFilter.append('<option value="">- Select Agency Id -</option>');
                $.each(response, function (index, item) {
                    agencyIdFilter.append('<option value="' + item.text + '">' + item.text + '</option>');
                });
                agencyIdFilter.trigger("change");
            },
            failure: function () {
                hideLoader();
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
            },
            error: function () {
                hideLoader();
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
            }
        });
    }
}

function getLoggedInUserInfo() {
    let isThinkAnewUser = false;
    showLoader();
    $.ajax({
        url: urls.AgencyManagement.GetLoggedInUserInfo,
        type: 'GET',
        async: false,
        success: function (result) {
            hideLoader();
            if (result != null) {
                isThinkAnewUser = result;
            }
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

    return isThinkAnewUser;
}

function getOrganizationUserDetails() {
    let organizationId = null;
    showLoader();
    $.ajax({
        url: urls.AgencyManagement.GetOrganizationUserInfo,
        type: 'GET',
        async: false,
        success: function (result) {
            hideLoader();
            if (result != null) {
                organizationId = result;
            }
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

    return organizationId;
}


