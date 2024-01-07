function updateKronosForm() {

    let formData = {
        KronosHostGLC: $("#KronosHostGLC").val(),
        KronosPortGLC: $("#KronosPortGLC").val(),
        KronosUserNameGLC: $("#KronosUserNameGLC").val(),
        KronosPasswordGLC: $("#KronosPasswordGLC").val(),
        StorageConnectionString: $("#StorageConnectionString").val(),
        KronosDecryptionPasswordGLC: $("#KronosDecryptionPasswordGLC").val(),
        KronosDecryptOrEncryptKeyBlobNameGLC: $("#KronosDecryptOrEncryptKeyBlobNameGLC").val(),
        KronosPunchExportGLC: $("#KronosPunchExportGLC").val(),
        OrganizationId: $("#OrganizationId").val()
    };
    showLoader();
    if (validatecredentials()) {
        $.ajax({
            url: urls.LabourFileMoverurls.UpdateKronosForm,
            type: "POST",
            data: formData,
            success: function (result) {
                hideLoader();
                if (result == "Success") {
                    SuccessMsg("Success", "Record Added successfully", "success");
                    window.location.href = urls.LabourFileMoverurls.OrgCredsHome;
                }
                else if (result == "Data Exists") {
                    WarningMsg("Alert!", "Record Added successfully", "success");
                }
                else {
                    ErrorMsg("Error", "Failed to Update", '');
                }
            },

            error: function () {
                hideLoader();
                ErrorMsg("Error", "Something went wrong", '');
            }
        });
    }
    else {
        hideLoader();
    }
}
function checkOrgCreds() {
    let Id = $("#OrganizationId").val();

    if (Id !== '' && Id !== undefined) {
        $.ajax({
            url: urls.LabourFileMoverurls.OrgCredsCheck,
            type: "POST",
            data: { id: Id },
            success: function (result) {
                if (!result) {
                    WarningMsg("Alert!", "Record exists", "success");
                    $("#OrganizationId").val("");
                    $("#OrganizationId").trigger('change');
                }

            },
            error: function () {

            }
        });
    }

}

function combinedFormSubmit() {

    let combinedFormData = {
        KronosHostGLC: $("#KronosHostGLC").val(),
        KronosPortGLC: $("#KronosPortGLC").val(),
        KronosUserNameGLC: $("#KronosUserNameGLC").val(),
        KronosPasswordGLC: $("#KronosPasswordGLC").val(),
        StorageConnectionString: $("#StorageConnectionString").val(),
        KronosDecryptionPasswordGLC: $("#KronosDecryptionPasswordGLC").val(),
        KronosDecryptOrEncryptKeyBlobNameGLC: $("#KronosDecryptOrEncryptKeyBlobNameGLC").val(),
        KronosPunchExportGLC: $("#KronosPunchExportGLC").val(),
        OrganizationId: $("#OrganizationId").val()
    };
    showLoader();
    if (validatecredentials()) {

        $.ajax({
            url: urls.LabourFileMoverurls.addOrgCreds,
            type: "POST",
            data: combinedFormData,
            success: function (result) {
                hideLoader();
                if (result == "Success") {
                    SuccessMsgWithReload("Success", "Record Added successfully", "success");

                }
                else if (result == "Data Exists") {
                    WarningMsg("Alert!", "Record exists", "success");
                }
                else {
                    ErrorMsg("Error", "Failed to Add Credentials", '');
                }
            },
            error: function () {
                hideLoader();
            }
        });
    }
    else {
        hideLoader();
    }
}
function validatecredentials() {

    let result = true;
    if ($("#KronosHostGLC").val() == "") {
        $('#spKronosHostGLC').text("Please enter KronosHostGLC");
        result = false;
    }
    if ($("#KronosPortGLC").val() == "") {
        $('#spKronosPortGLC').text("Please enter KronosPortGLC");
        result = false;
    }
    if ($("#KronosUserNameGLC").val() == "") {
        $('#spKronosUserNameGLC').text("Please enter KronosUserNameGLC");
        result = false;
    }
    if ($("#KronosPasswordGLC").val() == "") {
        $('#spKronosPasswordGLC').text("Please enter KronosPasswordGLC");
        result = false;
    }
    if ($("#StorageConnectionString").val() == "") {
        $('#spStorageConnectionString').text("Please enter StorageConnectionString");
        result = false;
    }
    if ($("#KronosDecryptOrEncryptKeyBlobNameGLC").val() == "") {
        $('#spKronosDecryptOrEncryptKeyBlobNameGLC').text("Please enter KronosDecryptOrEncryptKeyBlobNameGLC");
        result = false;
    }
    if ($("#KronosDecryptionPasswordGLC").val() == "") {
        $('#spKronosDecryptionPasswordGLC').text("Please enter KronosDecryptionPasswordGLC");
        result = false;
    }
    if ($("#KronosPunchExportGLC").val() == "") {
        $('#spKronosPunchExportGLC').text("Please enter KronosPunchExportGLC");
        result = false;
    }
    if ($("#OrganizationId").val() == "") {
        $('#spOrganizationId').text("Please select Organization");
        result = false;
    }

    return result;

}
function clearCombinedFormValues() {
    $('#KronosForm')[0].reset();
    //chosen 
    $('#OrganizationId').val('').trigger('change');
    // Clear any error messages
    $('.validation').empty();

    $("#KronosHostGLC").val("");
    $("#KronosPortGLC").val("");
    $("#KronosUserNameGLC").val("");
    $("#KronosPasswordGLC").val("");
    $("#StorageConnectionString").val("");
    $("# KronosDecryptionPasswordGLC").val("");
    $("#KronosDecryptOrEncryptKeyBlobNameGLC").val("");
    $("#KronosPunchExportGLC").val("");


};
function updateKronosStatus(Id, flag) {

    let status = "";
    if (flag == '0') {
        status = "Deactivate";
    }
    else {
        status = "Activate";
    }
    let confirmationMsg = "Are you sure want to " + status + " this Organization?";
    ConfirmMsg('Kronos', confirmationMsg, 'Continue', event, function () {

        let statusInput = {
            userId: Id,
            status: flag,
        }
        showLoader();
        $.ajax({
            type: 'Post',
            data: statusInput,
            async: false,
            url: urls.LabourFileMoverurls.ChangeKronosStatus,
            success: function (response) {
                hideLoader();
                if (response == "success") {
                    SuccessMsgWithReload('Success', "Active status has been changed", '');

                }
                else {
                    ErrorMsg('Error', "Unable to change status!Try again later", '');
                }
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
    });

}
function getEmployees() {
    let filterInput = {
        id: $("#Facility").val(),
    }
    $.ajax({
        url: urls.LabourFileMoverurls.GetEmployees,
        type: 'POST',
        data: filterInput,
        async: false,
        cache: false,
        success: function (response) {
            let orgFacilities = $("#EmployeeList");
            orgFacilities.empty();
            orgFacilities.append('<option value="">- Select Employee Name -</option>');
            $.each(response, function (index, item) {
                orgFacilities.append('<option value="' + item.text + '">' + item.text + '</option>');
            });
            orgFacilities.trigger("change");
        },
        failure: function () {
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        }

    })
}
function filterReset() {
    $('#Orglist').val("").trigger('change');
    $('#Facility').val("").trigger('change');
    $('#EmployeeList').val("").trigger('change');
    $('#QuarterList').val("").trigger('change');
    $('#UserName').val("");
    pendingDataFilters();
}
function getFacilities() {
    let filterInput = {
        id: $("#Orglist").val(),
    }
    $.ajax({
        url: urls.LabourFileMoverurls.GetFacilities,
        type: 'POST',
        data: filterInput,
        async: false,
        cache: false,
        success: function (response) {
            let orgFacilities = $("#Facility");
            orgFacilities.empty();
            orgFacilities.append('<option value="">- Select Facility Name -</option>');
            $.each(response, function (index, item) {
                orgFacilities.append('<option value="' + item.value + '">' + item.text + '</option>');
            });
            orgFacilities.trigger("change");
        },
        failure: function () {
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        }

    })
}
function pendingDataFilters() {
    let filterData = {
        orgId: $('#Orglist').val(),
        facilityId: $('#Facility').val(),
        empId: $('#EmployeeList').val(),
        quarter: $('#QuarterList').val(),
        userName: $('#UserName').val()
    }; showLoader();
    $.ajax({
        url: urls.LabourFileMoverurls.Pendingrecords,
        type: 'POST',
        data: filterData,
        async: false,
        cache: false,
        success: function (response) {
            hideLoader();
            $("#KronosPendingList").html($(response).find("#KronosPendingList").html());
            $('#KronosPendingList').dataTable().fnDestroy();
            $('#KronosPendingList').dataTable({
                order: [],
                pagingType: "full_numbers",
                language: {
                    paginate: {
                        first: '<i class="fa fa-angle-double-left"></i>',
                        previous: '<i class="fa fa-angle-left"></i>',
                        next: '<i class="fa fa-angle-right"></i>',
                        last: '<i class="fa fa-angle-double-right"></i>',
                    }
                },
                columnDefs: [
                    { orderable: false, targets: [3] }
                ]
            });

        },
        failure: function () {
            hideLoader();
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            hideLoader();
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        }

    })
}
function getComparionPartial(id) {
    showLoader();
    $.ajax({
        url: urls.LabourFileMoverurls.GetComparison,
        type: 'POST',
        data: { rowId: id },
        async: false,
        cache: false,
        success: function (result) {
            hideLoader();
            $('#ComparisionpartialViewContainer').html(result);
            $('#ComparisonModal').modal('show');
        },
        failure: function () {
            hideLoader();
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        }

    })
}
function updatecomp() {

    gatherSelectedData();
}
function gatherSelectedData() {
    // Array to store selected data
    var selectedData = [];


    // Select all checkboxes with the class 'selected'
    var checkboxes = document.querySelectorAll('.checkbox-row.selected');

    checkboxes.forEach(function (checkbox) {
        if (checkbox.checked) {
            // Get the row containing the selected checkbox
            var row = checkbox.closest('tr');

            // Extract the data from the row cells
            // Assuming 'row' represents a specific table row
            var data = {
                Id: row.cells[0].querySelector('input[type="hidden"]').value,
                Source: row.cells[1].innerText.trim(),
                EmployeeId: row.cells[2].innerText.trim(),
                facilityId: row.cells[3].querySelector('input[type="hidden"]').value,
                WorkDay: row.cells[4].innerText.trim(),
                JobTypeCode: row.cells[5].querySelector('input[type="hidden"]').value,
                PayTypeCode: row.cells[6].querySelector('input[type="hidden"]').value,
                Hours: row.cells[7].innerText.trim()
            };


            // Add the data to the selectedData array
            selectedData.push(data);
        }
    });
    showLoader();
    $.ajax({
        url: urls.LabourFileMoverurls.UpdateComparison,
        type: 'POST',
        data: { viewModel: selectedData },
        async: false,
        cache: false,
        success: function (result) {
            hideLoader();
            if (result == "Success") {
                $('#ComparisonModal').modal('hide');
                SuccessMsgWithReload("Success", "Record Added successfully", "success");

            }
            else {
                hideLoader();
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
            }
        },
        failure: function () {
            hideLoader();
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            hideLoader();
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        }

    });
}
function sendToProcess(id, OrgId) {
    console.log('sendToProcess');
    showLoader();
    setTimeout(function () {
        retrieve(id, OrgId);
    }, 1000);


}
function retrieve(id, OrgId) {

    $.ajax({
        url: urls.LabourFileMoverurls.ProcessData,
        type: 'POST',
        data: { id: id, OrgId: OrgId },
        async: false,
        cache: false,

        success: function (result) {


            if (result == "Success") {
                hideLoader();
                SuccessMsgWithReload("Success", "Record Added successfully", "success");
            }
            else if (result == "No Data Exists") {
                hideLoader();
                ErrorMsg('Error', "No Data Exist!Try again Later", '');
            }
            else if (result == "No Facility") {
                hideLoader();
                ErrorMsg('Error', "No Facility Exist!Try again Later", '');
            }
            else {
                hideLoader();
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
            }

        },
        failure: function () {
            hideLoader();
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        },
        error: function (jqXHR, textStatus, errorThrown) {
            hideLoader();
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        }

    });
}

function togglePasswordVisibility(icon) {
    var passwordField = icon.parentElement.parentElement.previousElementSibling;

    if (passwordField.type === 'password') {
        passwordField.type = 'text';
        icon.classList.remove("ri-eye-line");
        icon.classList.add("ri-eye-off-line");
    } else {
        passwordField.type = 'password';
        icon.classList.remove("ri-eye-off-line");
        icon.classList.add("ri-eye-line");
    }
}

function ShowDeduction() {
    if ($('#hasDeduction').is(':checked')) {
        $("#regularTimeValue").val('');
        $("#overTimeValue").val('');
        $("#Duduction").show();
        $("#regularTime").show();
        $("#overTime").show();
        ClearDeductionSpan();
    } else {
        $("#Duduction").hide();
        $("#DefaultTime").prop('checked', false);
        ClearDeductionSpan();
    }
}
function HideTimeInputs() {
    var regularTimeValue = $("#regularTimeValue").val();
    var overTimeValue = $("#overTimeValue").val();

    if ($('#DefaultTime').is(':checked')) {
        $("#regularTimeValue").val(30);
        $("#overTimeValue").val(60);
        $("#regularTime").hide();
        $("#overTime").hide();
        ClearDeductionSpan();
    } else {
        $("#regularTime").show();
        $("#overTime").show();
        $("#regularTimeValue").val('');
        $("#overTimeValue").val('');
        ClearDeductionSpan();
    }
}
function ClearDeductionSpan() {

    $("#spHasDeduction").empty();
}

