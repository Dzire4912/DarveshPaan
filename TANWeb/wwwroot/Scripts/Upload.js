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

var MappingDataStatus = {
    SUCCESS_CODE: 200,
    MULTIPLE_CMS_CODE: 201,
    FILE_UPLOAD_FAILED_CODE: 202,
    SERVER_FAILED_CODE: 400,
    MAP_PROPERLY_FAILED_CODE: 203,
    CMS_VALIDATION_FAILED_CODE: 204,
    INCOMPATIBLE_QUARTER_CODE: 205,
    INVALID_RECORD_CODE: 206
};

$('document').ready(function () {
    getYear();
    getQuaterList();
    getMonthList();
    getAgencies();

    // Get a reference to the checkbox element by its ID
    let checkbox = document.getElementById("onedrive");

    // Add a click event listener to the checkbox
    checkbox.addEventListener("click", function () {
        // Check if the checkbox is checked
        if (checkbox.checked) {
            processOneDrive();
        }
    });
    $('#DifferentQurterDataTable').dataTable().fnDestroy();
    $('#DifferentQurterDataTable').DataTable({
        pagingType: "full_numbers",
        language: {
            paginate: {
                first: '<i class="fa fa-angle-double-left"></i>',
                previous: '<i class="fa fa-angle-left"></i>',
                next: '<i class="fa fa-angle-right"></i>',
                last: '<i class="fa fa-angle-double-right"></i>',
            }
        },
    });
})

function getMonthList() {
    $('#MonthList').html();
    var mnthList = "";
    var qrtId = $('#QuarterList').val();

    $.each(ListMonths[qrtId - 1], function (i, item) {
        mnthList += "<option value='" + i + "'>" + item + "</option>";
    })
    $('#MonthList').html(mnthList);
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

$('#fileUpload').change(function (e) {
    if ($('#FacilityId').val() == "") {
        alert('Please select Facility');
        $('#fileUpload').val('');
        return false;
    }
    if ($('#YearList').val() == "0") {
        alert('Please select Year');
        $('#fileUpload').val('');
        return false;
    }
    if ($('#QuarterList').val() == "-1") {
        alert('Please select Quarter');
        $('#fileUpload').val('');
        return false;
    }
    if ($('#UploadType').val() == "0") {
        $('#fileUpload').val(null);
        alert('Please select Upload Type');

        return false;
    }
    if ($('#fileUpload').val().trim() != '') {
        fileUpload(e);
        $('#fileUpload').val('');
    }
});

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
                alert('error ' + JSON.stringify(result));
            }
        });
    }
}

function fileUpload(e) {
    if ($('#FacilityId').val() == "" || $('#FacilityId').val() == undefined) {
        clearFileInputField()
        WarningMsg("Warning!", 'Please select Facility', "Okay");
        $('#fileUpload').val('');
        return false;
    }
    if ($('#YearList').val() == "0") {
        clearFileInputField()
        WarningMsg("Warning!", 'Please select Year', "Okay");
        $('#fileUpload').val('');
        return false;
    }
    if ($('#QuarterList').val() == "-1") {
        clearFileInputField()
        WarningMsg("Warning!", 'Please select Quarter', "Okay");
        $('#fileUpload').val('');
        return false;
    }
    if ($('#UploadType').val() == "0") {
        clearFileInputField();
        WarningMsg("Warning!", 'Please select Upload Type', "Okay");
        $('#fileUpload').val(null);
        return false;
    }

    var file = e.target.files;
    if (file.length > 0) {
        var data = new FormData();
        data.append("FacilityId", $('#FacilityId').val());
        data.append("year", $('#YearList').val());
        data.append("Quarter", $('#QuarterList').val());
        data.append("Month", $('#MonthList').val());
        data.append("UploadType", $('#UploadType').val());
        data.append("AgencyId", $('#AgenciesId').val());
        data.append("file", file[0]);
        showLoader();
        $.ajax({
            url: urls.Upload.UploadFile,
            type: 'POST',
            data: data,
            contentType: false,
            processData: false,
            success: function (result) {
                hideLoader();
                $('#filterDiv').css("pointer-events", "none");
                $('#MapData').html('');
                $('#MapData').html(result);
            },
            error: function (result) {
                hideLoader();
                ErrorMsg("Failed", result.responseJSON.message);
                clearFileInputField();
            }
        });
    }
}

function clearFileInputField() {
    let deleteElement = $(".input-file__delete");
    let inputFile = deleteElement.parents(".input-file").find(".input-file__input");
    let placeholderText = inputFile[0].placeholder || "Drop files here";
    inputFile.val(null);
    deleteElement.css("display", "none")
        .prev(".input-file__msg")
        .text(placeholderText);
}

function mapFunction() {
    var MappingData = [];
    $("#MapTable tbody tr").each(function () {
        var CmsId = $(this).find('.selectddl').val();
        var ColName = $(this).find('.excelcol').text();
        var CmsFieldName = $(this).find('.selectddl option:selected').text();
        var tempdata = {
            CmsMappingID: CmsId,
            ColumnName: ColName,
            CmsFieldName: CmsFieldName
        }
        MappingData.push(tempdata);
    });

    var RequestModel = {
        FacilityId: parseInt($('#FacilityId').val()),
        year: parseInt($('#YearList').val()),
        Quarter: parseInt($('#QuarterList').val()),
        Month: parseInt($('#MonthList').val()),
        UploadType: $('#UploadType').val(),
        AgencyId: $('#AgenciesId').val()
    }
    var MappingRequestData = {
        MappingDatas: MappingData,
        RememberMe: $('#RememberMe').val(),
        RequestModel: RequestModel
    }
    showLoader();
    $.ajax({
        url: urls.Upload.MappingData,
        type: 'POST',
        data: MappingRequestData,
        dataType: 'json',
        success: function (result) {
            hideLoader();
            if (result.statusCode == MappingDataStatus.SUCCESS_CODE) {
                SuccessMsg("Success", "The file has been successfully uploaded.", "success");
                clearFileInputField();
                $('#filterDiv').css("pointer-events", "");
                $('#MapData').html('');
            } else if (result.statusCode == MappingDataStatus.INCOMPATIBLE_QUARTER_CODE) {
                $('#MapData').html('');
                $('#filterDiv').css("pointer-events", "");
                ConfirmMsg('Continue uploading prior quarter record(s)', result.message, 'Confirm', event, function () {
                    $('#filterDiv').css("pointer-events", "none");
                    $('#MapData').html('');
                    $('#DifferentQurterDataDiv').removeClass('hide');
                    populateTable(result.diffQuarterTimesheet);
                });
            } else if (result.statusCode == MappingDataStatus.INVALID_RECORD_CODE) {
                ConfirmMsg('Invalid Record(s)', result.message, 'Confirm', event, function () {
                    window.location.href = result.url;
                });
            }
            else {
                WarningMsg("Warning", result.message);
            }
        },
        error: function (result) {
            hideLoader();
            ErrorMsg('Error', 'Failed to upload', '');
        }
    });
}

function rememberMyMapping() {
    if ($('#RememberMe').is(':checked')) {
        $('#RememberMe').val(1);
    } else {
        $('#RememberMe').val(0);
    }
}

function processOneDrive() {
    let odOptions = {
        clientId: "3c39b444-9cde-4ddc-9878-a9c8b69197d2",
        action: "download",
        multiSelect: false,
        openInNewWindow: true,
        success: function (files) {
            /* success handler */
            $.each(files.value, function (index, element) {
                oneDriveUpload(element["@microsoft.graph.downloadUrl"], element.name);
            });
        },
        cancel: function () {
            ErrorMsg("warning", "You cancelled your upload.", "warning");
            let checkbox = document.getElementById("onedrive");
            checkbox.checked = false;
        },
        error: function (e) {
            ErrorMsg("Error", "Something went wrong.", "error");
        }
    }
    OneDrive.open(odOptions);
}

function oneDriveUpload(fileURL, fileName) {
    if ($('#FacilityId').val() == "" || $('#FacilityId').val() == undefined) {
        WarningMsg("Warning!", 'Please select Facility', "Okay");
        $('#fileUpload').val('');
        return false;
    }
    if ($('#YearList').val() == "0") {
        WarningMsg("Warning!", 'Please select Year', "Okay");
        $('#fileUpload').val('');
        return false;
    }
    if ($('#QuarterList').val() == "-1") {
        WarningMsg("Warning!", 'Please select Quarter', "Okay");
        $('#fileUpload').val('');
        return false;
    }
    if ($('#UploadType').val() == "0") {
        WarningMsg("Warning!", 'Please select Upload Type', "Okay");
        $('#fileUpload').val('');
        return false;
    }

    let data = new FormData();
    data.append("FacilityId", $('#FacilityId').val());
    data.append("year", $('#YearList').val());
    data.append("Quarter", $('#QuarterList').val());
    data.append("Month", $('#MonthList').val());
    data.append("UploadType", $('#UploadType').val());
    data.append("AgencyId", $('#AgenciesId').val());
    data.append("fileURL", fileURL);
    data.append("fileName", fileName);
    data.append("IsOneDrive", true);
    showLoader();
    $.ajax({
        url: urls.Upload.UploadFile,
        type: 'POST',
        data: data,
        contentType: false,
        processData: false,
        success: function (result) {
            hideLoader();
            $('#filterDiv').css("pointer-events", "none");
            $('#MapData').html('');
            $('#MapData').html(result);
        },
        error: function (result) {
            hideLoader();
            ErrorMsg("Failed", result.responseJSON.message);
        }
    });
}

function populateTable(data) {
    var columns = [
        { data: 'employeeId', title: 'Employee ID' },
        { data: 'firstName', title: 'First Name' },
        { data: 'lastName', title: 'Last Name' },
        { data: 'workday', title: 'Workday' },
        { data: 'tHours', title: 'Total Hours' }
    ];

    $('#DifferentQurterDataTable').dataTable().fnDestroy();
    $('#DifferentQurterDataTable').DataTable({
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

function uploadDifferentQuarterData() {
    showLoader();
    $.ajax({
        url: urls.Upload.InsertDifferntQuarterData,
        type: 'GET',
        success: function (result) {
            hideLoader();
            if (result.statusCode == MappingDataStatus.SUCCESS_CODE) {
                clearFileInputField();
                SuccessMsg("Success", result.message, "success");
                $('#filterDiv').css("pointer-events", "");
                $('#DifferentQurterDataTable tbody').html('');
                $('#DifferentQurterDataDiv').addClass('hide');
            } else if (result.statusCode == MappingDataStatus.INVALID_RECORD_CODE) {
                ConfirmMsg('Invalid Record(s)', result.message, 'Confirm', event, function () {
                    $('#DifferentQurterDataTable tbody').html('');
                    $('#DifferentQurterDataDiv').addClass('hide');
                    window.location.href = result.url;
                });
            } else {
                WarningMsg("Warning", result.message);
            }
        },
        error: function (result) {
            hideLoader();
            ErrorMsg('Error', 'Failed to upload', '');
        }
    });
}

