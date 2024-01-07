function deleteOrganization(id) {
    let token = $('input[name="__RequestVerificationToken"]').val();
    ConfirmMsg('Organization Management', 'Are you sure want to delete this organization?', 'Continue', event, function () {
        $.ajax({
            url: urls.OrganizationManagement.DeleteOrganization,
            type: 'POST',
            headers: { 'RequestVerificationToken': token },
            data: { organizationId: id },
            async: false,
            success: function (result) {
                if (result == 'success') {
                    SuccessMsg("Success", "Record Deleted successfully", "success");
                }
                else {
                    $('#OrganizationModal').modal('hide');
                    ErrorMsg("Failed", error, "error")
                }
                let table = $('#orgTable').DataTable();
                table.destroy();
                loadOrganizationData();
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

function editOrganization(id) {
    clearOrganizationData();
    showLoader();
    $.ajax({
        url: urls.OrganizationManagement.EditOrganization,
        type: 'GET',
        data: { organizationId: id },
        success: function (result) {
            hideLoader();
            $('#OrganizationModal').modal('show');
            draggableModel('#OrganizationModal');
            $('#organizationID').val(result.organizationID);
            $('#organizationName').val(result.organizationName);
            $('#organizationEmail').val(result.organizationEmail);
            $('#organizationPrimaryPhone').val(result.primaryPhone);
            $('#organizationSecondaryPhone').val(result.secondaryPhone);
            $('#organizationAddress1').val(result.address1);
            $('#organizationAddress2').val(result.address2);
            $('#organizationAddress').val(result.address);
            $('#organizationCity').val(result.city);
            $('#organizationState').val(result.state).trigger('change');
            $('#organizationZipCode').val(result.zipCode);
            let selectedServiceTypes = [];
            if (result.serviceDetails != null) {
                for (const element of result.serviceDetails) {
                    selectedServiceTypes.push(element.serviceType);
                    document.getElementById("ServiceSelectionTypeDiv").style.display = "block";
                    let checkbox = document.getElementById("bandwidthAccessChkBox");
                    checkbox.checked = true;
                    if (element.serviceType == '0') {
                        $('#organizationMasterAccountId').val(element.masterAccountId);
                        $('#organizationMasterAccountName').val(element.masterAccountId);
                        $('#organizationSubAccountId').val(element.subAccountId);
                    }
                }
                $('#ServiceSelectBox').val(selectedServiceTypes).trigger('change');
            }
            $('#OrganizationModalLabel').text("Update Organization");
            $('#btnSaveOrganization').hide();
            $('#btnUpdateOrganization').show();
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

function saveOrganization() {
    let token = $('input[name="__RequestVerificationToken"]').val();
    let masterAccountId = $('#organizationMasterAccountId').val();
    let subAccountId = $('#organizationSubAccountId').val();
    let serviceType = $('#ServiceSelectBox').val();
    if (!serviceType || serviceType.length === 0) {
        // If the list is empty or null, assign a default value
        serviceType = "";
    }
    let Organization = {
        OrganizationName: $('#organizationName').val(),
        OrganizationEmail: $('#organizationEmail').val(),
        PrimaryPhone: $('#organizationPrimaryPhone').val(),
        SecondaryPhone: $('#organizationSecondaryPhone').val(),
        Address1: $('#organizationAddress1').val(),
        Address2: $('#organizationAddress2').val(),
        Country: 'USA',
        City: $('#organizationCity').val(),
        State: $('#organizationState').val(),
        ZipCode: $('#organizationZipCode').val(),
        ServiceType: serviceType,
        MasterAccountId: (masterAccountId === '' || masterAccountId === null) ? '-' : masterAccountId,
        SubAccountId: (subAccountId === '' || subAccountId === null) ? '-' : subAccountId,
    };
    showLoader();
    $.ajax({
        url: urls.OrganizationManagement.SaveOrganization,
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: Organization,
        async: false,
        success: function (result) {
            hideLoader();
            if (result == 'success') {
                SuccessMsg("Success", "Record Added successfully", "success");
                $('#OrganizationModal').modal('hide');
            }
            else if (result == "failed") {
                ErrorMsg("Failed", "error", "error")
                $('#OrganizationModal').modal('hide');
            }
            let table = $('#orgTable').DataTable();
            table.destroy();
            loadOrganizationData();
            clearOrganizationData();
        },
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
            else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                $('#OrganizationModal').modal('hide');
                let table = $('#orgTable').DataTable();
                table.destroy();
                loadOrganizationData();
                clearOrganizationData();
            }
            else {
                ErrorMsg("Failed", error, "error");
            }
        }
    });
}

function updateOrganization() {
    let token = $('input[name="__RequestVerificationToken"]').val();
    let masterAccountId = $('#organizationMasterAccountId').val();
    let subAccountId = $('#organizationSubAccountId').val();
    let serviceType = $('#ServiceSelectBox').val();
    if (!serviceType || serviceType.length === 0) {
        serviceType = "";
    }
    let Organization = {
        OrganizationID: $('#organizationID').val(),
        OrganizationName: $('#organizationName').val(),
        OrganizationEmail: $('#organizationEmail').val(),
        PrimaryPhone: $('#organizationPrimaryPhone').val(),
        SecondaryPhone: $('#organizationSecondaryPhone').val(),
        Address1: $('#organizationAddress1').val(),
        Address2: $('#organizationAddress2').val(),
        Country: 'USA',
        City: $('#organizationCity').val(),
        State: $('#organizationState').val(),
        ZipCode: $('#organizationZipCode').val(),
        ServiceType: serviceType,
        MasterAccountId: (masterAccountId === '' || masterAccountId === null) ? '-' : masterAccountId,
        SubAccountId: (subAccountId === '' || subAccountId === null) ? '-' : subAccountId
    };
    showLoader();
    $.ajax({
        url: urls.OrganizationManagement.UpdateOrganization,
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: Organization,
        async: false,
        success: function (result) {
            hideLoader();
            if (result == 'success') {
                SuccessMsg("Success", "Record Updated successfully", "success");
                $('#OrganizationModal').modal('hide');
            }
            else {
                $('#OrganizationModal').modal('hide');
                ErrorMsg("Failed", error, "error")
            }
            let table = $('#orgTable').DataTable();
            table.destroy();
            loadOrganizationData();
            clearOrganizationData();
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

function clearOrganizationData() {
    $('#organizationID').val('');
    $('#organizationName').val('');
    $('#organizationEmail').val('');
    $('#organizationPrimaryPhone').val('');
    $('#organizationSecondaryPhone').val('');
    $('#organizationAddress1').val('');
    $('#organizationAddress2').val('');
    $('#organizationCity').val('');
    $('#organizationState').val(null).trigger('change');
    $('#organizationZipCode').val('');
    let checkbox = document.getElementById("bandwidthAccessChkBox");
    checkbox.checked = false;
    $('#ServiceSelectBox').val(null).trigger('change');
    document.getElementById("ServiceSelectionTypeDiv").style.display = "none";
    $('#organizationMasterAccountId').val('');
    $('#organizationMasterAccountName').val('')
    $('#organizationSubAccountId').val('')
    $('.errormsg').text('');

    $('#OrganizationModalLabel').text("Add Organization");
    $('#btnSaveOrganization').show();
    $('#btnUpdateOrganization').hide();
}


function validateOrganizationForm() {
    let checkbox = document.getElementById('bandwidthAccessChkBox');
    if (checkbox.checked) {
        let serviceType = $('#ServiceSelectBox').val();
        if (serviceType.length == 0) {
            return !($('#organizationName').val() == "" || $('#organizationEmail').val() == "" || $('#organizationPrimaryPhone').val() == "" || $('#organizationAddress1').val() == "" || $('#organizationCity').val() == "" || $('#organizationState').val() == "" || $('#organizationZipCode').val() == "" || $('.errormsg').text() != "" || $('#ServiceSelectBox').val() == "");
        }
        else if (serviceType[0] == 0) {
            return !($('#organizationName').val() == "" || $('#organizationEmail').val() == "" || $('#organizationPrimaryPhone').val() == "" || $('#organizationAddress1').val() == "" || $('#organizationCity').val() == "" || $('#organizationState').val() == "" || $('#organizationZipCode').val() == "" || $('.errormsg').text() != "" || $('#organizationSubAccountId').val() == "" || $('#organizationMasterAccountId').val() == "" || $('#organizationMasterAccountName').val() == "");
        }
        else {
            return !($('#organizationName').val() == "" || $('#organizationEmail').val() == "" || $('#organizationPrimaryPhone').val() == "" || $('#organizationAddress1').val() == "" || $('#organizationCity').val() == "" || $('#organizationState').val() == "" || $('#organizationZipCode').val() == "" || $('.errormsg').text() != "");
        }
    }
    else {
        return !($('#organizationName').val() == "" || $('#organizationEmail').val() == "" || $('#organizationPrimaryPhone').val() == "" || $('#organizationAddress1').val() == "" || $('#organizationCity').val() == "" || $('#organizationState').val() == "" || $('#organizationZipCode').val() == "" || $('.errormsg').text() != "");
    }
}

function loadOrganizationData() {
    let serviceType = $('#ServiceSelectBoxFilter option:selected').text();;
    if (serviceType == 'Select Service Type')
        serviceType = null;
    let formData = {
        serviceType: serviceType
    };
    $('#orgTable').DataTable(
        {
            ajax: {
                url: urls.OrganizationManagement.GetOrganizationsList,
                type: "POST",
                data: function (data) {
                    // You can pass additional data to the server if needed
                    data.searchValue = $('#orgTable_filter input').val();
                    data.start = data.start || 0; // start parameter
                    data.length = data.length || 10; // length parameter
                    data.draw = data.draw || 1; // draw parameter
                    data.sortColumn = data.columns[data.order[0].column].data; // sort column
                    data.sortDirection = data.order[0].dir;
                    data.serviceType = formData.serviceType;
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
                { data: "organizationName", name: "Organization Name" },
                { data: "organizationEmail", name: "Organization Email" },
                { data: "primaryPhone", name: "Primary Phone" },
                { data: "state", name: "State" },
                { data: "serviceTypes", name: "Service Types", orderable: false },
                {
                    data: null, orderable: false,
                    "render": function (data, type, full, row) {
                        return "<a href='#' onclick=editOrganization('" + full.organizationID + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Edit' class='tooltip-black'><i class='ri-edit-2-line fs-6 pe-2 text-info'></i></a>"
                            + " " + "<a href='#' onclick=deleteOrganization('" + full.organizationID + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
                    }
                },
                {
                    data: null, orderable: false,
                    render: function (data, type, full, row) {
                        return "<a href='#' onclick=viewFacilities('" + encodeURIComponent(full.organizationName) + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='View Facilities' style='color: black' ><i class='ri-eye-line fs-6 pe-2 text-success'></i></a>"
                            + " " + "<a href='#' onclick=addNewFacility('" + full.organizationID + "','" + encodeURIComponent(full.organizationName) + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Add Facility' style='color: black' ><i class='ri-add-circle-line fs-6 pe-2 text-primary'></i></a >";
                    }
                }
            ]
        }
    );

}
function viewFacilities(organizationName) {
    sessionStorage.setItem("organizationName", decodeURIComponent(organizationName));
    let isFromOrganization = true;
    window.location.href = urls.FacilityManagement.GetValues + '?isFromOrganization=' + isFromOrganization;
}

function addNewFacility(organizationId, organizationName) {
    let buttonId = 'btnAddFacility';
    let isFromOrganization = true;
    sessionStorage.setItem("organizationId", organizationId);
    sessionStorage.setItem("organizationName", decodeURIComponent(organizationName));
    window.location.href = urls.FacilityManagement.GetValues + '?buttonId=' + buttonId + '&isFromOrganization=' + isFromOrganization;

}

function initSelect2ForOrganization() {
    $('#organizationState').select2({
        dropdownParent: $('#selectDiv'),
        placeholder: "Select State Name",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#ServiceSelectBox').select2({
        dropdownParent: $('#ServiceTypeDiv'),
        placeholder: "Select Service Type",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#ServiceSelectBoxFilter').select2({
        placeholder: "Select Service Type",
        width: '100%'
    });
}

function allowOnlyDigits(event) {
    let key = event.key;
    if (/^\d$/.test(key) || key === 'Backspace' || key === 'Tab') {
        return;
    }
    event.preventDefault();
}

function checkForUniqueEmail() {
    let emailId = $('#organizationEmail').val();
    let organizationId = $('#organizationID').val();
    let saveButton = $('#btnSaveOrganization');
    let updateButton = $('#btnUpdateOrganization');
    let inputOrganizationEmail = document.getElementById('organizationEmail');
    $.ajax({
        url: urls.OrganizationManagement.CheckForEmailId,
        type: 'GET',
        data: { emailId: emailId, organizationId: organizationId },
        success: function (result) {
            if (result) {
                $('#organizationEmail_Error').text("Organization Email Id already exists, please use another Email Id");
                inputOrganizationEmail.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else {
                $('#organizationEmail_Error').text('');
                saveButton.prop('disabled', false); // Disable the button
                updateButton.prop('disabled', false); // Disable the button
            }
        },
        error: function (xhr, status, error) {
            ErrorMsg("Failed", error, "error")
        }
    });
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

function isAlphaNumeric(evt) {
    let charCode = (evt.which) ? evt.which : evt.keyCode;
    return !!((charCode > 47 && charCode < 58) || (charCode > 64 && charCode < 91) || (charCode > 96 && charCode < 123) || charCode === 32);
}

function validateInputOnBlur(inputElement) {
    let inputValue = inputElement.value;
    let inputOrganizationName = document.getElementById('organizationName');
    let saveButton = $('#btnSaveOrganization');
    let updateButton = $('#btnUpdateOrganization');
    let hasCharacter = /[a-zA-Z]/.test(inputValue); // Check for at least one character
    if (!(hasCharacter)) {
        $('#organizationName_Error').text("Input is invalid. It should contain at least one character");
        inputOrganizationName.focus();
        saveButton.prop('disabled', true); // Disable the button
        updateButton.prop('disabled', true); // Disable the button
    }
    else {
        $('#organizationName_Error').text('');
        saveButton.prop('disabled', false); // Disable the button
        updateButton.prop('disabled', false); // Disable the button
    }
    checkForUniqueOrganizationName();
}

function checkForUniqueOrganizationName() {
    let organizationName = $('#organizationName').val();
    let organizationId = $('#organizationID').val();
    let saveButton = $('#btnSaveOrganization');
    let updateButton = $('#btnUpdateOrganization');
    let inputOrganizationName = document.getElementById('organizationName');
    $.ajax({
        url: urls.OrganizationManagement.CheckForOrganizationName,
        type: 'GET',
        data: { organizationName: organizationName, organizationId: organizationId },
        success: function (result) {
            if (result) {
                $('#organizationName_Error').text("Organization Name already exists, please use another Name");
                inputOrganizationName.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else {
                $('#organizationName_Error').text('');
                saveButton.prop('disabled', false); // Disable the button
                updateButton.prop('disabled', false); // Disable the button
            }
        },
        error: function (xhr, status, error) {
            ErrorMsg("Failed", error, "error")
        }
    });
}

$(function () {
    $('[data-bs-toggle="tooltip"]').tooltip()
})

function getMasterAccountId() {
    let serviceType = $('#ServiceSelectBox').val();
    if (serviceType[0] == 0) {
        $('#ServiceSelectionDiv').show();
        $.ajax({
            url: urls.OrganizationManagement.GetMasterAccountDetails,
            type: 'GET',
            success: function (result) {
                if (result != '') {
                    $("#organizationMasterAccountName").val(result.accountName);
                    $("#organizationMasterAccountId").val(result.accountId);
                }
                else {
                    $("#organizationMasterAccountId").val('');
                    $("#organizationMasterAccountName").val('');
                }
            },
            error: function (xhr, status, error) {
                ErrorMsg("Failed", error, "error")
            }
        });
    }
    else {
        document.getElementById("ServiceSelectionDiv").style.display = "none";
        $('#organizationMasterAccountId').val('');
        $('#organizationMasterAccountName').val('');
        $('#organizationSubAccountId').val('');
    }
}

function showServiceSelectionTypeDiv() {
    document.getElementById("ServiceSelectionTypeDiv").style.display = "block";
}

function clearFilter() {
    $('#ServiceSelectBoxFilter').val(null).trigger('change');
}

function checkForSubAccoutId() {
    let organizationId = $('#organizationID').val();
    let subAccountId = $('#organizationSubAccountId').val();
    let saveButton = $('#btnSaveOrganization');
    let updateButton = $('#btnUpdateOrganization');
    let inputSubAccountId = document.getElementById('organizationSubAccountId');
    if (subAccountId !== '' || subAccountId !== null) {
        $.ajax({
            url: urls.OrganizationManagement.CheckForSubAccountId,
            type: 'GET',
            data: { organizationId: organizationId, subAccountId: subAccountId },
            success: function (result) {
                if (result) {
                    $('#organizationSubAccountId_Error').text("Sub Account Id already exists, please use another Sub Account Id");
                    inputSubAccountId.focus();
                    saveButton.prop('disabled', true); // Disable the button
                    updateButton.prop('disabled', true); // Disable the button
                }
                else {
                    $('#organizationSubAccountId_Error').text('');
                    saveButton.prop('disabled', false); // Disable the button
                    updateButton.prop('disabled', false); // Disable the button
                }
            },
            error: function (xhr, status, error) {
                ErrorMsg("Failed", error, "error")
            }
        });
    }
}


