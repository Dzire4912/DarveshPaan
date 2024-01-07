var ListQuarters = [
    { key: '1', value: '1st Quarter (October 1 - December 31)' },
    { key: '2', value: '2nd Quarter (January 1 - March 31)' },
    { key: '3', value: '3rd Quarter (April 1 - June 30)' },
    { key: '4', value: '4th Quarter (July 1 - September 30)' }
];

var ListMonths = [{
    0: "Select All",
    10: "October",
    11: "November",
    12: "December"
},
{
    0: "Select All",
    1: "January",
    2: "February",
    3: "March"
},
{
    0: "Select All",
    4: "April",
    5: "May",
    6: "June"
}, {
    0: "Select All",
    7: "July",
    8: "August",
    9: "September"
}]
var GetPayCodeList = [];
var GetJobTitleList = [];
var AddSelectedDates = [];
var addSelectedColorDates = [];

$('document').ready(function () {
    getYear();
    getQuaterList();
    getMonthList();
    getAgencies();
    loadEmployeeListData();
    loadStaffDeptData();
    loadCencusData();
    getPayTypeList();
    getJobTitleList();
    initSelect2Review();
    checkIfValidated();
    checkIfApproved();
    getAllFileNameByFacility();
    $('#AddNurseWorkingHours').modal({
        backdrop: 'static',
        keyboard: false
    });
    $('#ReviewModal').modal({
        backdrop: 'static',
        keyboard: false
    });
    $('#AddEmployeeModel').modal({
        backdrop: 'static',
        keyboard: false
    });
    $('#EmployeeDataWorkingHoursCalenderModal').modal({
        backdrop: 'static',
        keyboard: false
    });
    $('#EmpAddUpdateWorkingHours').modal({
        backdrop: 'static',
        keyboard: false
    });
    $('#CencusModal').modal({
        backdrop: 'static',
        keyboard: false
    });
})

function getQuaterList() {
    $('#QuarterList').html();
    var dateobj = new Date();
    var getMonth = dateobj.getMonth();
    var _key = findQuarter(getMonth + 1);
    var qtrList = "<option value='-1'>Select</option>";
    ListQuarters.forEach(function (value, key) {
        if (value.key == _key) {
            qtrList += "<option value='" + value.key + "' selected>" + value.value + "</option>";
        } else {
            qtrList += "<option value='" + value.key + "'>" + value.value + "</option>";
        }
    })
    $('#QuarterList').html(qtrList);
}

function findQuarter(month) {
    var value = 0;
    switch (month) {
        case 1:
        case 2:
        case 3:
            value = 2;
            break;
        case 4:
        case 5:
        case 6:
            value = 3;
            break;
        case 7:
        case 8:
        case 9:
            value = 4;
            break;
        case 10:
        case 11:
        case 12:
            value = 1;
            break;
    }
    return value;
}

function getMonthList() {
    $('#MonthList').html();
    var mnthList = "";
    var qrtId = $('#QuarterList').val();
    qrtId = qrtId - 1;
    $.each(ListMonths[qrtId], function (i, item) {
        mnthList += "<option value='" + i + "'>" + item + "</option>";
    })
    $('#MonthList').html(mnthList);
}

function getYear() {
    var start_date = 2020;
    var dateobj = new Date();
    var end_date = dateobj.getFullYear();
    $('#YearList').html();
    var yrList = "";
    yrList += "<option value='0'>Select</option>";

    while (start_date <= end_date) {
        if (end_date == start_date) {
            yrList += "<option value='" + start_date + "' selected>" + start_date + "</option>";
        } else {
            yrList += "<option value='" + start_date + "'>" + start_date + "</option>";
        }
        start_date = start_date + 1;
    }
    $('#YearList').html(yrList);
}

function getAgencies() {
    if ($('#FacilityId').val() != '' && $('#FacilityId').val() != null) {
        var facility = $('#FacilityId').val();

        $.ajax({
            url: '/Agency/GetAgenciesByFacility',
            type: 'POST',
            data: { FacilityId: facility },
            success: function (result) {
                var agencyList = "<option value=''>Select Agency</option>";
                result.forEach(function (value, key) {
                    agencyList += "<option value='" + key.id + "'>" + value.agencyName + "</option>";
                })
                $('#AgenciesId').html('');
                $('#AgenciesId').html(agencyList);
            },
            error: function (result) {
                ErrorMsg("Warning", "Failed");
            }
        });
    }
}

function openCencusModal() {
    $('#CensusModalLabel').html('');
    $('#CensusModalLabel').html('Add Census');
    $('#SaveCencus').css("display", "block");
    $('#btnUpdateCencus').css("display", "none");
    clearCencusData();
    getCencusMonth();
}

function getCencusMonth() {
    $('#CencusMonthList').html();
    var monthList = "";
    var qrterId = $('#QuarterList').val();
    qrterId = qrterId - 1;
    $.each(ListMonths[qrterId], function (i, item) {
        monthList += "<option value='" + i + "'>" + item + "</option>";
    })
    $('#CencusMonthList').html(monthList);
}

function saveFunction() {
    let form = $('#CensunFormInReview');
    let token = form.find('input[name="__RequestVerificationToken"]').val();
    var flag = 0;
    if ($('#CencusMonthList').val() == '0') {
        $('#CencusMonthListError').html('The ' + $('#CencusMonthListError').attr("title") + ' field is required!');
        flag = 1;
        return false;
    }
    $('#CencusMonthListError').html('');
    $('.CencusfieldRequired').each(function () {
        var data = $(this).val();
        var elementId = $(this).attr("id");
        if (data == 0 || data == '') {
            $('#' + elementId + 'Error').html('The ' + $('#' + elementId + 'Error').attr("title") + ' has invalid count!');
            flag = 1;
            return false;
        } else {
            $('#' + elementId + 'Error').html('');
        }
    });

    if (flag == 1) {
        return false;
    }

    var cencus = {
        CensusId: $('#CensusId').val().trim(),
        FacilityId: $('#FacilityId').val(),
        ReportQuarter: $('#QuarterList').val(),
        Month: $('#CencusMonthList').val(),
        Year: $('#YearList').val(),
        Medicare: $('#RMedicare').val(),
        Medicad: $('#RMedicad').val(),
        Other: $('#ROther').val()
    }
    showLoader();
    $.ajax({
        url: urls.Review.AddCencus,
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: cencus,
        async: false,
        cache: false,
        success: function (result) {
            hideLoader();
            if (result.statusCode == 200) {
                SuccessMsg("Success", result.message, "success");
                $("#CencusModal").modal('hide');
                clearCencusData();
                let table = $('#CencusTable').DataTable();
                table.destroy();
                loadCencusData();
            } else {
                ErrorMsg("Failed", result.message);
            }

        },
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
            else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                $("#CencusModal").modal('hide');
                clearCencusData();
                let table = $('#CencusTable').DataTable();
                table.destroy();
                loadCencusData();
            }
            else {
                ErrorMsg("Failed", error, "error")
            }
        }
    });
}

function clearCencusData() {
    $('#RMedicare').val('');
    $('#RMedicad').val('');
    $('#ROther').val('');

}

function deleteCencus(CencusId) {
    let form = $('#CensunFormInReview');
    let token = form.find('input[name="__RequestVerificationToken"]').val();
    ConfirmMsg('Cencus', 'Are you sure want to delete the cencus data?', 'Continue', event, function () {
        showLoader();
        $.ajax({
            url: urls.Review.DeleteCencus,
            type: 'POST',
            headers: { 'RequestVerificationToken': token },
            data: { CencusId: CencusId },
            async: false,
            Cache: false,
            success: function (result) {
                hideLoader();
                if (result.statusCode == 200) {
                    SuccessMsg("Success", result.message, "Deleted");
                    loadCencusData();
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

$(".edit-cencus").click(function () {
    getCencusMonth();
    clearCencusData();
    $('#RMedicare').val($(this).closest("tr").find('td').eq(3).text().trim());
    $('#RMedicad').val($(this).closest("tr").find('td').eq(4).text().trim());
    $('#ROther').val($(this).closest("tr").find('td').eq(5).text().trim());
    $('#RMedicare').val($(this).closest("tr").find('td').eq(3).text().trim());
    $('#CencusMonthList').val($(this).closest("tr").find('td').eq(7).text().trim());
    $('#CensusId').val($(this).closest("tr").find('td').eq(6).text().trim());
    $('#SaveCencus').css("display", "none");
    $('#btnUpdateCencus').css("display", "block");
    $("#CencusModal").modal('show');
    draggableModel('#CencusModal');
});

function GetCencusChange() {
    LoadCencusData(1);
}

function loadStaffDeptData() {
    let facilityName = $('#FacilityId').val();
    let StaffingDepartmentCsvRequest = {
        FacilityId: facilityName,
        Year: $('#YearList').val(),
        ReportQuarter: $('#QuarterList').val()
    };
    let table = $('#StaffDeptTable').DataTable();
    table.destroy();
    $('#StaffDeptTable').DataTable(
        {
            ajax: {
                url: urls.Review.GetStaffingData,
                type: "POST",
                data: StaffingDepartmentCsvRequest,
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
                { data: "title", name: "Title" },
                { data: "totalHours", name: "Hours" },
                { data: "staffCount", name: "StaffCount" },
                { data: "exempt", name: "Exempt" },
                { data: "nonExempt", name: "nonExempt" },
                { data: "contractors", name: "contractors" },
                //{ data: "facilityName", name: "facilityName" }
            ]
        }
    );
}

function saveEmployeeFunction() {
    let form = $('#EmployeeDataFormInReview');
    var alphanumericRegex = /^[a-zA-Z0-9]+$/;
    var alphaRegex = /^[a-zA-Z'. ]+$/;
    let token = form.find('input[name="__RequestVerificationToken"]').val();
    var flag = 0;
    clearErrorMessage();
    $('.fieldRequired').each(function () {
        var elementId = $(this).attr("id");
        var data = $('#' + elementId).val().trim();
        if (data == '') {
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
    var employeeData = {
        Id: $('#EmployeeId').val(),
        EmployeeId: $('#Employee_Id').val().trim(),
        FirstName: $('#FirstName').val().trim(),
        LastName: $('#LastName').val().trim(),
        PayType: $('#PayType').val(),
        JobTitle: $('#JobTitle').val(),
        HireDate: $('#HireDate').val(),
        TerminationDate: $('#TerminationDate').val(),
        FacilityId: $('#FacilityId').val()
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
                loadEmployeeListData();
            }
            else if (result == "IdError") {
                $('#Employee_IdError').html('No spacial characters are allowed');
            }

            else if (result == "FirstNameError") {
                $('#FirstNameError').html('First Name allows only alphabatic characters');
            }
            else if (result == "LastNameError") {
                $('#LastNameError').html('Last Name allows only alphabatic characters');
            }
            else {
                WarningMsg("Alert!", result, "Okay");
                return false;
            }

        },
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access", '');
            }
            else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                ErrorMsg("Failed", "Too Many Requests, Please try after some time", '');
                $("#AddEmployeeModel").modal('hide');
                let table = $('#EmployeeListTable').DataTable();
                table.destroy();
                loadEmployeeListData();
            }
            else {
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
            }
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
function openEmployeeMoal() {
    $('#EmployeeId').val('');
    $('#EmployeeModalLabel').html('');
    $('#EmployeeModalLabel').html('Add Staff');
    $('#Employee_Id').prop('disabled', false);
    $('#SaveEmployee').css("display", "block");
    $('#UpdateEmployee').css("display", "none");
    clearEmployeeData();
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
}

function loadCencusData() {

	var CencusRequest = {
		FacilityID: $('#FacilityId').val(),
		Year: $('#YearList').val(),
		ReportQuarter: $('#QuarterList').val()
	}
	let formData = {
		cencusViewRequest: CencusRequest
	};
	let table = $('#CencusTable').DataTable();
	table.destroy();
	$('#CencusTable').DataTable(
		{
			ajax: {
				url: urls.Review.GetCensus,
				type: "POST",
				data: formData,
				error: function (xhr, status, error) {
                    if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
						ErrorMsg("Failed", "Unauthorized Access");
					}
				},
			},
			processing: true,
			serverSide: true,
			filter: true,
			/*scrollY: "300px",
			scrollCollapse: true,*/
			searching: false,
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
				{ data: "facilityName", name: "FacilityName" },
				{ data: "year", name: "Year" },
				{ data: "month", name: "Month" },
				{ data: "medicare", name: "Medicare" },
				{ data: "medicad", name: "Medicad" },
				{ data: "other", name: "Other" },
				{
					data: null, orderable: false,
					"render": function (data, type, full, row) {
						return "<a href='#' onclick=editCencus('" + full.censusId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Edit'><i class='ri-edit-2-line fs-6 pe-2 text-info'></i></a>"
							+ " " + "<a href='#' onclick=deleteCencus('" + full.censusId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
					}
				}
			]
		}
	);
}

function initSelect2ForReview() {
    $('#PayType').select2({
        placeholder: "Select Pay Type",
        dropdownParent: $('#selectPayTypeDivState'),
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#JobTitle').select2({
        placeholder: 'Select Job Title',
        dropdownParent: $('#selectJobTitleDivState'),
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });
}

function editCencus(CencusId) {
    let formData = {
        CencusId: CencusId
    };
    clearCencusData();
    showLoader();
    $.ajax({
        url: urls.Review.EditCencus,
        type: 'POST',
        data: formData,
        success: function (result) {
            hideLoader();
            getCencusMonth();
            $('#RMedicad').val(result.medicad);
            $('#ROther').val(result.other);
            $('#RMedicare').val(result.medicare);
            $('#CencusMonthList').val(result.month);
            $('#CensusId').val(result.censusId);
            $('#SaveCencus').css("display", "none");
            $('#btnUpdateCencus').css("display", "block");
            $('#CensusModalLabel').html('');
            $('#CensusModalLabel').html('Update Census');
            $("#CencusModal").modal('show');
            draggableModel('#CencusModal');
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

function loadEmployeeListData() {
    let facilityId = $('#FacilityId').val();
    let EmployeeListRequest = {
        facilityId: facilityId,
        Year: $('#YearList').val(),
        ReportQuarter: $('#QuarterList').val(),
        Month: $('#MonthList').val()
    };
    let table = $('#EmployeeListTable').DataTable();
    table.destroy();
    $('#EmployeeListTable').DataTable(
        {
            ajax: {
                url: urls.Review.EmployeeData,
                type: "POST",
                data: EmployeeListRequest,
                error: function (xhr, status, error) {
                    if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                        ErrorMsg("Failed", "Unauthorized Access");
                    }
                },
            },
            processing: true,
            serverSide: true,
            filter: true,
            //scrollY: "200px",
            order: [[1, "asc"]],
            scrollCollapse: true,
            fixedHeader: true,
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
                {
                    data: null, orderable: false,
                    "render": function (data, type, full, row) {
                        return "<a href='#' onclick=openEmployeeCalender('" + full.employeeId + "','" + full.facilityId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Edit'><i class='ri-calendar-line fs-6 pe-2 text-info'></i></a>";
                    }
                },
                { data: "employeeId", name: "Id" },
                { data: "employeeFullName", name: "Name" },
                { data: "hireDate", name: "Hire Date" },
                { data: "terminationDate", name: "Termination Date" },
                { data: "payType", name: "Pay Type" },
                { data: "jobTitle", name: "Job Title" },
                {
                    data: null, orderable: false,
                    "render": function (data, type, full, row) {
                        return "<div class='text-center'>" +
                            "<a href='#' onclick=editEmployee('" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Edit'>" +
                            "<i class='ri-edit-2-line fs-6 pe-2 text-info'></i>" +
                            "</a>" +
                            "</div>";
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

function FilterChange() {
    checkIfValidated();
    checkIfApproved();
    loadEmployeeListData();
    loadStaffDeptData();
    loadCencusData();
    $('#dataTableContainer').html('');
    $('#rowCountId').html('');
    $('#pageCountId').html('');
    getAllFileNameByFacility();
}

var events = [];
var selectedEvent = null;
function openEmployeeCalender(empoyeeId, facilityId) {
    events = [];
    let employeeName = getEmployeeName(empoyeeId);
    let empData = getEmpDetials(empoyeeId, facilityId);
    let startDate = moment(new Date).format("YYYY-MM-01");
    let endDate = moment(new Date).format("YYYY-MM-dd");
    var EmployeeTimesheetDataRequest = {
        EmployeeId: empoyeeId,
        FacilityId: $('#FacilityId').val(),
        ReportQuarter: $('#QuarterList').val(),
        Year: $('#YearList').val(),
        Month: $('#MonthList').val(),
        StartDate: moment(startDate).format("YYYY-MM-DD"),
        EndDate: moment(endDate).format("YYYY-MM-DD")
    }
    $.ajax({
        url: urls.Review.GetEmployeeTimesheet,
        type: 'POST',
        data: EmployeeTimesheetDataRequest,
        success: function (result) {
            $.each(result, function (i, v) {
                events.push({
                    eventID: v.timesheetId,
                    title: v.totalHours,
                    description: v.employeeId,
                    start: moment(v.workday),
                    end: v.end != null ? moment(v.workday) : null,
                    color: v.color,
                    editable: v.editable,
                    isButtonEditable: v.isButtonEditable
                });
            })
            $('#ReviewModalLabel').html('Working Details');
            $('#ReviewModalEmpDetails').html('<strong>Employee Id : </strong>' + empData.Id);
            $('#ReviewModalEmpDetails1').html('<strong>Employee Name : </strong> ' + empData.FullName);
            $('#ReviewModalEmpDetails2').html('<strong>JobTitile : </strong>' + empData.JobTitle);
            $('#ReviewModalEmpDetails3').html('<strong>PayType : </strong>' + empData.PayTypeName);
            $('#EmployeeIdWorking').val(empoyeeId);
            $('#EmployeeDataWorkingHoursCalenderModal').modal('show');
            draggableModel('#EmployeeDataWorkingHoursCalenderModal');
            GenerateEmployeeCalender(events);
        },
        error: function (result) {
            ErrorMsg("Alert!", "Failed to load");
        }
    });
}
//Get PayType and JobTybe employee details by Id
function getEmpDetials(employeeId, facilityId) {
    let EmployeeById = {
        Id: '',
        FullName: '',
        PayTypeName: '',
        JobTitle: '',
    }
    $.ajax({
        url: urls.Review.GetEmpDetials,
        type: 'GET',
        data: {
            EmployeeId: employeeId,
            FacilityId: facilityId
        },
        async: false,
        success: function (data) {
            if (data) {
                EmployeeById.Id = data.result.id;
                EmployeeById.FullName = data.result.fullName;
                EmployeeById.PayTypeName = data.result.payTypeName;
                EmployeeById.JobTitle = data.result.jobTitle;
            }
            else {
                EmployeeById = '';
            }
        },
        error: function (result) {
            ErrorMsg('Error', 'Failed to Get Employee Name', '');
        }
    });

    return EmployeeById;
}

function GenerateEmployeeCalender(events) {
    $('#EmployeeWorkingHoursCalender').fullCalendar('destroy');
    $('#EmployeeWorkingHoursCalender').fullCalendar({
        contentHeight: 350,
        defaultDate: new Date(),
        timeFormat: 'h(:mm)a',
        displayEventTime: false,
        header: {
            left: 'prev,next today',
            center: 'title',
            right: 'month,basicWeek,basicDay'
        },
        eventLimit: true,
        eventColor: '#378006',
        events: events,
        buttonText: {
            today: 'Today',
            month: 'Month',
            week: 'Week',
            day: 'Day'
        },
        eventClick: function (calEvent, jsEvent, view) {
            let date = new Date(calEvent.start);
            let month = date.getMonth();
            let quarter = findQuarter(month + 1);

            selectedEvent = calEvent;
            /*getJobTitleList();
            getPayTypeList();*/

            var PayType = "";
            $.each(GetPayCodeList, function (i, item) {
                PayType += "<option value='" + item.payTypeCode + "'>" + item.payTypeDescription + "</option>";
            })
            $('#EmpWorkingPayType').html(PayType);

            var JobTitle = "";
            $.each(GetJobTitleList, function (i, item) {
                JobTitle += "<option value='" + item.id + "'>" + item.title + "</option>";
            })
            $('#EmpWorkingJobTitle').html(JobTitle);

            var EmployeeTimesheetDataRequest = {
                TimesheetId: calEvent.eventID,
                EmployeeId: $('#EmployeeIdWorking').val(),
                FacilityId: $('#FacilityId').val(),
                ReportQuarter: $('#QuarterList').val(),
                Month: $('#MonthList').val(),
                Year: $('#YearList').val(),
                Workday: calEvent.start.format("MM/DD/YYYY")
            }
            $.ajax({
                url: urls.Review.GetEmployeeData,
                type: 'POST',
                data: EmployeeTimesheetDataRequest,
                success: function (result) {
                    console.log('GenerateEmployeeCalender ~ ' + JSON.stringify(result));
                    $('#EmpTimesheetId').val('');
                    $('#EmpWorkingPayType').val(result.payTypeCode).trigger('change');
                    /*$('#EmpWorkingPayType').trigger('change');*/
                    $('#EmpWorkingJobTitle').val(result.jobTitleCode).trigger('change');
                    /*$('#EmpWorkingJobTitle').trigger('change');*/
                    $('#EmpTimesheetId').val(calEvent.eventID);
                    $('#EmpHoursWorked').val(calEvent.title);
                    $('#EmpWorkDate').val(calEvent.start.format("MM/DD/YYYY"));
                    $('#AddWorkingHoursLabel').html('Update working details');
                    if (calEvent.isButtonEditable) {
                        $('#btnSaveEmployeeWorkingHours').removeClass('hide');
                        $('#btnDeleteEmployeeWorkingHours').removeClass('hide');
                    } else {
                        $('#btnSaveEmployeeWorkingHours').addClass('hide');
                        $('#btnDeleteEmployeeWorkingHours').addClass('hide');
                    }

                    $('#EmpHoursWorkedError').html('');
                    $('#EmpAddUpdateWorkingHours').modal('show');
                    draggableModel('#EmpAddUpdateWorkingHours');
                },
                error: function (result) {
                    ErrorMsg("Alert!", "Failed to load");
                }
            });

        },
        selectable: true,
        select: function (start, end, jsEvent, view) {
            selectedEvent = {
                eventID: 0,
                title: '',
                description: '',
                start: start,
                end: end,
                allDay: false,
                color: ''
            };
            let startDate = new Date(start);
            let endDate = new Date(end.subtract(1, 'days'));
            let month = startDate.getMonth();
            let quarter = findQuarter(month + 1);
            let date = new Date();

            if (jsEvent && jsEvent.shiftKey) {

                let dateToAdd = moment(new Date(startDate)).format('YYYY-MM-DD');
                AddSelectedDates.push(dateToAdd);

                let tempSelectedColorDates = [];

                if (!addSelectedColorDates.find(date => date.start == dateToAdd)) {
                    tempSelectedColorDates.push({
                        start: dateToAdd,
                        backgroundColor: 'yellow',
                    });
                    addSelectedColorDates.push({
                        start: dateToAdd,
                        backgroundColor: 'yellow',
                    });
                }
                $('#EmployeeWorkingHoursCalender').fullCalendar('removeEvents', tempSelectedColorDates);
                $('#EmployeeWorkingHoursCalender').fullCalendar('addEventSource', tempSelectedColorDates);
            }

            if (start < date) {
                if ((startDate.toDateString() == endDate.toDateString()) && !jsEvent.shiftKey) {
                    var GetEmployeeDetailsRequest = {
                        FacilityId: $('#FacilityId').val(),
                        EmployeeId: $('#EmployeeIdWorking').val()
                    }
                    $.ajax({
                        url: '/PBJSnap/EmployeeMaster/GetEmployeeById',
                        type: 'POST',
                        data: GetEmployeeDetailsRequest,
                        success: function (result) {
                            hideLoader();
                            $('#EmpHoursWorked').val('');
                            $('#EmpTimesheetId').val('');
                            $('#EmpWorkDate').val(start.format("MM/DD/YYYY"));
                            $('#AddWorkingHoursLabel').html('Add working details');
                            $('#EmpHoursWorkedError').html('');

                            $('#EmpWorkingPayType').val(result.payTypeCode);
                            $('#EmpWorkingPayType').trigger('change');

                            $('#EmpWorkingJobTitle').val(result.jobTitleCode);
                            $('#EmpWorkingJobTitle').trigger('change');
                            $('#btnSaveEmployeeWorkingHours').removeClass('hide');
                            $('#btnDeleteEmployeeWorkingHours').addClass('hide');
                            $('#EmpAddUpdateWorkingHours').modal('show');
                            draggableModel('#EmpAddUpdateWorkingHours');
                            $('#EmployeeWorkingHoursCalender').fullCalendar('unselect');

                        },
                        error: function (result) {
                            hideLoader();
                            ErrorMsg("Alert!", "Failed to fetch");
                        }
                    });

                }
                else if (startDate.toDateString() != endDate.toDateString()) {
                    addMultipleDaysArray(startDate, endDate);
                    $('#EmployeeWorkingHoursCalender').fullCalendar('removeEvents', addSelectedColorDates);
                    $('#EmployeeWorkingHoursCalender').fullCalendar('addEventSource', addSelectedColorDates);
                }
            }
            else {
                ErrorMsg("Warning!", "You have selected the future date!");
            }
        },
        editable: true,
        eventDrop: function (event) {
        },
        viewRender: function (view, element) {
            events = [];
            var EmployeeTimesheetDataRequest = {
                EmployeeId: $('#EmployeeIdWorking').val(),
                FacilityId: $('#FacilityId').val(),
                ReportQuarter: $('#QuarterList').val(),
                Year: $('#YearList').val(),
                Month: $('#MonthList').val(),
                StartDate: moment(view.start).format("YYYY-MM-DD"),
                EndDate: moment(view.end).format("YYYY-MM-DD")
            }
            $.ajax({
                url: urls.Review.GetEmployeeTimesheet,
                type: 'POST',
                data: EmployeeTimesheetDataRequest,
                success: function (result) {
                    $.each(result, function (i, v) {
                        events.push({
                            eventID: v.timesheetId,
                            title: v.totalHours,
                            description: v.employeeId,
                            start: moment(v.workday),
                            end: v.end != null ? moment(v.workday) : null,
                            color: v.color,
                            editable: v.editable,
                            isButtonEditable: v.isButtonEditable
                        });
                    })

                    $('#EmployeeWorkingHoursCalender').fullCalendar('removeEvents');
                    $('#EmployeeWorkingHoursCalender').fullCalendar('addEventSource', events);
                    $('#EmployeeWorkingHoursCalender').fullCalendar('addEventSource', addSelectedColorDates);
                },
                error: function (result) {
                    ErrorMsg("Alert!", "Failed to load");
                }
            });

        }
    })
}

function getPayTypeList() {
    $.ajax({
        type: "GET",
        url: urls.PayTypeJobTitle.GetPayCode,
        success: function (data) {
            $('#EmpWorkingPayType').html();
            GetPayCodeList = data;
            var PayType = "";
            $.each(data, function (i, item) {
                PayType += "<option value='" + item.payTypeCode + "'>" + item.payTypeDescription + "</option>";
            })
            $('#EmpWorkingPayType').html(PayType);
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
            $('#EmpWorkingJobTitle').html();
            GetJobTitleList = data;
            var JobTitle = "";
            $.each(data, function (i, item) {
                JobTitle += "<option value='" + item.id + "'>" + item.title + "</option>";
            })
            $('#EmpWorkingJobTitle').html(JobTitle);
        },
        error: function (error) {
            ErrorMsg("Alert!", "Failed to load");
        }
    })
}

function SaveEmployeeWorkingHours() {
    if ($('#EmpHoursWorked').val().trim() == '') {
        $('#EmpHoursWorkedError').html('Please enter work hours!');
        return false;
    }
    let totalHours = parseInt($('#EmpHoursWorked').val().trim());
    if (totalHours <= 0 || totalHours > 24) {
        $('#EmpHoursWorkedError').html('Please enter valid work hours!');
        return false;
    }
    var EmployeeTimesheetSaveDataRequest = {
        TimesheetId: $('#EmpTimesheetId').val(),
        EmployeeId: $('#EmployeeIdWorking').val(),
        FacilityId: $('#FacilityId').val(),
        ReportQuarter: $('#QuarterList').val(),
        Month: $('#MonthList').val(),
        Year: $('#YearList').val(),
        PayCode: $('#EmpWorkingPayType').val(),
        JobTitle: $('#EmpWorkingJobTitle').val(),
        UploadType: $('#UploadType').val(),
        TotalHours: $('#EmpHoursWorked').val(),
        WorkDate: $('#EmpWorkDate').val()
    }
    showLoader();
    $.ajax({
        url: urls.Review.SaveEmployeeWorkingDetails,
        type: 'POST',
        data: EmployeeTimesheetSaveDataRequest,
        success: function (result) {
            hideLoader();
            if (result == '24hours') {
                ErrorMsg("Alert!", "The total hours is more than 24 hours");
                return false;
            }
            SuccessMsg("Success", "Record saved successfully", "success");
            $("#EmpAddUpdateWorkingHours").modal('hide');
            employeeWorkingDataClear();
            openEmployeeCalender($('#EmployeeIdWorking').val());
            loadStaffDeptData();
        },
        error: function (result) {
            hideLoader();
            ErrorMsg("Alert!", "Failed to load");
        }
    });
}

function employeeWorkingDataClear() {
    $('#EmpTimesheetId').val('');
    $('#EmpHoursWorked').val('');
}

function loadEmployeeCalenderData() {
    //add
}

function multipleAddUpdateWorking() {
    let len = AddSelectedDates.length;
    if (len == 0) {
        $('#SelectDateIdError').html('Please select the date.');
        return false;
    }
    if ($('#Emp_HoursWorked').val().trim() == 0) {
        $('#Emp_HoursWorkedError').html('Please enter the working hours.');
        return false;
    }
    let totalHours = parseInt($('#Emp_HoursWorked').val().trim());
    if (totalHours <= 0 || totalHours > 24) {
        $('#Emp_HoursWorkedError').html('Please enter valid work hours!');
        return false;
    }

    var EmployeeMultiDatesSaveRequest = {
        EmployeeId: $('#EmployeeIdWorking').val(),
        FacilityId: $('#FacilityId').val(),
        Year: $('#YearList').val(),
        ReportQuarter: $('#QuarterList').val(),
        Month: $('#MonthList').val(),
        MultipleDates: AddSelectedDates,
        PayCode: $('#Emp_WorkingPayType').val(),
        JobTitle: $('#Emp_WorkingJobTitle').val(),
        WorkingHours: $('#Emp_HoursWorked').val().trim()
    }
    showLoader();
    $.ajax({
        url: urls.Review.EmployeeMultiDatesSave,
        type: 'POST',
        data: EmployeeMultiDatesSaveRequest,
        success: function (result) {
            hideLoader();
            if (result != "") {
                WarningMsg("Alert!", "These many dates " + result + " are having total hours greater then 24 ", "Okay");
            } else {
                SuccessMsg("Success", "Record saved successfully", "success");
            }
            addSelectedColorDates = [];
            AddSelectedDates = [];
            $("#AddUpdateMultipleEmpWorking").modal('hide');
            openEmployeeCalender($('#EmployeeIdWorking').val());
            loadStaffDeptData();
        },
        error: function (result) {
            hideLoader();
            ErrorMsg("Alert!", "Failed to saved");
        }
    });

}

$(function () {
    $('#EmpDateRange').daterangepicker();
});

$(function () {
    $('#EmpDateRange').daterangepicker({
        opens: 'left'
    }, function (start, end, label) {
        /*AddSelectedDates = [];
        var startDate = new Date(start.format('YYYY-MM-DD'));
        var endDate = new Date(end.format('YYYY-MM-DD'));
        var currentDate = new Date(startDate);
        while (currentDate <= endDate) {
            var formattedDate = currentDate.toISOString().slice(0, 10);
            AddSelectedDates.push(formattedDate)
            currentDate.setDate(currentDate.getDate() + 1);
        }*/
    });
});

function OpenMultipleDateRangeModal() {
    $('#Emp_WorkingPayType').html('');
    $('#Emp_WorkingJobTitle').html('');
    var PayType = "";
    var JobTitle = "";

    var GetEmployeeDetailsRequest = {
        FacilityId: $('#FacilityId').val(),
        EmployeeId: $('#EmployeeIdWorking').val()
    }
    $.ajax({
        url: '/PBJSnap/EmployeeMaster/GetEmployeeById',
        type: 'POST',
        data: GetEmployeeDetailsRequest,
        success: function (result) {
            hideLoader();
            $.each(GetPayCodeList, function (i, item) {
                PayType += "<option value='" + item.payTypeCode + "'>" + item.payTypeDescription + "</option>";
            })
            $.each(GetJobTitleList, function (i, item) {
                JobTitle += "<option value='" + item.id + "'>" + item.title + "</option>";
            })
            $('#Emp_WorkingPayType').html(PayType);
            $('#Emp_WorkingPayType').val(result.payTypeCode);
            $('#Emp_WorkingPayType').trigger('change');

            $('#Emp_WorkingJobTitle').html(JobTitle);
            $('#Emp_WorkingJobTitle').val(result.jobTitleCode);
            $('#Emp_WorkingJobTitle').trigger('change');

            $('#Emp_HoursWorked').val('');
            if (AddSelectedDates.length == 0) {
                AddSelectedDates = [];
            }
            arrDateList();
            $('#AddUpdateMultipleEmpWorking').modal('show');
            draggableModel('#AddUpdateMultipleEmpWorking');
        },
        complete: {

        },
        error: function (result) {
            hideLoader();
            ErrorMsg("Alert!", "Failed to fetch");
        }
    });

}

function AddDatesFunction() {
    var count = 0;
    var selectedDate = $('#SelectDateId').val();
    let date = moment(new Date()).format("YYYY-MM-DD");
    if (selectedDate != '') {
        $.each(AddSelectedDates, function (i, item) {
            if (item == selectedDate) {
                count++;
            }
        });
        if (count == 0) {
            if (selectedDate < date) {
                AddSelectedDates.push(selectedDate);
                arrDateList();
            } else {
                $('#SelectDateIdError').html('You have selected the future date.');
            }
        } else {
            $('#SelectDateIdError').html('You have already selected the date.');
        }
    }
    else {
        $('#SelectDateIdError').html('Please select the date.');
    }
}

function removeDates(removeDate) {
    AddSelectedDates.splice($.inArray(removeDate, AddSelectedDates), 1);
    let index = addSelectedColorDates.findIndex(element => element.start === removeDate);
    addSelectedColorDates.splice(index, 1);
    arrDateList();
    GenerateEmployeeCalender();
}

function arrDateList() {
    $('#SelectDateIdError').html('');
    $("#divDateList").html('');
    AddSelectedDates = AddSelectedDates.filter((value, index, self) => self.indexOf(value) === index);
    $.each(AddSelectedDates, function (i, item) {
        $("#divDateList").append("<span class='mb-0 mt-1 me-2 badge font-medium bg-light-primary text-primary' onclick='removeDates((\"" + item + "\"))' >" + item + '</span>');
    });
}

$(function () {
    $('#EmpDateRangeCalender').daterangepicker();
});
$(function () {
    $('#EmpDateRangeCalender').daterangepicker({
        opens: 'left',
        maxDate: new Date()
    }, function (start, end, label) {
        AddSelectedDates = [];
        let date = new Date();
        var startDate = new Date(start.format('YYYY-MM-DD'));
        var endDate = new Date(end.format('YYYY-MM-DD'));
        var currentDate = new Date(startDate);
        while (currentDate <= endDate) {
            var formattedDate = currentDate.toISOString().slice(0, 10);
            if (currentDate < date) {
                AddSelectedDates.push(formattedDate);
            }
            currentDate.setDate(currentDate.getDate() + 1);
        }
    });
});

function switchToEmpConsecutiveDate() {
    AddSelectedDates = [];
    if ($('#SwitchEmpConsecutiveDateToBasicDate').is(':checked')) {
        $('#SwitchEmpConsecutiveDateToBasicDate').val(1);
        $('#EmpWorkDateRangeDiv').css('display', 'block');
        $('#EmpWorkDateNonConsecutiveDiv').css('display', 'none');
    } else {
        $('#SwitchEmpConsecutiveDateToBasicDate').val(0);
        $('#EmpWorkDateRangeDiv').css('display', 'none');
        $('#EmpWorkDateNonConsecutiveDiv').css('display', 'block');
    }
}

//start Multiple delete records

function OpenMultipleDateRangeDeleteModal() {
    DeleteSelectedDates = [];
    arrDeleteDateList();
    $('#DeleteMultipleEmpWorking').modal('show');
    draggableModel('#DeleteMultipleEmpWorking');
}

var DeleteSelectedDates = [];
function DeleteDatesFunction() {
    var count = 0;
    var selectedDate = $('#DeleteSelectDateId').val();
    if (selectedDate != '') {
        $.each(DeleteSelectedDates, function (i, item) {
            if (item == selectedDate) {
                count++;
            }
        });
        if (count == 0) {
            DeleteSelectedDates.push(selectedDate);
            arrDeleteDateList();
        } else {
            $('#DeleteSelectDateIdError').html('You have already selected the date.');
        }
    }
    else {
        $('#DeleteSelectDateIdError').html('Please select the date.');
    }
}

function arrDeleteDateList() {
    $('#DeleteSelectDateIdError').html('');
    $("#divDeleteDateList").html('');
    $.each(DeleteSelectedDates, function (i, item) {
        $("#divDeleteDateList").append("<span class='mb-0 mt-1 me-2 badge font-medium bg-light-primary text-primary' onclick='removeDeleteDates((\"" + item + "\"))' >" + item + '</span>');
    });
}

function removeDeleteDates(removeDate) {
    DeleteSelectedDates.splice($.inArray(removeDate, DeleteSelectedDates), 1);
    arrDeleteDateList();
}

function deleteMultipleWorking() {
    if (DeleteSelectedDates.length == 0) {
        $('#DeleteSelectDateIdError').html('Please select and add the date.');
        return false;
    }
    ConfirmMsg('Delete working hours', 'Are you sure want to delete the data?', 'Delete', event, function () {

        var EmployeeMultiDatesSaveRequest = {
            EmployeeId: $('#EmployeeIdWorking').val(),
            FacilityId: $('#FacilityId').val(),
            Year: $('#YearList').val(),
            ReportQuarter: $('#QuarterList').val(),
            Month: $('#MonthList').val(),
            MultipleDates: DeleteSelectedDates
        }
        showLoader();
        $.ajax({
            url: urls.Review.DeleteMultipleWorkingHoursDates,
            type: 'POST',
            data: EmployeeMultiDatesSaveRequest,
            success: function (result) {
                hideLoader();
                SuccessMsg("Success", "Record deleted successfully", "success");
                DeleteSelectedDates = [];
                $("#DeleteMultipleEmpWorking").modal('hide');
                openEmployeeCalender($('#EmployeeIdWorking').val());
                loadStaffDeptData();
            },
            error: function (result) {
                hideLoader();
                ErrorMsg("Alert!", "Failed to delete the data!");
            }
        });
    });

}

$(function () {
    $('#EmpDeleteDateRangeCalender').daterangepicker();
});
$(function () {
    $('#EmpDeleteDateRangeCalender').daterangepicker({
        opens: 'left',
        maxDate: new Date()
    }, function (start, end, label) {
        DeleteSelectedDates = [];
        var startDate = new Date(start.format('YYYY-MM-DD'));
        var endDate = new Date(end.format('YYYY-MM-DD'));
        var currentDate = new Date(startDate);
        while (currentDate <= endDate) {
            var formattedDate = currentDate.toISOString().slice(0, 10);
            DeleteSelectedDates.push(formattedDate)
            currentDate.setDate(currentDate.getDate() + 1);
        }
    });
});

function switchToDeleteEmpConsecutiveDate() {
    DeleteSelectedDates = [];
    if ($('#SwitchDeleteEmpConsecutiveDateToBasicDate').is(':checked')) {
        $('#SwitchDeleteEmpConsecutiveDateToBasicDate').val(1);
        $('#EmpDeleteWorkDateRangeDiv').css('display', 'block');
        $('#EmpDeleteWorkDateNonConsecutiveDiv').css('display', 'none');
    } else {
        $('#SwitchDeleteEmpConsecutiveDateToBasicDate').val(0);
        $('#EmpDeleteWorkDateRangeDiv').css('display', 'none');
        $('#EmpDeleteWorkDateNonConsecutiveDiv').css('display', 'block');
    }
}


//End Multiple delete records

//Approve Timesheet data
function approveData() {
    ConfirmMsg('Approve', 'Are you sure want to approve the data?', 'Approve', event, function () {
        var ApproveTimesheetRequest = {
            FacilityId: $('#FacilityId').val(),
            Year: $('#YearList').val(),
            ReportQuarter: $('#QuarterList').val(),
            Month: $('#MonthList').val(),
            IsApproved: false
        }
        showLoader();
        $.ajax({
            type: "POST",
            url: urls.Review.ApproveUploadData,
            data: ApproveTimesheetRequest,
            success: function (data) {
                hideLoader();
                if (data.success) {
                    if (data.message == "NotValidated") {
                        ConfirmMsg('Warning', 'There are pending data need to validate? Do you want to continue to approve the data.',
                            'Approve', event, function () {
                                reApproveData(true);
                            });
                    } else {
                        SuccessMsg("Success", data.message, "success");
                    }

                } else {
                    ErrorMsg("Alert!", data.message);
                }

            },
            error: function (error) {
                hideLoader();
                ErrorMsg("Alert!", "Failed to approve the data!");
            }
        })
    });
}

function reApproveData(isApprove) {
    var ApproveTimesheetRequest = {
        FacilityId: $('#FacilityId').val(),
        Year: $('#YearList').val(),
        ReportQuarter: $('#QuarterList').val(),
        Month: $('#MonthList').val(),
        IsApproved: isApprove
    }
    showLoader();
    $.ajax({
        type: "POST",
        url: urls.Review.ApproveUploadData,
        data: ApproveTimesheetRequest,
        success: function (data) {
            hideLoader();
            if (data.success) {
                SuccessMsg("Success", data.message, "success");
            } else {
                ErrorMsg("Alert!", data.message);
            }
        },
        error: function (error) {
            hideLoader();
            ErrorMsg("Alert!", "Failed to approve the data!");
        }
    })
}

//End Approve Timesheet data

//Add Data for Registered Nurse

var calenderEvents = [];
var calenderSelectedEvent = null;
var TempJobTitle = '';
function openNurseGenerateCalenderModalPopup(JobTitle) {
    calenderEvents = [];
    TempJobTitle = JobTitle;
    let startDate = moment(new Date).format("YYYY-MM-01");
    let endDate = moment(new Date).format("YYYY-MM-dd");
    var GetMissingNursesDates = {
        FacilityId: $('#FacilityId').val(),
        Year: $('#YearList').val(),
        ReportQuarter: $('#QuarterList').val(),
        Month: $('#MonthList').val(),
        JobTitle: JobTitle
    }
    $.ajax({
        type: "POST",
        url: urls.Review.GetMissingNursesDates,
        data: GetMissingNursesDates,
        success: function (data) {
            $.each(data, function (i, v) {
                calenderEvents.push({
                    eventID: '',
                    title: moment(v.missingDate).format("MM-DD-YYYY"),
                    description: '',
                    start: moment(v.missingDate),
                    end: v.end != null ? moment(v.missingDate) : null,
                    color: "Red",
                    editable: v.isEditable
                });
            })
            $('#ReviewModal').modal('show');
            draggableModel('#ReviewModal');
            GenerateNurseCalender(calenderEvents, JobTitle);
        },
        error: function (error) {
            ErrorMsg("Alert!", "Failed to load!");
        }
    })
}

function GenerateNurseCalender(events, JobTitle) {
    $('#calender').fullCalendar('destroy');
    $('#calender').fullCalendar({
        contentHeight: 350,
        defaultDate: new Date(),
        timeFormat: 'h(:mm)a',
        displayEventTime: false,
        header: {
            left: 'prev,next today',
            center: 'title',
            right: 'month,basicWeek,basicDay'
        },
        eventLimit: true,
        eventColor: '#378006',
        events: events,
        buttonText: {
            today: 'Today',
            month: 'Month',
            week: 'Week',
            day: 'Day'
        },
        eventClick: function (calEvent, jsEvent, view) {
            let date = new Date();
            if (calEvent.start < date) {
                selectedEvent = calEvent;
                $('#NurseEmployeeId').val('');
                $('#NurseWorkDay').val(calEvent.start.format("YYYY-MM-DD"));
                openNurseCalenderPopup(JobTitle);
            } else {
                ErrorMsg("Warning!", "You have selected the future date!");
            }
        },
        selectable: true,
        select: function (start, end) {

        },
        editable: true,
        eventDrop: function (event) {
            var data = {
                EventID: event.eventID,
                Subject: event.title,
                Start: event.start.format('DD/MM/YYYY HH:mm A'),
                End: event.end != null ? event.end.format('DD/MM/YYYY HH:mm A') : null,
                Description: event.description,
                ThemeColor: event.color,
                IsFullDay: event.allDay
            };

        },
        viewRender: function (view, element) {
            events = [];
            var GetMissingNursesDates = {
                FacilityId: $('#FacilityId').val(),
                Year: $('#YearList').val(),
                ReportQuarter: $('#QuarterList').val(),
                Month: $('#MonthList').val(),
                JobTitle: JobTitle,
                StartDate: moment(view.start).format("YYYY-MM-DD"),
                EndDate: moment(view.end).format("YYYY-MM-DD")
            }
            $.ajax({
                url: urls.Review.GetMissingNursesDates,
                type: 'POST',
                data: GetMissingNursesDates,
                success: function (result) {
                    $.each(result, function (i, v) {
                        events.push({
                            eventID: '',
                            title: moment(v.missingDate).format("MM-DD-YYYY"),
                            description: '',
                            start: moment(v.missingDate),
                            end: v.end != null ? moment(v.missingDate) : null,
                            color: "Red",
                            editable: v.isEditable
                        });
                    })

                    $('#calender').fullCalendar('removeEvents');
                    $('#calender').fullCalendar('addEventSource', events);
                },
                error: function (result) {
                    ErrorMsg("Alert!", "Failed to load");
                }
            });


        }
    })
}

function SaveNurseMissingWorkingHoursDates() {
    if ($('#NurseEmployeeId').val().trim() == 0) {
        $('#NurseEmployeeIdError').html('Please enter Employee Id!');
        return false;
    }
    if (AddNurseSelectedDates.length == 0) {
        $('#NurseWorkDayError').html('Please select the date!');
        return false;
    }
    if ($('#NurseHoursWorked').val().trim() == '') {
        $('#NurseHoursWorkedError').html('Please enter work hours!');
        return false;
    }
    let totalHours = parseInt($('#NurseHoursWorked').val().trim());
    if (totalHours <= 0 || totalHours > 24) {
        $('#NurseHoursWorkedError').html('Please enter valid work hours!');
        return false;
    }

    var EmployeeTimesheetSaveDataRequest = {
        EmployeeId: $('#NurseEmployeeId').val(),
        FacilityId: $('#FacilityId').val(),
        ReportQuarter: $('#QuarterList').val(),
        Month: $('#MonthList').val(),
        Year: $('#YearList').val(),
        PayCode: $('#NurseWorkingPayType').val(),
        JobTitle: $('#NurseWorkingJobTitle').val(),
        UploadType: $('#UploadType').val(),
        TotalHours: $('#NurseHoursWorked').val(),
        WorkDateList: AddNurseSelectedDates
    }
    showLoader();
    $.ajax({
        url: urls.Review.AddMissingNurseHoursData,
        type: 'POST',
        data: EmployeeTimesheetSaveDataRequest,
        success: function (result) {
            hideLoader();
            SuccessMsg("Success", "Record saved successfully", "success");
            $('#AddNurseWorkingHours').modal("hide");
            openNurseGenerateCalenderModalPopup(TempJobTitle);
            loadStaffDeptData();
        },
        error: function (result) {
            hideLoader();
            if (result.responseText == 'EmployeeNotFound') {
                ErrorMsg('Error', 'Invalid Employee Id', '');
                return false;
            }
            ErrorMsg('Error', Failed, '');
        }
    });
}

var AddNurseSelectedDates = [];
function openNurseCalenderPopup(jobTitleValue) {
    $('#NurseWorkingPayType').html('');
    $('#NurseWorkingJobTitle').html('');
    var PayType = "";
    var JobTitle = "";
    $.each(GetPayCodeList, function (i, item) {
        PayType += "<option value='" + item.payTypeCode + "'>" + item.payTypeDescription + "</option>";
    })
    $.each(GetJobTitleList, function (i, item) {
        JobTitle += "<option value='" + item.id + "'>" + item.title + "</option>";
    })
    AddNurseSelectedDates = [];
    $('#NurseWorkingPayType').html(PayType);
    $('#NurseWorkingJobTitle').html(JobTitle);
    $('#NurseWorkingJobTitle').val(jobTitleValue);
    $('#NurseWorkingJobTitle').trigger('change');
    $('#NurseHoursWorked').val('');
    $('#NurseEmployeeIdError').html('');
    $('#NurseDateRangeError').html('');
    $('#NurseWorkDayError').html('');
    $('#NurseHoursWorkedError').html('');
    $('#NurseWorkingPayTypeError').html('');
    $('#NurseWorkingJobTitleError').html('');
    $('#NurseEmployeeId').val(null).trigger('change');
    $('#AddNurseWorkingHours').modal("show");
}

function SwitchToConsecutiveDate() {
    AddNurseSelectedDates = [];
    if ($('#SwitchNurseConsecutiveDateToBasicDate').is(':checked')) {
        $('#SwitchNurseConsecutiveDateToBasicDate').val(1);
        $('#NurseDateRangeDiv').css('display', 'block');
        $('#NurseWorkDayDiv').css('display', 'none');
    } else {
        $('#SwitchNurseConsecutiveDateToBasicDate').val(0);
        $('#NurseDateRangeDiv').css('display', 'none');
        $('#NurseWorkDayDiv').css('display', 'block');
    }
    arrNurseDateList();
}
function addNurseNonConsecutiveDatesFunction() {
    var count = 0;
    var selectedDate = $('#NurseWorkDay').val();
    let date = moment(new Date()).format("YYYY-MM-DD");
    if (selectedDate != '') {
        $.each(AddNurseSelectedDates, function (i, item) {
            if (item == selectedDate) {
                count++;
            }
        });
        if (count == 0) {
            if (selectedDate < date) {
                AddNurseSelectedDates.push(selectedDate);
                arrNurseDateList();
            } else {
                $('#NurseWorkDayError').html('You have already selected the future date.');
            }
        } else {
            $('#NurseWorkDayError').html('You have already selected the date.');
        }
    }
    else {
        $('#NurseWorkDayError').html('Please select the date.');
    }
}

function removeNurseNonConsectiveDate(removeDate) {
    AddNurseSelectedDates.splice($.inArray(removeDate, AddNurseSelectedDates), 1);
    arrNurseDateList();
}

function arrNurseDateList() {
    $('#NurseWorkDayError').html('');
    $("#divNurseDateList").html('');
    $.each(AddNurseSelectedDates, function (i, item) {
        $("#divNurseDateList").append("<span class='mb-0 mt-1 me-2 badge font-medium bg-light-primary text-primary' onclick='removeNurseNonConsectiveDate((\"" + item + "\"))' >" + item + '</span>');
    });
}

$(function () {
    $('#NurseDateRange').daterangepicker();
});
$(function () {
    $('#NurseDateRange').daterangepicker({
        opens: 'left',
        maxDate: new Date()
    }, function (start, end, label) {
        AddNurseSelectedDates = [];
        let date = new Date();
        var startDate = new Date(start.format('YYYY-MM-DD'));
        var endDate = new Date(end.format('YYYY-MM-DD'));
        var currentDate = new Date(startDate);
        while (currentDate <= endDate) {
            var formattedDate = currentDate.toISOString().slice(0, 10);
            if (currentDate <= date) {
                AddNurseSelectedDates.push(formattedDate);
            }
            currentDate.setDate(currentDate.getDate() + 1);
        }
    });
});

//End Data for Registered Nurse

// CSV download for Cencus and staffingDepartmentdetails
function DownloadCencusCSV() {
    let date = new Date();
    let fileName = "Cencus_" + $('#FacilityId :selected').text() + moment(date).format("YYYYMMDDHHMMSS");
    var CencusCsvRequest = {
        FacilityId: $('#FacilityId').val(),
        Year: $('#YearList').val(),
        ReportQuarter: $('#QuarterList').val(),
        FileName: fileName
    }
    showLoader();
    $.ajax({
        url: urls.Review.ExportCensusToCsv,
        type: 'GET',
        data: CencusCsvRequest,
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data) {
            hideLoader();
            var url = window.URL.createObjectURL(data);
            var link = document.createElement('a');
            link.href = url;
            link.download = fileName + '.csv';
            link.click();

            window.URL.revokeObjectURL(url);
        }
    });
}

function DownloadStaffingDeptCSV() {
    let date = new Date();
    let fileName = "StaffingDepartmentDetails_" + $('#FacilityId :selected').text() + moment(date).format("YYYYMMDDHHMMSS");
    var StaffingDepartmentCsvRequest = {
        FacilityId: $('#FacilityId').val(),
        Year: $('#YearList').val(),
        ReportQuarter: $('#QuarterList').val(),
        FileName: fileName
    }
    showLoader();
    $.ajax({
        url: urls.Review.ExportStaffingDeptToCsv,
        type: 'GET',
        data: StaffingDepartmentCsvRequest,
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data) {
            hideLoader();
            var url = window.URL.createObjectURL(data);
            var link = document.createElement('a');
            link.href = url;
            link.download = fileName + '.csv';
            link.click();

            window.URL.revokeObjectURL(url);
        },
        error: function (result) {
            hideLoader();
            ErrorMsg('Error', 'Failed to export', '');
        }
    });
}


function openStaffExportModal() {
    showLoader();
    var arrPayType = [];
    var arrJobTitle = [];
    var arrEmpIds = [];
    let payType = "";
    let jobType = "";

    let EmployeeListRequest = {
        facilityId: $('#FacilityId').val(),
        Year: $('#YearList').val(),
        ReportQuarter: $('#QuarterList').val(),
        Month: $('#MonthList').val(),
        SearchValue: $('#EmployeeListTable_filter input').val()
    };

    if (EmployeeListRequest.SearchValue != '' && EmployeeListRequest.SearchValue != null) {
        $.ajax({
            type: 'GET',
            data: EmployeeListRequest,
            url: urls.Review.GetSearchedEmployees,
            success: function (response) {
                for (const element of response) {
                    if (!arrPayType.includes(element.payTypeCode)) {
                        payType += "<option value='" + element.payTypeCode + "'>" + element.payType + "</option>";
                        arrPayType.push(element.payTypeCode);
                    }
                    if (!arrJobTitle.includes(element.jobTitleCode)) {
                        jobType += "<option value='" + element.jobTitleCode + "'>" + element.jobTitle + "</option>";
                        arrJobTitle.push(element.jobTitleCode);
                    }
                    arrEmpIds.push(element.employeeId)
                }
                $('#StaffExportPayType').html(payType);
                $('#StaffExportPayType').select2({
                    multiple: true,
                    placeholder: 'Select Pay Type'
                });
                $('#StaffExportPayType').val(arrPayType).trigger('change');
                $('#StaffExportJobTitle').html(jobType);
                $('#StaffExportJobTitle').select2({
                    multiple: true,
                    placeholder: 'Select Job Title'
                });
                $('#StaffExportJobTitle').val(arrJobTitle).trigger('change');

                let jsonArray = JSON.stringify(arrEmpIds);
                sessionStorage.setItem('empIds', jsonArray);
            },
            failure: function () {
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
                $('#preloader').addClass('d-none');
            },
            error: function () {
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
                $('#preloader').addClass('d-none');
            }
        });
    }
    else {
        jobType = "";
        payType = "";
        $('#StaffExportPayType').html();
        $.each(GetPayCodeList, function (i, item) {
            payType += "<option value='" + item.payTypeCode + "'>" + item.payTypeDescription + "</option>";
            arrPayType.push(item.payTypeCode);
        })
        $('#StaffExportPayType').html(payType);

        $('#StaffExportPayType').select2({
            multiple: true,
            placeholder: 'Select Pay Type'
        });
        $('#StaffExportPayType').val(arrPayType).trigger('change');
        $('#StaffExportJobTitle').html();
        $.each(GetJobTitleList, function (i, item) {
            jobType += "<option value='" + item.id + "'>" + item.title + "</option>";
            arrJobTitle.push(item.id);
        })

        $('#StaffExportJobTitle').html(jobType);
        $('#StaffExportJobTitle').select2({
            multiple: true,
            placeholder: 'Select Job Title'
        });
        $('#StaffExportJobTitle').val(arrJobTitle).trigger('change');
    }
    hideLoader();
    $('#ExportEmpolyeeModal').modal('show');
}

function getEmployeeName(employeeId) {
    let empName = '';
    $.ajax({
        url: urls.Review.GetEmployeeName,
        type: 'GET',
        data: { EmployeeId: employeeId },
        async: false,
        success: function (data) {
            if (data) {
                empName = data;
            }
            else {
                empName = '';
            }
        },
        error: function (result) {
            ErrorMsg('Error', 'Failed to Get Employee Name', '');
        }
    });

    return empName;
}

function initSelect2Review() {
    $('#EmpWorkingPayType').select2({
        dropdownParent: $('#EmpWorkingPayTypeDiv'),
        placeholder: "Select Pay Type",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');


        var offset = $(this).offset().top - dropdown.outerHeight();
        dropdown.css({
            'top': offset + 'px',
            'left': '0',
            'right': '0'
        });

    });

    $('#EmpWorkingJobTitle').select2({
        dropdownParent: $('#EmpWorkingJobTitleDiv'),
        placeholder: "Select Pay Type",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

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
        placeholder: "Select Pay Type",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#NurseWorkingPayType').select2({
        dropdownParent: $('#NurseWorkingPayTypeDiv'),
        placeholder: "Select Pay Type",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#NurseWorkingJobTitle').select2({
        dropdownParent: $('#NurseWorkingJobTitleDiv'),
        placeholder: "Select Pay Type",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#Emp_WorkingPayType').select2({
        dropdownParent: $('#Emp_WorkingPayTypeDiv'),
        placeholder: "Select Pay Type",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#Emp_WorkingJobTitle').select2({
        dropdownParent: $('#Emp_WorkingJobTitleDiv'),
        placeholder: "Select Pay Type",
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#NurseEmployeeId').select2({
        dropdownParent: $('#NurseEmployeeIdDiv'),
        placeholder: "Search employee id",
        width: '100%',
        ajax: {
            url: 'Review/GetEmployeeIdAndName',
            dataType: 'json',
            type: 'POST',
            delay: 250,
            data: function (params) {
                return {
                    term: params.term,
                    FacilityId: $('#FacilityId').val()
                };
            },
            processResults: function (data) {
                return {
                    results: data.items
                };
            },
            cache: true
        }
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });
}

function checkIfValidated() {
    let data = new FormData();
    data.append("FacilityId", $('#FacilityId').val());
    data.append("ReportQuarter", $('#QuarterList').val());
    data.append("Year", $('#YearList').val());
    //data.append("Month", $('#MonthList').val());

    $.ajax({
        url: urls.Review.CheckIfValidated,
        type: 'POST',
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            if (result.success) {
                if (result.fullName.length > 0 && result.validationDate.length > 0) {
                    $('#validateDiv').show();
                    $('#validationUserFullName').text(result.fullName);
                    $('#validationUserFullName').css('display', 'block');
                    $('#validationValidationDate').text(result.validationDate);
                    $('#validationValidationDate').css('display', 'block');
                }
                else {
                    $('#validationUserFullName').hide();
                    $('#validationValidationDate').hide();
                }
            }
            else {
                $('#validateDiv').hide();
                $('#validationUserFullName').hide();
                $('#validationValidationDate').hide();
            }
        },
        error: function (result) {
        }
    });
}

function checkIfApproved() {
    let data = new FormData();
    data.append("FacilityId", $('#FacilityId').val());
    data.append("ReportQuarter", $('#QuarterList').val());
    data.append("Year", $('#YearList').val());
    //data.append("Month", $('#MonthList').val());

    $.ajax({
        url: urls.Review.CheckIfApproved,
        type: 'POST',
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            if (result.success) {
                if (result.fullName.length > 0 && result.approvedDate.length > 0) {
                    $('#approvedDiv').show();
                    $('#approvedUserFullName').text(result.fullName);
                    $('#approvedUserFullName').css('display', 'block');
                    $('#approvedDate').text(result.approvedDate);
                    $('#approvedDate').css('display', 'block');
                }
                else {
                    $('#approvedUserFullName').hide();
                    $('#approvedDate').hide();
                }
            }
            else {
                $('#approvedDiv').hide();
                $('#approvedUserFullName').hide();
                $('#approvedDate').hide();
            }
        },
        error: function (result) {
        }
    });
}

function autoselect(id) {
    var auto = $("#FacilityId");
    auto.val(id).trigger('change');
}

function validateData() {
    ConfirmMsg('Validate', 'Are you sure want to validate the data?', 'Validate', event, function () {
        var ApproveTimesheetRequest = {
            FacilityId: $('#FacilityId').val(),
            Year: $('#YearList').val(),
            ReportQuarter: $('#QuarterList').val(),
            Month: $('#MonthList').val()
        }
        showLoader();
        $.ajax({
            type: "POST",
            url: urls.Review.Validate,
            data: ApproveTimesheetRequest,
            success: function (data) {
                hideLoader();
                /* SuccessMsg("Success", "Record Validated successfully", "success");*/
                if (data.success) {
                    SuccessMsg("Success", data.message, "success");
                } else {
                    ErrorMsg("Alert!", data.message);
                }
                checkIfValidated();
            },
            error: function (error) {
                hideLoader();
                ErrorMsg("Alert!", "Failed to approve the data!");
            }
        })
    });
}
function autoselect(id) {
    var auto = $("#FacilityId");
    auto.val(id).trigger('change');
}

function exportStaff() {
    let payType = $('#StaffExportPayType').val();
    let jobTitle = $('#StaffExportJobTitle').val();
    let selectedId = $('input[name="radioFormatType"]:checked').attr('id');
    let jsonArray = sessionStorage.getItem('empIds');
    let myArray = JSON.parse(jsonArray);
    let formatType = $('#' + selectedId).val();
    if (payType == '') {
        ErrorMsg("Warning", "Please select Pay Type");
        return false;
    }
    if (jobTitle == '') {
        ErrorMsg("Warning", "Please select Job Title");
        return false;
    }
    let _payType = '';
    let _jobTitle = '';
    let _employeeIds = '';
    $.each(payType, function (i, item) {
        _payType += item + ',';
    });
    $.each(jobTitle, function (i, item) {
        _jobTitle += item + ',';
    });
    $.each(myArray, function (i, item) {
        _employeeIds += item + ',';
    });
    let date = new Date();
    let fileName = "StaffExport_" + $('#FacilityId :selected').text() + moment(date).format("YYYYMMDDHHMMSS");
    let EmployeeExportRequest = {
        payType: _payType,
        jobTitle: _jobTitle,
        FormatType: formatType,
        Year: $('#YearList').val(),
        Quarter: $('#QuarterList').val(),
        Month: $('#MonthList').val(),
        FacilityId: $('#FacilityId').val(),
        Filename: fileName,
        EmployeeIds: _employeeIds
    }
    showLoader();
    $.ajax({
        url: urls.Review.ExportStaff,
        type: 'GET',
        data: EmployeeExportRequest,
        contentType: 'application/json',
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data) {
            hideLoader();
            if (formatType == "1") {
                var url = window.URL.createObjectURL(data);
                var link = document.createElement('a');
                link.href = url;
                link.download = fileName + '.csv';
                link.click();
                window.URL.revokeObjectURL(url);
            } else {
                var blob = new Blob([data], { type: 'application/pdf' });
                var url = window.URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.style.display = 'none';
                a.href = url;
                a.download = fileName + '.pdf';
                document.body.appendChild(a);
                a.click();

                // Clean up
                window.URL.revokeObjectURL(url);
            }

        },
        error: function (result) {
            hideLoader();
            ErrorMsg('Error', 'Failed to export', '');
        }
    });

}
//End

function validationErrorsList(_pageNo = 1) {

    if ($('#FileNameList').val() == '0') {
        ErrorMsg("Alert", "Please select the file name");
        return false;
    }
    var ValidationErrorListRequest = {
        FileDetailId: $('#FileNameList').val(),
        PageNo: _pageNo,
        PageSize: $('#PageSizeddl').val()
    }
    showLoader();
    $('#fileDetailsLabel').addClass("hide");
    $('#FacilityNameLabel').html('');
    $('#CreateDateLabel').html('');
    $('#UploadedByLabel').html('');
    $('#YearByLabel').html('');
    $('#ReportQuarterLabel').html('');
    $.ajax({
        url: urls.Backout.ListValidationErrorByFileId,
        type: 'POST',
        data: ValidationErrorListRequest,
        dataType: 'json',
        success: function (data) {
            if (data.dynamicDataTableModel.length > 0) {
                $('#fileDetailsLabel').removeClass("hide");
                $('#FacilityNameLabel').html(data.uploadFileDetails.facilityName);
                $('#CreateDateLabel').html(moment(data.uploadFileDetails.uploadedDate).format("YYYY-MM-DD HH:MM"));
                $('#UploadedByLabel').html(data.uploadFileDetails.uploadedBy);
                $('#YearByLabel').html(data.uploadFileDetails.year);
                $('#ReportQuarterLabel').html(data.uploadFileDetails.reportQuarter);

                var columns = Object.keys(data.dynamicDataTableModel[0].dynamicDataTable);
                var table = '<table class="table display table-bordered table-striped no-wrap" style="max-height:150px;overflow-y: auto;">';
                table += '<thead><tr>';
                for (var i = 0; i < columns.length; i++) {
                    if (columns[i] != "ErrorId") {
                        table += '<th>' + columns[i] + '</th>';
                    }
                }
                table += '<th>Action</th></tr></thead><tbody>';

                for (var j = 0; j < data.dynamicDataTableModel.length; j++) {
                    table += '<tr>';
                    for (var k = 0; k < columns.length; k++) {
                        var columnName = columns[k];
                        if (columnName != "ErrorId") {
                            table += '<td>' + data.dynamicDataTableModel[j].dynamicDataTable[columnName] + '</td>';
                        }
                    }
                    table += "<td><a href='#' onclick=deleteValidationErrorRecord('" + data.dynamicDataTableModel[j].dynamicDataTable["ErrorId"] + "'); title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
                    table += " <a href='#' onclick=openErrorDetailListModal('" + data.dynamicDataTableModel[j].dynamicDataTable["ErrorId"] + "')  data-bs-toggle='tooltip'  data-bs-placement='top' title='View'><i class='ri-eye-line fs-6 text-success ms-2'></i></a>";
                    table += "</td></tr>";
                }
                table += '</tbody></table>';

                /* Pagination */
                let totalCount = data.totalCount;
                let pageSize = $('#PageSizeddl').val();
                let pageNo = _pageNo;
                table += '<nav aria-label="Page navigation example">';
                table += '<ul class="pagination">';

                if (pageNo > 1 || (pageNo - 1) > 1) {
                    table += '<li class="page-item">';
                    table += '<a class="page-link link" href="#" aria-label="Previous" onclick=validationErrorsList("' + (pageNo - 1) + '"); >';
                    table += '<span aria-hidden="true">';
                    table += '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="feather feather-chevrons-left feather-sm"><polyline points="11 17 6 12 11 7"></polyline><polyline points="18 17 13 12 18 7"></polyline></svg>';
                    table += '</span>';
                    table += '</a>';
                    table += '</li>';
                }

                let temp = pageNo;
                let count = 1;
                let diff = 3;
                for (let i = temp; i > 0 && count <= 3; i++) {
                    if ((pageNo - diff) > 0) {
                        table += '<li class="page-item"><a class="page-link" href="#" onclick=validationErrorsList("' + (pageNo - diff) + '");>' + (pageNo - diff) + '</a></li>';
                    }
                    count++;
                    diff--;
                }

                temp = pageNo;
                count = 1;
                let add = 0;
                for (let i = temp; i > 0 && count <= 3; i++) {
                    let pageSum = parseInt(pageNo) + parseInt(add);
                    if (pageNo == pageSum) {
                        table += '<li class="page-item" ><a class="page-link link" href="#">' + pageSum + '</a></li>';
                    } else {
                        if (((parseInt(pageSum) - 1) * parseInt(pageSize)) < parseInt(data.totalCount)) {
                            table += '<li class="page-item"><a class="page-link" href="#" onclick=validationErrorsList("' + pageSum + '");>' + pageSum + '</a></li>';
                        }
                    }
                    count++;
                    add++;
                }

                if ((parseInt(pageSize) * parseInt(pageNo)) <= parseInt(totalCount)) {
                    table += '<li class="page-item">';
                    table += '<a class="page-link" href="#" aria-label="Next" onclick=validationErrorsList("' + (pageNo + 1) + '");>';
                    table += '<span aria-hidden="true">';
                    table += '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="feather feather-chevrons-right feather-sm"><polyline points="13 17 18 12 13 7"></polyline><polyline points="6 17 11 12 6 7"></polyline></svg>';
                    table += '</span>';
                    table += '</a>';
                    table += '</li>';
                }

                table += '</ul>';
                table += '</nav>';
                $('#dataTableContainer').html('');
                $('#dataTableContainer').html(table);
                $('#rowCountId').html('Row count :' + data.rowCount + ' of ' + data.totalCount);
                $('#pageCountId').html('Page No. :' + pageNo + ' of ' + calculateTotalPages(data.totalCount, $('#PageSizeddl').val()));
                $('.disabled-li').css({
                    "color": "gray",
                    "cursor": "not-allowed"
                });
            } else {
                $('#rowCountId').html('');
                $('#pageCountId').html('');
                $('#dataTableContainer').html('No data available.');
            }
            hideLoader();
        },
        error: function () {
            hideLoader();
            $('#rowCountId').html('');
            $('#pageCountId').html('');
            $('#dataTableContainer').html('Error fetching data.');
        }
    });
}

function getAllFileNameByFacility() {
    let _hdnFacilityid;
    if ($('#hdnFacilityId').val() != "" && $('#hdnFacilityId').val() != undefined) {
        _hdnFacilityid = $('#hdnFacilityId').val();
    } else {
        _hdnFacilityid = $('#FacilityId').val();
    }
    let FileDetailListRequest = {
        FacilityId: _hdnFacilityid,
        ReportQuarter: $('#QuarterList').val(),
        Year: $('#YearList').val()
    }
    showLoader();
    $.ajax({
        url: urls.Backout.GetAllFileNameByFacility,
        type: 'POST',
        data: FileDetailListRequest,
        success: function (data) {
            $('#FileNameList').html('');
            var fileNameList = '';
            let hdnFileId = $('#hdnFileId').val();
            fileNameList += "<option value='0'> Select filename </option>";
            $.each(data, function (i, item) {
                if (hdnFileId == item.fileNameId) {
                    fileNameList += "<option value='" + item.fileNameId + "' selected>" + item.fileName + "</option>";
                } else {
                    fileNameList += "<option value='" + item.fileNameId + "'>" + item.fileName + "</option>";
                }
            })
            $('#FileNameList').html(fileNameList);
            $('#dataTableContainer').html('');
            $('#rowCountId').html('');
            $('#pageCountId').html('');
            if (hdnFileId != "") {
                $("#ValidationErrorAccordian").click();
                let pageNo = 1;
                validationErrorsList(pageNo);
            }
            hideLoader();
        },
        error: function () {
            hideLoader();
            ErrorMsg("Failed", "Failed to fetch the data");
        }
    });
}

function deleteValidationErrorRecord(ErrorId) {
    ConfirmMsg('Delete', 'Are you sure want to Delete the record?', 'Delete', event, function () {
        let formdata = {
            Id: ErrorId
        }
        showLoader();
        $.ajax({
            url: urls.Backout.DeleteErrorRowData,
            type: 'POST',
            data: formdata,
            success: function (data) {
                if (data.statusCode == 200) {
                    SuccessMsg("Success", data.message, "success");
                    validationErrorsList();
                    hideLoader();
                } else {
                    ErrorMsg("Failed", data.message);
                    hideLoader();
                }
                hideLoader();
            },
            error: function (err) {
                hideLoader();
                ErrorMsg("Failed", "Failed to Delete");
            }
        });
    })
}

function openErrorDetailListModal(ErrorId) {
    let formdata = {
        Id: ErrorId
    }
    showLoader();
    $.ajax({
        url: urls.Backout.GetErrorRowData,
        type: 'POST',
        data: formdata,
        success: function (data) {
            hideLoader();
            $('#RightErrorTab').html('');
            $('#TopWorkdayListTabTableBody').html('');
            $('#lblTotalWorkingHours').html('');
            let employeeId = '';
            if (data.keyValueList.length > 0) {
                let div = '';
                div += "<input type='hidden' id='_errorId' value='" + data.id + "' />";
                div += "<input type='hidden' id='_facilityId' value='" + data.facilityId + "' />";
                div += "<input type='hidden' id='_fileDetailId' value='" + data.fileDetailId + "' />";
                div += "<input type='hidden' id='_reportQuarter' value='" + data.reportQuarter + "' />";
                div += "<input type='hidden' id='_year' value='" + data.year + "' />";
                div += "<input type='hidden' id='_month' value='" + data.month + "' />";
                div += "<input type='hidden' id='_uploadType' value='" + data.uploadType + "' />";
                data.keyValueList.forEach(function (value, key) {
                    if (value.key == "Employee Id") {
                        employeeId = value.value;
                    }
                    div += '<div class="col-sm-6 keyValue">';
                    div += '<span id="_key" style="font-weight: 700;">' + value.key + '</span>';
                    div += "<span><input id='_value' class='form-control my-2' type='text'' value='" + value.value + "' /></span>";
                    div += '</div>';

                });
                div += "<input type='hidden' id='_employeeIdValue' value='" + employeeId + "' />";
                $('#RightErrorTab').html('');
                $('#RightErrorTab').html(div);

                let TopWorkdayList = '';
                if (data.workdayList.length > 0) {
                    let lblTotalWorkingHours = 0;
                    $.each(data.workdayList, function (i, item) {
                        TopWorkdayList += '<tr>';
                        TopWorkdayList += '<td>' + item.employeeId + '</td>';
                        TopWorkdayList += '<td>' + item.firstName + ' ' + item.lastName + '</td>';
                        TopWorkdayList += '<td>' + item.workday + '</td>';
                        TopWorkdayList += '<td>' + item.payTypeCode + '</td>';
                        TopWorkdayList += '<td>' + item.jobTitleCode + '</td>';
                        TopWorkdayList += '<td>' + item.tHours + '</td>';
                        TopWorkdayList += '<tr>';
                        lblTotalWorkingHours += parseFloat(item.tHours);
                    })
                    $('#TopWorkdayListTabTableBody').html('');
                    $('#TopWorkdayListTabTableBody').html(TopWorkdayList);
                    $('#lblTotalWorkingHours').html('');
                    $('#lblTotalWorkingHours').html(lblTotalWorkingHours);
                }

                $('#ValidationErrorModalData').modal('show');
                draggableModel('#ValidationErrorModalData');

            } else {
                SuccessMsg("Success", "No record found", "success");
            }
        },
        error: function (err) {
            hideLoader();
            ErrorMsg("Failed", "Failed to fetch the data");
        }
    });
}

function UpdateErrorRowData() {
    var _array = [];
    $(".keyValue").each(function (index, element) {
        let subArr = {
            Key: $(this).find("#_key").text(),
            Value: $(this).find("#_value").val()
        };
        _array.push(subArr);
    });
    var ErrorDataReuest = {
        Id: $('#_errorId').val(),
        FileDetailId: $('#_fileDetailId').val(),
        FacilityId: $('#_facilityId').val(),
        ReportQuarter: $('#_reportQuarter').val(),
        Year: $('#_year').val(),
        Month: $('#_month').val(),
        UploadType: $('#_uploadType').val(),
        keyValueList: _array
    }
    showLoader();
    $.ajax({
        url: urls.Backout.UpdateErrorRowData,
        type: 'POST',
        data: ErrorDataReuest,
        success: function (data) {
            if (data.success) {
                validationErrorsList();
                $('#ValidationErrorModalData').modal('hide');
                hideLoader();
                SuccessMsg("Success", data.errorMessage, "success");
            } else {
                hideLoader();
                ErrorMsg("Failed", data.errorMessage);
            }
        },
        error: function (err) {
            hideLoader();
            ErrorMsg("Failed", "Failed to Delete");
        }
    });
}

function calculateTotalPages(totalCount, itemsPerPage) {
    return Math.ceil(totalCount / itemsPerPage);
}

function restrictInput(event) {
    let pattern = /^[A-Za-z\s\-'.]+$/;
    let key = event.key;
    if (pattern.test(key) || key === 'Backspace') {
        return;
    }
    event.preventDefault();
}

function ViewValidatedList() {
    let data = new FormData();
    data.append("FacilityId", $('#FacilityId').val());
    data.append("ReportQuarter", $('#QuarterList').val());
    data.append("Year", $('#YearList').val());

    $.ajax({
        url: urls.Review.GetStatusValidatedList,
        type: 'POST',
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            let modalContent = '';
            let successItems = result.filter(function (item) {
                return item.success === true;
            });

            if (successItems.length > 0) {
                successItems.forEach(function (item) {
                    modalContent += '<p>Validated By: ' + item.fullName + ', Validated Date & Time: ' + item.validationDate + '</p>';
                });
                $('#ViewReportVerificationModal .modal-body').html(populateHistoryTable(result));
                $('#myModalLabel').text('Validation History');
                $('#ViewReportVerificationModal').modal('show');
                draggableModel('#ViewReportVerificationModal');
            } else {
                ErrorMsg("Failed", "No validation history available for current facility's current report quarter.");
            }
        },
        error: function (result) {
            ErrorMsg("Failed", "Failed to Load Report Validation Status");
        }
    });
}

function populateHistoryTable(data) {
    let validationDate = data[2];
    if (validationDate.validationDate != null) {
        var columns = [
            { data: 'fullName', title: 'Validated By' },
            { data: 'validationDate', title: 'Validated Date & Time' }
        ];
    }
    else {
        var columns = [
            { data: 'fullName', title: 'Approved By' },
            { data: 'approvedDate', title: 'Approved Date & Time' }
        ];
    }

    $('#historyDataTable').dataTable().fnDestroy();
    $('#historyDataTable').DataTable({
        pagingType: "full_numbers",
        language: {
            paginate: {
                first: '<i class="fa fa-angle-double-left"></i>',
                previous: '<i class="fa fa-angle-left"></i>',
                next: '<i class="fa fa-angle-right"></i>',
                last: '<i class="fa fa-angle-double-right"></i>',
            }
        },
        "paging": true,
        "pageLength": 10,
        "columns": columns,
        "data": data // Provide your data array here
    });
}

function ViewApprovedList() {
    let data = new FormData();
    data.append("FacilityId", $('#FacilityId').val());
    data.append("ReportQuarter", $('#QuarterList').val());
    data.append("Year", $('#YearList').val());

    $.ajax({
        url: urls.Review.GetStatusApprovedList,
        type: 'POST',
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            let modalContent = '';
            let successItems = result.filter(function (item) {
                return item.success === true;
            });

            if (successItems.length > 0) {
                successItems.forEach(function (item) {
                    modalContent += '<p>Approved By: ' + item.fullName + ', Approved Date & Time: ' + item.approvedDate + '</p>';
                });
                $('#ViewReportVerificationModal .modal-body').html(populateHistoryTable(result));
                $('#myModalLabel').text('Approved History');
                $('#ViewReportVerificationModal').modal('show');
                draggableModel('#ViewReportVerificationModal');
            } else {
                ErrorMsg("Failed", "No approved history available for current facility's current report quarter.");
            }
        },
        error: function (result) {
            ErrorMsg("Failed", "Failed to Load Report Approved Status");
        }
    });
}

function deleteEmployeeWorkingHours() {
    ConfirmMsg('Delete working hours', 'Are you sure want to delete the data?', 'Delete', event, function () {
        showLoader();
        let employeeTimesheetDataRequest = {
            TimesheetId: $('#EmpTimesheetId').val()
        }
        $.ajax({
            url: urls.Review.DeleteTimesheetData,
            type: 'POST',
            data: employeeTimesheetDataRequest,
            success: function (result) {
                hideLoader();
                SuccessMsg("Success", "Deleted successfully", "success");
                $("#EmpAddUpdateWorkingHours").modal('hide');
                openEmployeeCalender($('#EmployeeIdWorking').val());
                loadStaffDeptData();
            },
            error: function (result) {
                hideLoader();
                ErrorMsg("Failed", "Failed to delete");
            }
        });


    });

}

function addMultipleDaysArray(startDate, endDate) {
    // Use a for loop to iterate through the dates and add them to the array
    for (var currentDate = startDate; currentDate <= endDate; currentDate.setDate(currentDate.getDate() + 1)) {
        // Clone the date object to avoid modifying the same object in the array
        var dateToAdd = moment(new Date(currentDate)).format('YYYY-MM-DD');
        AddSelectedDates.push(dateToAdd);

        if (!addSelectedColorDates.find(date => date.start == dateToAdd)) {
            addSelectedColorDates.push({
                start: dateToAdd,
                backgroundColor: 'yellow',
            });
        }

    }
}
function clearSessionVariables() {
    sessionStorage.removeItem('empIds');
}


