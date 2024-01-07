function saveFacility() {
    let token = $('input[name="__RequestVerificationToken"]').val();
    let locationId = $('#facilityLocationId').val();
    let locationDescription = $('#facilityDescription').val();
    let Facility = {
        OrganizationId: $('#facilityOrganizationName').val(),
        FacilityID: $('#facilityID').val(),
        FacilityName: $('#facilityName').val(),
        Address1: $('#facilityAddress1').val(),
        Address2: $('#facilityAddress2').val(),
        City: $('#facilityCity').val(),
        State: $('#facilityState').val(),
        ZipCode: $('#facilityZipCode').val(),
        ApplicationId: $('#facilityApplicationName').val(),
        HasDeduction: $('#hasDeduction').is(':checked'),
        OverTimeDeduction: $('#overTimeValue').val(),
        RegularDeduction: $('#regularTimeValue').val(),
        LocationId: (locationId === '' || locationId === null) ? "" : locationId,
        locationDescription: (locationDescription === '' || locationDescription === null) ? "" : locationDescription
    };
    showLoader();
    $.ajax({
        url: urls.FacilityManagement.SaveFacility,
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: Facility,
        async: false,
        cache: false,
        success: function (result) {
            hideLoader();
            if (result == 'success') {
                SuccessMsg("Success", "Record Added successfully", "success");
                $('#FacilityModal').modal('hide');
            }
            else if (result == 'failed') {
                ErrorMsg("Failed", error, "error");
                $('#FacilityModal').modal('hide');
            }
            let table = $('#facilityTable').DataTable();
            table.destroy();
            loadFacilityData();
            clearFacilityData();
        },
        failure: function (response) {
            hideLoader();
            ErrorMsg('Error', "Something went wrong !Try again later", '');
        },
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
            else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                $('#FacilityModal').modal('hide');
                let table = $('#facilityTable').DataTable();
                table.destroy();
                loadFacilityData();
                clearFacilityData();
            }
            else {
                ErrorMsg("Failed", error, "error")
            }
        }
    });
}

function kronossaveFacility() {
    let token = $('input[name="__RequestVerificationToken"]').val();
    let Facility = {
        OrganizationId: $('#facilityOrganizationName').val(),
        FacilityID: $('#facilityID').val(),
        FacilityName: $('#facilityName').val(),
        Address1: $('#facilityAddress1').val(),
        Address2: $('#facilityAddress2').val(),
        City: $('#facilityCity').val(),
        State: $('#facilityState').val(),
        ZipCode: $('#facilityZipCode').val(),
        ApplicationId: $('#facilityApplicationName').val(),
        HasDeduction: $('#hasDeduction').is(':checked'),
        OverTimeDeduction: $('#overTimeValue').val(),
        RegularDeduction: $('#regularTimeValue').val()
    };
    showLoader();
    $.ajax({
        url: urls.FacilityManagement.SaveFacility,
        headers: { 'RequestVerificationToken': token },
        type: 'POST',
        data: Facility,
        async: false,
        cache: false,
        success: function (result) {
            hideLoader();
            if (result == 'success') {
                $('#KronosFacilityModal').modal('hide');
                $.ajax({
                    url: urls.FacilityManagement.DeleteByFacilityID,
                    type: 'POST',
                    data: { id: Facility.FacilityID }, // Correct data object
                    async: false,
                    cache: false,
                    success: function (newres) {
                        if (newres) {
                            SuccessMsgWithReload("Success", "Record Added successfully", "success");

                        } else {
                            ErrorMsg("Failed", "Failed to delete", "error");
                        }
                    },
                    error: function (xhr, status, error) {
                        ErrorMsg("Failed", error, "error");                        
                        $('#KronosFacilityModal').modal('hide');
                        clearFacilityData();
                    }
                });

            }

            else if (result == 'failed') {
                ErrorMsg("Failed", "error", "error");
            }
         

        },
        failure: function (response) {
            hideLoader();
            ErrorMsg('Error', "Something went wrong !Try again later", '');
            clearFacilityData();
        },
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
            else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                $('#KronosFacilityModal').modal('hide');

                clearFacilityData();
            }
            else {
                ErrorMsg("Failed", error, "error")
            }
            clearFacilityData();
        }
    });
}


function deleteFacility(id) {
    let token = $('input[name="__RequestVerificationToken"]').val();
    ConfirmMsg('Facility Management', 'Are you sure want to delete this facility?', 'Continue', event, function () {
        $.ajax({
            url: urls.FacilityManagement.DeleteFacility,
            type: 'POST',
            headers: { 'RequestVerificationToken': token },
            data: { facilityId: id },
            success: function (result) {
                if (result == 'success') {
                    SuccessMsg("Success", "Record Deleted successfully", "success");
                }
                else {
                    ErrorMsg("Failed", error, "error")
                }
                let table = $('#facilityTable').DataTable();
                table.destroy();
                loadFacilityData();
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

function editFacility(id) {
    clearFacilityData();
    showLoader();
    $.ajax({
        url: urls.FacilityManagement.EditFacility,
        type: 'GET',
        data: { facilityId: id },
        success: function (result) {
            hideLoader();
            $('#FacilityModal').modal('show');
            draggableModel('#FacilityModal');
            $('#id').val(result.id);
            $('#facilityID').val(result.facilityID);
            $('#facilityOrganizationName').val(result.organizationId).trigger('change');
            $('#facilityName').val(result.facilityName);
            $('#facilityAddress1').val(result.address1);
            $('#facilityAddress2').val(result.address2);
            $('#facilityCity').val(result.city);
            $('#facilityState').val(result.state).trigger('change');
            $('#facilityZipCode').val(result.zipCode);
            $('#facilityApplicationName').val(result.applicationId).trigger('change');
            $('#hasDeduction').val(result.hasDeduction);
        
            if (result.hasDeduction===true) {
                $('#hasDeduction').prop('checked',true);
                $("#regularTime").show();
                $("#overTime").show();
                $("#Duduction").show();
                $("#DefaultTime").show();
                $("#regularTimeValue").val(result.regularDeduction);
                $("#overTimeValue").val(result.overTimeDeduction);
            } else
            {
                $("#regularTime").hide();
                $("#overTime").hide();
                $("#Duduction").hide();
                $("#regularTimeValue").val('');
                $("#overTimeValue").val('');
            }
            $('#facilityLocationId').val(result.locationId);
            $('#facilityDescription').val(result.locationDescription);
            $('#FacilityModalLabel').text("Update Facility");
            $('#btnSaveFacility').hide();
            $('#btnUpdateFacility').show();
            $('#facilityID').prop('disabled', true);
            $('#facilityOrganizationName').prop('disabled', true);
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

function clearFacilityData() {

    $('#facilityOrganizationName').val(null).trigger('change');
    $('#facilityID').val('');
    $('#facilityName').val('');
    $('#facilityAddress1').val('');
    $('#facilityAddress2').val('');
    $('#facilityCity').val('');
    $('#facilityState').val('').trigger('change');
    $('#facilityZipCode').val('');
    $('#facilityApplicationName').val('').trigger('change');
    $('#facilityLocationId').val('');
    $('#facilityDescription').val('');

    $('.errormsg').text('');

    $('#FacilityModalLabel').text("Add Facility");
    $('#btnSaveFacility').show();
    $('#btnUpdateFacility').hide();
    $('#facilityID').prop('disabled', false);
    $('#facilityOrganizationName').prop('disabled', false);
    $('#regularTimeValue').val('');
    $('#overTimeValue').val('');
    $('#regularTime').hide();
    $('#overTime').hide();
    $('#Duduction').hide();
    $('#DefaultTime').prop('checked', false);
    $('#hasDeduction').prop('checked', false);     
}

function updateFacility() {
    let token = $('input[name="__RequestVerificationToken"]').val();
    let locationId = $('#facilityLocationId').val();
    let locationDescription = $('#facilityDescription').val();
    let Facility = {
        Id: $('#id').val(),
        OrganizationId: $('#facilityOrganizationName').val(),
        FacilityID: $('#facilityID').val(),
        FacilityName: $('#facilityName').val(),
        Address1: $('#facilityAddress1').val(),
        Address2: $('#facilityAddress2').val(),
        City: $('#facilityCity').val(),
        State: $('#facilityState').val(),
        ZipCode: $('#facilityZipCode').val(),
        ApplicationId: $('#facilityApplicationName').val(),
        LocationId: (locationId === '' || locationId === null) ? "" : locationId,
        locationDescription: (locationDescription === '' || locationDescription === null) ? "" : locationDescription
    };
    showLoader();
    $.ajax({
        url: urls.FacilityManagement.UpdateFacility,
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: Facility,
        async: false,
        success: function (result) {
            hideLoader();
            if (result == 'success') {
                SuccessMsg("Success", "Record Updated successfully", "success");
                $('#FacilityModal').modal('hide');
            }
            else {
                $('#FacilityModal').modal('hide');
                ErrorMsg("Failed", error, "error")
            }
            let table = $('#facilityTable').DataTable();
            table.destroy();
            loadFacilityData();
            clearFacilityData();
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
function checkDeduction() {
    let s = $("#regularTimeValue").val();
    let y = $("#overTimeValue").val();    

    if ($('#hasDeduction').is(':checked')) {
        if ($('#DefaultTime').is(':checked')) {
            $("#spHasDeduction").empty();
            return true;
        }
        else
        {
            if ($("#regularTimeValue").val() !== "" && $("#overTimeValue").val() != "") {
                $("#spHasDeduction").empty();
                return true;
            }
            else
            {
                $("#spHasDeduction").text("Please select default or Enter Deduction time");
                return false;
            }
        }
    } else {
        return true;
    }
}


function validateFacilityForm() {
    return !($('#facilityOrganizationName').val() == "" || $('#facilityID').val() == "" || $('#facilityName').val() == "" || $('#facilityAddress1').val() == "" || $('#facilityCity').val() == "" || $('#facilityState').val() == "" || $('#facilityZipCode').val() == "" || $('#facilityApplicationName').val() == "" || $('.errormsg').text() != "");
}

function initSelect2ForFacility() {
    $('#facilityOrganizationName').select2({
        placeholder: "Select Organization Name",
        dropdownParent: $('#organizationIdDiv'),
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#facilityState').select2({
        placeholder: "Select State Name",
        dropdownParent: $('#stateSelectDiv'),
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#facilityApplicationName').select2({
        placeholder: "Select Application Name",
        dropdownParent: $('#applicationIdDiv'),
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#facilityOrgName').select2({
        placeholder: "Select Organization Name",
        width: '100%'
    }).on('select2:close', function () {
        let placeholderText = 'Select options'; // Replace with your desired placeholder text
        if ($(this).val() == null) {
            $(this).val('').trigger('change');
            $(this).next('.select2-container').find('.select2-search__field').attr('placeholder', placeholderText);
        }
    });

    $('#facilityAppName').select2({
        placeholder: "Select Application Name",
        width: '100%'
    }).on('select2:close', function () {
        let placeholderText = 'Select options'; // Replace with your desired placeholder text
        if ($(this).val() == null) {
            $(this).val('').trigger('change');
            $(this).next('.select2-container').find('.select2-search__field').attr('placeholder', placeholderText);
        }
    });
}

function loadFacilityData() {
    let orgName = $('#facilityOrgName').val();
    let storedValue = sessionStorage.getItem("organizationName");
    if (storedValue != null) {
        $('#facilityOrgName').val(storedValue.trim()).trigger('change');
    }
    else {
        $('#facilityOrgName').val(orgName).trigger('change');
    }
    if (orgName == 'Select Organization Name')
        orgName = null;
    let appName = $('#facilityAppName').val();
    if (appName == 'Select Application Name')
        appName = null;
    else {
        $('#facilityAppName').val(appName).trigger('change');
    }
    let loggedInUser = getLoggedInUser();
    let isThinkAnewUser = getLoggedInUserInfo();
    let orgId = "";
    if (!isThinkAnewUser) {
        orgId = getOrganizationUserDetails();
    }
    let formData = {
        orgName: orgName,
        appName: appName,
        storedValue: storedValue,
        orgId: orgId
    };


    $('#facilityTable').DataTable({
        ajax: {
            url: urls.FacilityManagement.GetFacilitiesList,
            type: "POST",
            data: function (data) {
                // You can pass additional data to the server if needed
                data.searchValue = $('#facilityTable_filter input').val();
                data.start = data.start || 0; // start parameter
                data.length = data.length || 10; // length parameter
                data.draw = data.draw || 1; // draw parameter
                data.sortColumn = data.columns[data.order[0].column].data; // sort column
                data.sortDirection = data.order[0].dir;
                data.orgName = formData.orgName;
                data.appName = formData.appName;
                data.storedValue = formData.storedValue;
                data.orgId = formData.orgId;
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
            { data: "facilityID", name: "Facility Id" },
            { data: "facilityName", name: "Facility Name" },
            { data: "organizationName", name: "Organization Name" },
            { data: "appId", name: "Application Name" },
            { data: "state", name: "State" },
            {
                data: "locationId", name: "Location Id"
            },
            {
                data: null, orderable: false,
                "render": function (data, type, full, row) {
                    return "<a href='#' onclick=editFacility('" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Edit'><i class='ri-edit-2-line fs-6 pe-2 text-info'></i></a>"
                        + " " + "<a href='#' onclick=deleteFacility('" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
                }
            },
            {
                orderable: false,
                "render":
                    function (data, type, full, row) {
                        if (loggedInUser) {
                            return "<a href='#' onclick=addUser('" + full.organizationId + "','" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Add User' ><i class='ri-add-circle-line fs-6 pe-2 text-primary'></i></a>"
                        }
                        else {
                            return "<a href='#' onclick=addUser('" + full.organizationId + "','" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Add User' class='pe-none'><i class='ri-add-circle-line fs-6 pe-2 text-muted'></i></a>"
                        }
                    }
            }
        ]
    });
}

function addUser(organizationId, facilityId) {
    let AddUserId = 'btnAddUser';
    let fromFacility = true;
    sessionStorage.setItem("organizationId", organizationId);
    sessionStorage.setItem("facilityId", facilityId);
    window.location.href = urls.UserManagement.GetValues + '?AddUserId=' + AddUserId + '&fromFacility=' + fromFacility;
}

function clearFilter() {
    $('#facilityOrgName').val(null).trigger('change');
    $('#facilityAppName').val(null).trigger('change');
    sessionStorage.removeItem("organizationName");
}

function loadModalPopUp() {
    $('#btnAddFacility').trigger('click');
    let storedValue = sessionStorage.getItem("organizationId").trim();
    $('#facilityOrganizationName').val(storedValue).trigger('change');
}

function getLoggedInUser() {
    let isSuperAdmin;
    showLoader();
    $.ajax({
        url: '/Facility/CheckUserRole',
        type: 'GET',
        async: false,
        dataType: 'json',
        success: function (result) {
            hideLoader();
            if (result) {
                isSuperAdmin = true;
            }
            else {
                isSuperAdmin = false;
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

    return isSuperAdmin;
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
    if (inputValue == '') {
        $('#facilityName_Error').text("Please Enter a  Facility Name");
        return;
    }
    let saveButton = $('#btnSaveFacility');
    let updateButton = $('#btnUpdateFacility');
    let inputFacilityName = document.getElementById('facilityName');
    let hasCharacter = /[a-zA-Z]/.test(inputValue); // Check for at least one character
    if (!(hasCharacter)) {
        $('#facilityName_Error').text("Input is invalid. It should contain at least one character");
        inputFacilityName.focus();
        saveButton.prop('disabled', true); // Disable the button
        updateButton.prop('disabled', true); // Disable the button
    }
    else if ($('#facilityName_Error').text() != "") {
        inputFacilityName.focus();
        saveButton.prop('disabled', true); // Disable the button
        updateButton.prop('disabled', true); // Disable the button
    }
    else {
        $('#facilityName_Error').text('');
        saveButton.prop('disabled', false); // Disable the button
        updateButton.prop('disabled', false); // Disable the button
    }
    checkForUniqueFacilityName();
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

function checkForUniqueFacilityName() {
    let facilityName = $('#facilityName').val();
    let facilityId = $('#facilityID').val();
    let saveButton = $('#btnSaveFacility');
    let updateButton = $('#btnUpdateFacility');
    let inputFacilityName = document.getElementById('facilityName');
    $.ajax({
        url: urls.FacilityManagement.CheckForFacilityName,
        type: 'GET',
        data: { facilityName: facilityName, facilityId: facilityId },
        success: function (result) {
            if (result) {
                $('#facilityName_Error').text("Facility Name already exists, please use another Name");
                inputFacilityName.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else if ($('#facilityName_Error').text() != "") {
                inputFacilityName.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else {
                $('#facilityName_Error').text('');
                saveButton.prop('disabled', false); // Disable the button
                updateButton.prop('disabled', false); // Disable the button
            }
        },
        error: function (xhr, status, error) {
            ErrorMsg("Failed", error, "error")
        }
    });
}

function checkForUniqueId() {
    let facilityId = $('#facilityID').val();
    if (facilityId == '') {
        $('#facilityId_Error').text("Please Enter Facility Id"); return;
    }
    let saveButton = $('#btnSaveFacility');
    let updateButton = $('#btnUpdateFacility');
    let inputFacilityId = document.getElementById('facilityID');
    $.ajax({
        url: urls.FacilityManagement.CheckForFacilityId,
        type: 'GET',
        data: { facilityId: facilityId },
        success: function (result) {
            if (result) {
                $('#facilityId_Error').text("Facility Id already exists, please use another Id");
                inputFacilityId.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else if ($('#facilityId_Error').text() != "") {
                inputFacilityId.focus();
                saveButton.prop('disabled', true); // Disable the button
                updateButton.prop('disabled', true); // Disable the button
            }
            else {
                $('#facilityId_Error').text('');
                saveButton.prop('disabled', false); // Disable the button
                updateButton.prop('disabled', false); // Disable the button
            }
        },
        error: function (xhr, status, error) {
            ErrorMsg("Failed", error, "error")
        }
    });
}

function openModal(id,orgId, appId) {
    clearFacilityData();
    let Fid = id;
    let OrgId = orgId;
    $("#facilityOrganizationName").val(OrgId);
    $("#facilityOrganizationName").trigger('change');
    $("#facilityApplicationName").val(appId);
    $("#facilityApplicationName").trigger('change');
    $('#facilityID').val(Fid);
    $('#facilityID').prop('disabled', true);
    $("#facilityApplicationName").prop('disabled', true);
    $("#facilityOrganizationName") .prop('disabled', true);

}

function getOrgServiceType() {
    let orgId = $('#facilityOrganizationName').val();
    if (orgId != '' && orgId != null) {
        $.ajax({
            url: urls.FacilityManagement.GetOrganizationServiceDetails,
            type: 'GET',
            data: { organizationId: orgId },
            success: function (result) {
                if (result != '' && result.organizationId > 0) {
                    document.getElementById("LocationInformationDiv").style.display = "block";
                    $('#facilityApplicationName').val('3').trigger('change');
                }
                else {
                    document.getElementById("LocationInformationDiv").style.display = "none";
                    $('#facilityLocationId').val('');
                    $('#facilityLocationDescription').val('');
                }
            },
            error: function (xhr, status, error) {
                ErrorMsg("Failed", error, "error")
            }
        });
    }
    else {
        document.getElementById("LocationInformationDiv").style.display = "none";
        $('#facilityLocationId').val('');
        $('#facilityLocationDescription').val('');
    }
}

function getOrganizationUserDetails() {
    let organizationId = null;
    showLoader();
    $.ajax({
        url: urls.FacilityManagement.GetOrganizationUserInfo,
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
