$('document').ready(function () {
    getPayTypeList();
    getJobTitleList();
    loadEmployeeList();
    initSelect2EmployeeMaster();
    $('#AddEmployeeModel').modal({
        backdrop: 'static',
        keyboard: false
    });
    $('#FacilityNameId').on('select2:select', function () {
        let errorId = $(this).data('error-id');
        let errorSpan = $('#' + this.id).nextAll('span.text-danger:first');
        errorSpan.text('');
    });
})

function FilterChange() {
    loadEmployeeList();
}

function getPayTypeList() {
    $.ajax({
        type: "GET",
        url: urls.PayTypeJobTitle.GetPayCode,
        success: function (data) {
            $('#PayType').html();
            GetPayCodeList = data;
            let PayType = "";
            $.each(data, function (i, item) {
                PayType += "<option value='" + item.payTypeCode + "'>" + item.payTypeDescription + "</option>";
            })
            $('#PayType').html(PayType);
        },
        error: function (error) {
            ErrorMsg("Alert!", "Failed to load");
        }
    })
}

function getJobTitleList() {
    $.ajax({
        type: "GET",
        url: urls.PayTypeJobTitle.GetJobTitle,
        success: function (data) {
            $('#JobTitle').html();
            GetJobTitleList = data;
            let JobTitle = "";
            $.each(data, function (i, item) {
                JobTitle += "<option value='" + item.id + "'>" + item.title + "</option>";
            })
            $('#JobTitle').html(JobTitle);
        },
        error: function (error) {
            ErrorMsg("Alert!", "Failed to load");
        }
    })
}

function loadEmployeeList() {
    let facilityId = $('#FacilityId').val();
    if (facilityId == undefined || facilityId == '' || facilityId == 'Select Facility Name') {
        facilityId = null;
    }
    let formData = {
        facilityId: facilityId
    };
    let table = $('#EmployeeListDataTable').DataTable();
    table.destroy();
    $('#EmployeeListDataTable').DataTable(
        {
            ajax: {
                url: urls.Employee.EmployeeList,
                type: "POST",
                data: function (data) {
                    data.searchValue = $('#EmployeeListDataTable_filter input').val();
                    data.start = data.start || 0; // start parameter
                    data.length = data.length || 10; // length parameter
                    data.draw = data.draw || 1; // draw parameter
                    data.sortColumn = data.columns[data.order[0].column].data; // sort column
                    data.sortDirection = data.order[0].dir;
                    data.facilityId = formData.facilityId;
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
            //scrollY: "300px",
            //scrollCollapse: true,
            //ordering: true,
            //order: [[0, 'desc']],
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
                { data: "employeeId", name: "Id" },
                { data: "employeeFullName", name: "Employee Name" },
                { data: "hireDate", name: "Hire Date" },
                { data: "terminationDate", name: "Termination Date" },
                { data: "facilityName", name: "Facility Name" },
                { data: "payType", name: "Pay Type" },
                { data: "jobTitle", name: "Job Title" },
                { data: "createDate", name: "Create Date" },
                {
                    data: null, orderable: false,
                    "render": function (data, type, full, row) {
                        let toggleButtonHtml = full.isActive
                            ? "<a href='#' onclick=UpdateEmployeeStatus('" + full.id + "','false'); data-bs-toggle='tooltip' data-bs-placement='top' title='Deactivate'><i class='fas fa-toggle-on me-2 text-success fs-6'></i></a>"
                            : "<a href='#' onclick=UpdateEmployeeStatus('" + full.id + "','true'); data-bs-toggle='tooltip' data-bs-placement='top' title='Activate'><i class='fas fa-toggle-off me-2 text-muted fs-6'></i></a>";

                        return " <a href='#' onclick=editEmployee('" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Edit'><i class='ri-edit-2-line fs-6 pe-2 text-info'></i></a>"
                            + toggleButtonHtml;
                    }

                }
            ]
        }
    );
}

function editEmployee(id) {
    clearEmployeeData();
    let formData = {
        Id: id
    };
    showLoader();
    $.ajax({
        url: urls.Review.EditEmployeeData,
        type: 'POST',
        data: formData,
        success: function (result) {
            hideLoader();
            $('#EmployeeModalLabel').html('');
            $('#EmployeeModalLabel').html('Edit Staff');
            let hireDate = (result.hireDate == "0001-01-01T00:00:00") ? result.hireDate : moment(result.hireDate).format("YYYY-MM-DD");
            let terminationDate = (result.terminationDate == "0001-01-01T00:00:00") ? result.terminationDate : moment(result.terminationDate).format("YYYY-MM-DD");
            $('#EmployeeId').val(result.id);
            $('#Employee_Id').val(result.employeeId);
            $('#Employee_Id').prop('disabled', true);
            $('#FirstName').val(result.firstName);
            $('#LastName').val(result.lastName);
            $('#HireDate').val(hireDate);
            $('#TerminationDate').val(terminationDate);
            $('#PayType').val(result.payTypeCode).trigger('change');
            $('#JobTitle').val(result.jobTitleCode).trigger('change');
            $('#FacilityNameId').val(result.facilityId).trigger('change');
            $('#SaveEmployee').css("display", "none");
            $('#UpdateEmployee').css("display", "block");
            $("#AddEmployeeModel").modal('show');
            draggableModel('#AddEmployeeModel');
        },
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            } else {
                ErrorMsg("Alert!", "Failed to load");
            }

        }
    });
}

function deleteEmployee(employeeId) {
    ConfirmMsg('Employee', 'Are you sure want to delete the employee data?', 'Continue', event, function () {
        showLoader();
        $.ajax({
            url: urls.Review.DeleteEmployee,
            type: 'POST',
            data: { EmployeeId: employeeId },
            success: function (result) {
                hideLoader();
                if (result.statusCode == 200) {
                    SuccessMsg("Success", result.message, "success");
                    loadEmployeeList();
                } else {
                    ErrorMsg("Failed", result.message);
                }
            },
            error: function (xhr, status, error) {
                hideLoader();
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Failed", "Unauthorized Access");
                } else {
                    ErrorMsg('Error', error, '');
                }
            }
        });
    });
}

function clearEmployeeData() {
    $('#EmployeeId').val('');
    $('#Employee_Id').val('');
    $('#FirstName').val('');
    $('#LastName').val('');
    $('#HireDate').val('');
    $('#TerminationDate').val('');
    $('#Employee_IdError').html('');
    $('#FirstNameError').html('');
    $('#LastNameError').html('');
    $('#HireDateError').html('');
    $('#TerminationDateError').html('');
    $('#PayTypeError').html('');
    $('#JobTitleError').html('');
    $('#FacilityNameId').val(null).trigger('change');
    $('#FacilityNameIdError').html('');
}

function openEmployeeMoal() {
    $('#EmployeeId').val('');
    $('#EmployeeModalLabel').html('');
    $('#EmployeeModalLabel').html('Add Staff');
    $('#Employee_Id').prop('disabled', false);
    $('#SaveEmployee').css("display", "block");
    $('#UpdateEmployee').css("display", "none");
    clearEmployeeData();
}

function saveEmployeeFunction() {
    let token = $('input[name="__RequestVerificationToken"]').val();
    var alphanumericRegex = /^[a-zA-Z0-9]+$/;
    var alphaRegex = /^[a-zA-Z'. ]+$/;
    var flag = 0;
    clearErrorMessage();
    $('.fieldRequired').each(function () {
        let elementId = $(this).attr("id");
        let data = $('#' + elementId).val();
        if (data == '' || data == null) {
            $('#' + elementId + 'Error').html('The ' + $('#' + elementId + 'Error').attr("title") + ' field is required.');
            flag = 1;
            return false;
        } else {
            $('#' + elementId + 'Error').html('');
        }
    });
    if (flag == 1) {
        return false;
    }
    if ($('#Employee_Id').val().length > 30) {
        $('#Employee_IdError').html('Employee Id must be smaller then 30 characters.');
        return false;
    }
    if (!alphanumericRegex.test($('#Employee_Id').val())) {
        $('#Employee_IdError').html('No spacial characters are allowed');
        return false;
    }
    if (!alphaRegex.test($('#FirstName').val())) {
        $('#FirstNameError').html('First Name allows only alphabatic characters');
        return false;
    }
    if (!alphaRegex.test($('#LastName').val())) {
        $('#LastNameError').html('Last Name allows only alphabatic characters');
        return false;
    }
    if ($('#HireDate').val() != '' && $('#TerminationDate').val() != '') {
        
        if ($('#HireDate').val() > $('#TerminationDate').val()) {
            $('#TerminationDateError').html('The Termination date should be greater than Hire date.');
            return false;
        }
    }
    if ($('#TerminationDate').val() != '') {
        if ($('#HireDate').val() == '') {
            $('#HireDateError').html('Select Hire date.');
            return false;
        }
    }
    clearErrorMessage();
    if ($('#FacilityId').val() == undefined || $('#FacilityId').val() == '') {
        WarningMsg("Alert!", "Please select Facility", "Okay");
        return false;
    }
    let employeeData = {
        Id: $('#EmployeeId').val(),
        EmployeeId: $('#Employee_Id').val().trim(),
        FirstName: $('#FirstName').val().trim(),
        LastName: $('#LastName').val().trim(),
        PayType: $('#PayType').val(),
        JobTitle: $('#JobTitle').val(),
        HireDate: $('#HireDate').val(),
        TerminationDate: $('#TerminationDate').val(),
        FacilityId: $('#FacilityNameId').val()
    }
    showLoader();
    $.ajax({
        url: urls.Review.SaveEmployeeeData,
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: employeeData,
        async: false,
        cache: false,
        success: function (result) {
            hideLoader();
            if (result == "Success") {
                SuccessMsg("Success", "Record saved successfully", "success");
                $("#AddEmployeeModel").modal('hide');
                clearEmployeeData();
                let table = $('#EmployeeListTable').DataTable();
                table.destroy();
                loadEmployeeList();
            }
            else if (result == "IdError") {
                $('#Employee_IdError').html('No spacial characters are allowed');
            }
            else if (result == "FirstNameError") {
                $('#FirstNameError').html('First Name allows only alphabatic characters');
            }
            else if (result == "LastNameError") {
                $('#LastNameError').html('Last Name allows only alphabatic characters');
            } else {
                WarningMsg("Alert!", result, "Failed");
                return false;
            }
        },
        error: function (result) {
            hideLoader();
            ErrorMsg("Alert!", "Failed");
        }
    });
}
function clearErrorMessage() {
    $('#Employee_IdError').html('');
    $('#FirstNameError').html('');
    $('#LastNameError').html('');
    $('#HireDateError').html('');
    $('#TerminationDateError').html('');
}
function importEmpoyeeFile() {
    let faciltyID = $('#FacilityId').val();
    if (faciltyID == undefined || faciltyID == '' || faciltyID == 'Select Facility Name') {
        ErrorMsg("Warning", "Please select the facility name");
        return false;
    }
    let fileInput = $("input[type='file']")[0];
    let file = fileInput.files[0];
    if (file == undefined || file == '') {
        ErrorMsg("Warning", "Please select a file!");
        return false;
    }

    if (file != undefined) {
        let data = new FormData();
        data.append("FacilityId", $('#FacilityId').val());
        data.append("file", file);
        showLoader();
        $.ajax({
            url: urls.Employee.SaveEmployeeDetail,
            type: 'POST',
            data: data,
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.statusCode == 200) {
                    hideLoader();
                    SuccessMsg("Success", result.message, "success");
                    $('#file').val('');
                    $('#ImportEmployee').modal('hide');
                    loadEmployeeList();
                } else if (result.statusCode == 202) {
                    if (result.keyValuePairs.length > 0) {
                        $('#tableBodyEmpErrorList').html('');
                        $('#file').val('');
                        let body = '';
                        result.keyValuePairs.forEach(function (value, key) {
                            body += "<tr>";
                            body += "<td>" + value.key + "</td>";
                            body += "<td>" + value.value + "</td>";
                            body += "</tr>";
                        });
                        loadEmployeeList();
                        hideLoader();
                        WarningMsg("Alert!", result.message, "Okay");
                        $('#tableBodyEmpErrorList').html(body);
                        $('#ImportEmployee').modal('hide');
                        $('#ImportEmployeeErrorList').modal('show');
                        draggableModel('#ImportEmployeeErrorList');
                    }
                }
                else {
                    hideLoader();
                    $('#file').val('');
                    ErrorMsg("Warning", result.message);
                }
            },
            error: function (result) {
                hideLoader();
                ErrorMsg("Warning", "Failed to import");

            }
        });
    }
}


function getLoggedInUser() {
    let isSuperAdmin;
    $.ajax({
        url: '/Facility/CheckUserRole',
        type: 'GET',
        async: false,
        dataType: 'json',
        success: function (result) {
            if (result) {
                isSuperAdmin = true;
            }
            else {
                isSuperAdmin = false;
            }
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
    hideLoader();
    ErrorMsg("Warning", "Failed to import");

    return isSuperAdmin;
}

function initSelect2EmployeeMaster() {
    $('#PayType').select2({
        dropdownParent: $('#PayTypeDiv'),
        placeholder: "Select Pay Type",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#JobTitle').select2({
        dropdownParent: $('#JobTitleDiv'),
        placeholder: "Select Job Title",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#FacilityNameId').select2({
        dropdownParent: $('#FacilityNameIdDiv'),
        placeholder: "Select Facility Name",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });
}

function clearFilter() {
    $('#FacilityId').val('Select Facility Name').trigger('change');
}

function UpdateEmployeeStatus(employeeId, status) {
    let confirmationMessage = status === 'true'
        ? 'Are you sure you want to activate this employee?'
        : 'Are you sure you want to deactivate this employee?';
    ConfirmMsg('Employee Management', confirmationMessage, 'Continue', event, function () {
        showLoader();
        $.ajax({
            url: urls.Review.UpdateEmployeeStatus,
            type: 'POST',
            data: { employeeId: employeeId, status: status },
            success: function (result) {
                hideLoader();
                if (result.statusCode == 200) {
                    SuccessMsg("Success", result.message, "success");
                    loadEmployeeList();
                } else {
                    ErrorMsg("Failed", result.message);
                }
            },
            error: function (xhr, status, error) {
                hideLoader();
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Failed", "Unauthorized Access");
                } else {
                    ErrorMsg('Error', error, '');
                }
            }
        });
    });
}

function restrictInput(event) {
    let pattern = /^[A-Za-z\s\-'.]+$/;
    let key = event.key;
    if (pattern.test(key) || key === 'Backspace') {
        return;
    }
    event.preventDefault();
}
