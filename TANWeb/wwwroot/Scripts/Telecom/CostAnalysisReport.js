function loadCostAnalysisReportData() {
    let callDirection = $('#CallDirectionFilter option:selected').text();
    if (callDirection == 'Select Call Direction')
        callDirection = null;
    let callType = $('#CallTypeFilter option:selected').text();
    if (callType == 'Select Call Type')
        callType = null;
    let callResult = $('#CallResultFilter option:selected').text();
    if (callResult == 'Select Call Result Type')
        callResult = null;
    let orgName = $('#OrganizationNameFilter option:selected').text();
    if (orgName == 'Select Organization Name')
        orgName = null;
    let formData = {
        callDirection: callDirection,
        callType: callType,
        callResult: callResult,
        orgName: orgName
    };
    $('#costSummaryTable').DataTable(
        {
            ajax: {
                url: urls.TelecomReports.GetCostAnalysisReportList,
                type: "POST",
                data: function (data) {
                    // You can pass additional data to the server if needed
                    data.searchValue = $('#costSummaryTable_filter input').val();
                    data.start = data.start !== undefined ? data.start : 0; // start parameter
                    data.length = data.length || 10; // length parameter
                    data.draw = data.draw || 1; // draw parameter
                    data.sortColumn = data.columns[data.order[0].column].data; // sort column
                    data.sortDirection = data.order[0].dir;
                    data.callDirection = formData.callDirection;
                    data.callType = formData.callType;
                    data.callResult = formData.callResult;
                    data.orgName = formData.orgName;
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
                { data: "startTime", name: "Start Time" },
                { data: "endTime", name: "End Time" },
                {
                    data: "duration", name: "Duration", render: function (data, type, row) {
                        return data !== null ? data : "NA";
                    }
                },
                { data: "callingNumber", name: "Calling Number" },
                { data: "calledNumber", name: "Called Number" },
                { data: "callDirection", name: "Call Direction" },
                { data: "callType", name: "Call Type" },
                {
                    data: "cost", name: "Cost", render: function (data, type, row) {
                        return data !== null ? data : "NA";
                    }
                },
                { data: "callResult", name: "Call Result" }
            ]
        }
    );
}

function clearFilter() {
    $('#CallDirectionFilter').val(null).trigger('change');
    $('#CallTypeFilter').val(null).trigger('change');
    $('#CallResultFilter').val(null).trigger('change');
    $('#OrganizationNameFilter').val(null).trigger('change');
}

function initSelectTwoForCallAnalysisReport() {
    $('#CallDirectionFilter').select2({
        placeholder: "Select Call Direction",
        width: '100%'
    });
    $('#CallTypeFilter').select2({
        placeholder: "Select Call Type",
        width: '100%'
    });
    $('#CallResultFilter').select2({
        placeholder: "Select Call Result Type",
        width: '100%'
    });

    $('#OrganizationNameFilter').select2({
        placeholder: "Select Organization Name",
        width: '100%'
    });
}

function downloadCostAnalysisReportCSV() {
    let date = new Date();
    let fileName = '';
    let organizationName = $('#OrganizationNameFilter :selected').text();
    if (organizationName == '' || organizationName == 'Select Organization Name') {
        fileName = "All_Organizations_Cost_Analysis_Report_" + moment(date).format("YYYYMMDDHHMMSS");
        organizationName = null;
    }
    else {
        fileName = organizationName + "_Cost_Analysis_Report_" + moment(date).format("YYYYMMDDHHMMSS");
    }
    let callDirection = $('#CallDirectionFilter :selected').text();
    if (callDirection == '' || callDirection == 'Select Call Direction') {
        callDirection = null;
    }
    let callType = $('#CallTypeFilter :selected').text();
    if (callType == '' || callType == 'Select Call Type') {
        callType = null;
    }
    let callResult = $('#CallResultFilter :selected').text();
    if (callResult == '' || callResult == 'Select Call Result Type') {
        callResult = null;
    }
    let searchedValue = $('#costSummaryTable_filter input').val();
    let CostSummaryCsvRequest = {
        OrganizationName: organizationName,
        FileName: fileName,
        CallDirection: callDirection,
        CallType: callType,
        CallResult: callResult,
        SearchedValue:searchedValue
    }
    showLoader();
    $.ajax({
        url: urls.TelecomReports.ExportCostSummaryToCsv,
        type: 'GET',
        data: CostSummaryCsvRequest,
        xhrFields: {
            responseType: 'blob'
        },
        error: function (xhr, status, error) {
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access", "error")
            }
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
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access", "error");
            }
            else {
                ErrorMsg("Failed", error, "error")
            }
        }
    });
}