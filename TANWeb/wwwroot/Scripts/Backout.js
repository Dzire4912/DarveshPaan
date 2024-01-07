$('document').ready(function () {
    getYear();
    getQuaterList();
    loadBackoutData();
    $('#ErrorListModalId').modal({
        backdrop: 'static',
        keyboard: false
    });
})

function getYear() {
    let start_date = 2020;
    let dateobj = new Date();
    let end_date = dateobj.getFullYear();
    $('#YearList').html();
    let yrList = "";
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

let ListQuarters = [
    { key: '1', value: '1st Quarter (October 1 - December 31)' },
    { key: '2', value: '2nd Quarter (January 1 - March 31)' },
    { key: '3', value: '3rd Quarter (April 1 - June 30)' },
    { key: '4', value: '4th Quarter (July 1 - September 30)' }
];

function getQuaterList() {
    $('#QuarterList').html();
    let dateobj = new Date();
    let getMonth = dateobj.getMonth();
    let _key = findQuarter(getMonth + 1);
    let qtrList = "<option value='-1'>Select</option>";
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
    let value = 0;
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

function loadBackoutData() {
    let facilityId = $('#FacilityId').val();
    let BackoutTableRequest = {
        FacilityID: facilityId,
        Year: $('#YearList').val(),
        ReportQuarter: $('#QuarterList').val(),
        Status: $('#StatusListId').val()
    };

    let table = $('#BackoutListTable').DataTable();
    table.destroy();
    $('#BackoutListTable').DataTable(
        {
            ajax: {
                url: urls.Backout.GetUploadFileDetails,
                type: "POST",
                data: BackoutTableRequest,
                error: function (xhr, status, error) {
                    if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                        ErrorMsg("Failed", "Unauthorized Access");
                    }
                },
            },
            processing: true,
            serverSide: true,
            filter: true,
            scrollY: "300px",
            scrollCollapse: true,
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
                { data: "fileName", name: "File Name" },
                { data: "validRecord", name: "File Name" },
                {
                    data: null, orderable: false,
                    "render": function (data, type, full, row) {
                        if (full.invalidRecord != 0) {
                            return "<a href='#' onclick=deleteInvalidRecord('" + full.fileDetailsId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'>" + full.invalidRecord + "</a>";
                        } else {
                            return full.invalidRecord;
                        }
                    }
                },
                { data: "rowCount", name: "Total Count" },
                { data: "createDate", name: "Create Date" },
                {
                    data: null, orderable: false,
                    "render": function (data, type, full, row) {
                        return "<a href='#' onclick=deleteFile('" + full.fileDetailsId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>" +
                            "<a href='#' onclick=openErrorDetailListModal('" + full.fileDetailsId + "','" + full.facilityID + "','" + full.year + "','" + full.reportQuarter + "')  data-bs-toggle='tooltip'  data-bs-placement='top' title='View Valid / Invalid Records'><i class='ri-eye-line fs-6 text-success ms-2'></i></a>";
                    }
                }
            ]
        }
    );
}

function deleteFile(fileId) {
    ConfirmMsg('Backout', 'Are you sure want to Delete all the data?', 'Delete', event, function () {
        let BackOutAllRecordRequest = {
            FacilityId: $('#FacilityId').val(),
            FileDetailsId: fileId
        }
        showLoader();
        $.ajax({
            url: urls.Backout.BackOutAllData,
            type: 'POST',
            data: BackOutAllRecordRequest,
            success: function (data) {
                if (data.status == true) {
                    SuccessMsg("Success", data.message, "success");
                    loadBackoutData();
                    hideLoader();
                } else {
                    hideLoader();
                    ErrorMsg("Failed", data.message);
                }
            },
            error: function (xhr, status, error) {
                hideLoader();
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Failed", "Unauthorized Access");
                } else {
                    ErrorMsg("Failed", "Failed to Delete");
                }
            }
        });

    })
}

function getUploadFileDetails() {
    loadBackoutData();
}

function openErrorDetailListModal(fileDetailsId, facilityId, year, reportQuarter) {
    let BackOutAllRecordRequest = {
        FacilityId: facilityId,
        FileDetailsId: fileDetailsId
    }
    sessionStorage.setItem('FacilityId', BackOutAllRecordRequest.FacilityId);
    sessionStorage.setItem('FileDetailsId', BackOutAllRecordRequest.FileDetailsId);
    sessionStorage.setItem('Year', year);
    sessionStorage.setItem('ReportQuarter', reportQuarter);
    let facilityName = getFacilityName(facilityId);
    $('#facilityLbl').text(facilityName);
    $('#yearLbl').text(year);
    $('#quarterLbl').text(ListQuarters[reportQuarter - 1].value);
    let fileName = getFileName(BackOutAllRecordRequest.FacilityId, year, reportQuarter, fileDetailsId);
    $('#quarterLbl').text(ListQuarters[reportQuarter - 1].value);
    $('#fileNameLbl').text(fileName);
    let table = $('#BackoutErrorDetailListTable').DataTable();
    let flag = true;
    table.destroy();
    $('#BackoutErrorDetailListTable').DataTable(
        {
            ajax: {
                url: urls.Backout.GetFileErrorData,
                type: "POST",
                data: BackOutAllRecordRequest,
                error: function (xhr, status, error) {
                    if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                        flag = false;
                        ErrorMsg("Failed", "Unauthorized Access");
                    }
                },
            },
            processing: true,
            serverSide: true,
            filter: false,
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
                { data: "errorMessage", name: "ErrorMessage" },
                {
                    data: null, name: "Employee Id",
                    width: "10px",
                    "render": function (data, type, full, row) {
                        return "<span onclick='getErrorData(this," + full.id + ")'; style='cursor: pointer;' data-bs-toggle='offcanvas' data-bs-target='#offcanvasRight'>" + full.finalFileRowData.employeeId + "</span>";
                    }
                },
                {
                    data: null, name: "Pay Type Code",
                    "render": function (data, type, full, row) {
                        return "<span  onclick='getErrorData(this," + full.id + ")'; style='cursor: pointer;' data-bs-toggle='offcanvas' data-bs-target='#offcanvasRight'>" + full.finalFileRowData.payTypeCode + "</span>";
                    }
                },
                {
                    data: null, name: "Work Day",
                    "render": function (data, type, full, row) {
                        return "<span  onclick='getErrorData(this," + full.id + ")'; style='cursor: pointer;' data-bs-toggle='offcanvas' data-bs-target='#offcanvasRight'>" + full.finalFileRowData.workDay + "</span>";
                    }
                },
                {
                    data: null, name: "Hours",
                    "render": function (data, type, full, row) {
                        return "<span  onclick='getErrorData(this," + full.id + ")'; style='cursor: pointer;' data-bs-toggle='offcanvas' data-bs-target='#offcanvasRight'>" + full.finalFileRowData.hours + "</span>";
                    }
                },
                {
                    data: null, name: "Job Title Code",
                    "render": function (data, type, full, row) {
                        return "<span  onclick='getErrorData(this," + full.id + ")'; style='cursor: pointer;' data-bs-toggle='offcanvas' data-bs-target='#offcanvasRight'>" + full.finalFileRowData.jobTitleCode + "</span>";
                    }
                },
                {
                    data: null, name: "Employee First Name",
                    "render": function (data, type, full, row) {
                        //return "<span  onclick='getErrorData(this," + full.id + ")'; style='cursor: pointer;'> " + full.finalFileRowData.employeeFirstName + "</span>";
                        return "<span onclick='getErrorData(this, " + full.id + ")' style='cursor: pointer;' data-bs-toggle='offcanvas' data-bs-target='#offcanvasRight'>" + full.finalFileRowData.employeeFirstName + "</span>";
                    }
                }
            ]
        }
    );
    $('#ButtonDivHide').css('display', 'none');
    $('#RightErrorTab').html('');
    if (flag) {
        $("#ErrorListModalId").modal("show");
    }
    //loadInValidData();
    loadInValidDynamicData();
}

function getErrorData(clickedElement, Id) {
    $('#BackoutErrorDetailListTable tbody tr').removeClass('highlighted-row');
    let row = clickedElement.closest('tr');
    if (row) {
        row.classList.add('highlighted-row');
    }
    let formdata = {
        Id: Id
    }
    showLoader();
    $.ajax({
        url: urls.Backout.GetErrorRowData,
        type: 'POST',
        data: formdata,
        success: function (data) {
            hideLoader();
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
                    div += '<div class="col-sm-12 keyValue">';
                    div += '<span id="_key">' + value.key + '</span>';
                    if (value.key.toLowerCase().includes('workday')) {
                        const parts = value.value.split('/');
                        console.log(parts);
                        if (parts.length === 3) {
                            const yyyy = parts[2];
                            const mm = parts[0].padStart(2, '0');
                            const dd = parts[1].padStart(2, '0');
                            const formattedDate = yyyy + '-' + mm + '-' + dd;
                            div += "<span><input id='_value' class='form-control' type='date' value='" + formattedDate + "' /></span>";
                        }
                    }
                    else {
                        div += "<span><input id='_value' class='form-control' type='text' value='" + value.value + "' /></span>";
                    }
                    //div += "<span><input id='_value' class='form-control' type='text'' value='" + value.value + "' /></span>";
                    div += '</div>';
                });
                $('#RightErrorTab').html('');
                $('#RightErrorTab').html(div);
                $('#CanvasData').html('');
                $('#CanvasData').html(div);
                $('#ButtonDivHide').css('display', 'block');

            } else {
                $('#RightErrorTab').html('');
                $('#RightErrorTab').html('<span>No record found</span>');
                $('#CanvasData').html('');
                $('#CanvasData').html('<span>No record found</span>');
            }
        },
        error: function (err) {
            $('#ButtonDivHide').css('display', 'none');
            ErrorMsg("Failed", "Failed to Delete");
        }
    });
}

function updateErrorRowData() {
    let _array = [];
    $(".keyValue").each(function (index, element) {
        let subArr = {
            Key: $(this).find("#_key").text(),
            Value: $(this).find("#_value").val()
        };
        _array.push(subArr);
    });
    let ErrorDataReuest = {
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
                hideLoader();
                SuccessMsg("Success", data.errorMessage, "success");
                //let offcanvasElement = document.getElementById("offcanvasRight");
                //offcanvasElement.style.display = "none";
                let dismissButton = document.querySelector("[data-bs-dismiss='offcanvas']");
                dismissButton.click();
                loadInValidDynamicData(1);
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

function DeleteErrorRowData() {
    let facilityId = sessionStorage.getItem('FacilityId');
    ConfirmMsg('Delete', 'Are you sure want to Delete the record?', 'Delete', event, function () {
        if ($('#_errorId').val() == undefined || $('#_errorId').val() == '') {
            ErrorMsg(" Alert", "Please choose the record from the table and try again!");
            return false;
        }
        let formdata = {
            Id: $('#_errorId').val()
        }
        showLoader();
        $.ajax({
            url: urls.Backout.DeleteErrorRowData,
            type: 'POST',
            data: formdata,
            success: function (data) {
                hideLoader();
                $('#ButtonDivHide').css('display', 'none');
                if (data.statusCode == 200) {
                    openErrorDetailListModal($('#_fileDetailId').val(), facilityId, $('#yearLbl').text(), $('#QuarterList').val());
                    SuccessMsg("Success", data.message, "success");
                } else {
                    ErrorMsg("Failed", data.message);
                }

            },
            error: function (err) {
                hideLoader();
                $('#ButtonDivHide').css('display', 'none');
                ErrorMsg("Failed", "Failed to Delete");
            }
        });
    })
}

function deleteInvalidRecord(fileDetailId) {
    ConfirmMsg('Delete', 'Are you sure want to delete all invalid record?', 'Delete', event, function () {
        let formdata = {
            FileDetailId: fileDetailId
        }
        showLoader();
        $.ajax({
            url: urls.Backout.DeleteInvalidRecord,
            type: 'POST',
            data: formdata,
            success: function (data) {
                if (data.statusCode == 200) {
                    SuccessMsg("Success", data.message, "success");
                    loadBackoutData();
                    hideLoader();
                } else {
                    hideLoader();
                    ErrorMsg("Failed", data.message);
                }
            },
            error: function (xhr, status, error) {
                hideLoader();
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Failed", "Unauthorized Access");
                } else {
                    ErrorMsg("Failed", "Failed to Delete");
                }

            }
        });
    })
}

function loadValidData() {
    $('#tab1-tab').addClass('active');
    $('#tab3-tab').removeClass('active');
    $('#tab1').addClass('show active');
    $('#tab3').removeClass('show active');
    let facilityId = sessionStorage.getItem('FacilityId');
    let fileDetailsId = sessionStorage.getItem('FileDetailsId');
    let BackOutValidRecordRequest = {
        FacilityId: facilityId,
        FileDetailsId: fileDetailsId
    }
    let table = $('#BackoutErrorDetailListValidRecordTable').DataTable();
    let flag = true;
    table.destroy();
    $('#BackoutErrorDetailListValidRecordTable').DataTable(
        {
            ajax: {
                url: urls.Backout.GetFileValidData,
                type: "POST",
                data: BackOutValidRecordRequest,
                error: function (xhr, status, error) {
                    if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                        flag = false;
                        ErrorMsg("Failed", "Unauthorized Access");
                    }
                },
            },
            processing: true,
            serverSide: true,
            filter: false,
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
                { data: "employeeId", name: "EmployeeId" },
                { data: "firstName", name: "FirstName" },
                { data: "lastName", name: "LastName" },
                { data: "payTypeCode", name: "Pay Type Code" },
                { data: "workday", name: "Work Day" },
                { data: "tHours", name: "Hours" },
                { data: "jobTitleCode", name: "Job Title Code" },
            ]
        }
    );
}

function loadInValidData() {
    $('#tab1-tab').removeClass('active');
    $('#tab2-tab').addClass('active');
    $('#tab1').removeClass('show active');
    $('#tab2').addClass('show active');
}

function downloadInvalidFileData() {
    let facilityId = sessionStorage.getItem('FacilityId');
    let fileDetailsId = sessionStorage.getItem('FileDetailsId');
    let year = sessionStorage.getItem('Year');
    let reportQuarter = sessionStorage.getItem('ReportQuarter');
    let date = new Date();
    let fileName = "InvalidErrorFileDetails_" + facilityId + "_" + moment(date).format("YYYYMMDDHHMMSS");
    let formData = {
        facilityId: facilityId,
        year: year,
        reportQuarter: reportQuarter,
        fileName: fileName,
        fileDetailsId: fileDetailsId
    };
    showLoader();
    $.ajax({
        url: urls.Backout.ExportInvalidFileErrorDataToCsv,
        type: 'GET',
        data: formData,
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data) {
            hideLoader();
            let url = window.URL.createObjectURL(data);
            let link = document.createElement('a');
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

function downloadValidFileData() {
    let facilityId = sessionStorage.getItem('FacilityId');
    let fileDetailsId = sessionStorage.getItem('FileDetailsId');
    let date = new Date();
    let fileName = "ValidFileDetails_" + facilityId + "_" + moment(date).format("YYYYMMDDHHMMSS");
    let formData = {
        facilityId: facilityId,
        //year: year,
        //reportQuarter: reportQuarter,
        fileName: fileName,
        fileDetailsId: fileDetailsId
    };
    showLoader();
    $.ajax({
        url: urls.Backout.ExportValidFileDataToCsv,
        type: 'GET',
        data: formData,
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data) {
            hideLoader();
            let url = window.URL.createObjectURL(data);
            let link = document.createElement('a');
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

function getFileName(FacilityId, year, reportQuarter, fileDetailsId) {
    let fileName = "";
    let FileDetailListRequest = {
        FacilityId: FacilityId,
        ReportQuarter: reportQuarter,
        Year: year,
        FileDetailsId: fileDetailsId
    }
    showLoader();
    $.ajax({
        url: urls.Backout.GetFileNameByFacility,
        type: 'GET',
        data: FileDetailListRequest,
        async: false,
        success: function (result) {
            hideLoader();
            if (result.fileName != '')
                fileName = result.fileName;
            else {
                fileName = '';
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

    return fileName;
}

function clearSessionVariables() {
    sessionStorage.removeItem('FacilityId');
    sessionStorage.removeItem('FileDetailsId');
    sessionStorage.removeItem('Year');
    sessionStorage.removeItem('ReportQuarter');
}

function loadInValidDynamicData(_pageNo = 1) {
    $('#tab1-tab').removeClass('active');
    $('#tab1').removeClass('show active');
    $('#tab3-tab').addClass('active');
    $('#tab3').addClass('show active');

    let fileDetailsId = sessionStorage.getItem('FileDetailsId');

    if (fileDetailsId == '0') {
        return false;
    }
    let ValidationErrorListRequest = {
        FileDetailId: fileDetailsId,
        PageNo: _pageNo,
        PageSize: $('#PageSizeddl').val()
    }
    showLoader();
    $.ajax({
        url: urls.Backout.ListValidationErrorByFileId,
        type: 'POST',
        data: ValidationErrorListRequest,
        dataType: 'json',
        success: function (data) {
            if (data.dynamicDataTableModel.length > 0) {
                let columns = Object.keys(data.dynamicDataTableModel[0].dynamicDataTable);
                let table = '<table id="errorDynamicTable" class="table display table-bordered no-wrap">';
                table += '<thead><tr>';
                for (let i = 0; i < columns.length; i++) {
                    if (columns[i] != "ErrorId") {
                        if (columns[i] === "Error Message") {
                            // Set a specific width for the "Error Message" column
                            table += '<th style="width: 20px;">' + columns[i] + '</th>';
                        } else {
                            // For other columns, no specific width is set
                            table += '<th>' + columns[i] + '</th>';
                        }
                    }
                }
                table += '<th>Action</th></tr></thead><tbody>';

                for (let j = 0; j < data.dynamicDataTableModel.length; j++) {
                    table += '<tr>';
                    for (let k = 0; k < columns.length; k++) {
                        let columnName = columns[k];
                        if (columnName != "ErrorId") {
                            if (columnName === "Error Message") {
                                table += '<td style="width: 20px;">' + data.dynamicDataTableModel[j].dynamicDataTable[columnName] + '</td>';
                            }
                            else {
                                table += '<td>' + data.dynamicDataTableModel[j].dynamicDataTable[columnName] + '</td>';
                            }
                        }
                    }
                    table += "<td><a href='#' onclick=deleteValidationErrorRecord('" + data.dynamicDataTableModel[j].dynamicDataTable["ErrorId"] + "'); title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
                    table += " <a href='#' onclick=openErrorDetailListModalForDynamic(this,'" + data.dynamicDataTableModel[j].dynamicDataTable["ErrorId"] + "')  title='View' data-bs-toggle='offcanvas' data-bs-target='#offcanvasRight'><i class='ri-eye-line fs-6 text-success ms-2'></i></a>";
                    table += "</td></tr>";
                }
                table += '</tbody></table>';

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
                $('#rowCountId').html('Showing ' + data.rowStart + ' to ' + data.rowCount + ' of ' + data.totalCount + ' entries');
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

function openErrorDetailListModalForDynamic(clickedElement, ErrorId) {
    $('#errorDynamicTable tbody tr').removeClass('highlighted-row');
    let row = clickedElement.closest('tr');
    if (row) {
        row.classList.add('highlighted-row');
    }
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
                    div += '<div class="col-sm-12 keyValue mt-2">';
                    div += '<span id="_key">' + value.key + '</span>';
                    if (value.key.toLowerCase().includes('workday') || value.key.toLowerCase().includes('work day') || value.key.toLowerCase().includes('clockpunch_in1') || value.key.toLowerCase().includes('clockpunch_out1')) {
                        const parts = value.value.split('/');
                        if (parts.length === 3) {
                            const year = parts[2];
                            const dateObject = new Date(year);
                            const yyyy = dateObject.getFullYear();
                            const mm = parts[0].padStart(2, '0');
                            const dd = parts[1].padStart(2, '0');
                            const formattedDate = yyyy + '-' + mm + '-' + dd;
                            div += "<span><input id='_value' class='form-control' type='date' value='" + formattedDate + "' /></span>";
                        }
                    }
                    else {
                        div += "<span><input id='_value' class='form-control' type='text' value='" + value.value + "' /></span>";
                    }
                    div += '</div>';
                });
                $('#CanvasData').html('');
                $('#CanvasData').html(div);
                $('#ButtonDivHide').css('display', 'block');
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
function calculateTotalPages(totalCount, itemsPerPage) {
    return Math.ceil(totalCount / itemsPerPage);
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
                hideLoader();
                if (data.statusCode == 200) {
                    SuccessMsg("Success", data.message, "success");
                    loadInValidDynamicData(1);
                } else {
                    ErrorMsg("Failed", data.message);
                }
            },
            error: function (err) {
                hideLoader();
                ErrorMsg("Failed", "Failed to Delete");
            }
        });
    })
}

function downloadValidFileDynamicData() {
    let facilityId = sessionStorage.getItem('FacilityId');
    let fileDetailsId = sessionStorage.getItem('FileDetailsId');
    let year = sessionStorage.getItem('Year');
    let reportQuarter = sessionStorage.getItem('ReportQuarter');
    let date = new Date();
    let fileName = "InvalidErrorFileDetails_" + facilityId + "_" + moment(date).format("YYYYMMDDHHMMSS");
    let formData = {
        facilityId: facilityId,
        year: year,
        reportQuarter: reportQuarter,
        fileName: fileName,
        fileDetailsId: fileDetailsId
    };
    showLoader();
    $.ajax({
        url: urls.Backout.ExportInvalidFileErrorDynamicDataToCsv,
        type: 'GET',
        data: formData,
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data) {
            hideLoader();
            let url = window.URL.createObjectURL(data);
            let link = document.createElement('a');
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

function validationErrorsList(_pageNo = 1) {
    let fileDetailsId = sessionStorage.getItem('FileDetailsId');
    let ValidationErrorListRequest = {
        FileDetailId: fileDetailsId,
        PageNo: _pageNo,
        PageSize: $('#PageSizeddl').val()
    }
    showLoader();
    $.ajax({
        url: urls.Backout.ListValidationErrorByFileId,
        type: 'POST',
        data: ValidationErrorListRequest,
        dataType: 'json',
        success: function (data) {
            if (data.dynamicDataTableModel.length > 0) {
                let columns = Object.keys(data.dynamicDataTableModel[0].dynamicDataTable);
                let table = '<table id="errorDynamicTable" class="table display table-bordered no-wrap">';
                table += '<thead><tr>';
                for (let i = 0; i < columns.length; i++) {
                    if (columns[i] != "ErrorId") {
                        table += '<th>' + columns[i] + '</th>';
                    }
                }
                table += '<th>Action</th></tr></thead><tbody>';

                for (let j = 0; j < data.dynamicDataTableModel.length; j++) {
                    table += '<tr>';
                    for (let k = 0; k < columns.length; k++) {
                        let columnName = columns[k];
                        if (columnName != "ErrorId") {
                            table += '<td>' + data.dynamicDataTableModel[j].dynamicDataTable[columnName] + '</td>';
                        }
                    }
                    table += "<td><a href='#' onclick=deleteValidationErrorRecord('" + data.dynamicDataTableModel[j].dynamicDataTable["ErrorId"] + "'); title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
                    table += " <a href='#' onclick=openErrorDetailListModalForDynamic(this,'" + data.dynamicDataTableModel[j].dynamicDataTable["ErrorId"] + "')  title='View' data-bs-toggle='offcanvas' data-bs-target='#offcanvasRight'><i class='ri-eye-line fs-6 text-success ms-2'></i></a>";
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
                $('#rowCountId').html('Showing ' + data.rowStart + ' to ' + data.rowCount + ' of ' + data.totalCount + ' entries');
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

function getFacilityName(facilityId) {
    let facilityName = '';
    $.ajax({
        url: urls.Backout.GetFacilityName,
        type: 'GET',
        data: { facilityId: facilityId },
        async: false,
        success: function (result) {
            if (result != null)
            {
                facilityName = result;
            }
        },
        error: function (xhr, status, error) {
            ErrorMsg("Failed", error, "error")
        }
    });
    return facilityName;
}
