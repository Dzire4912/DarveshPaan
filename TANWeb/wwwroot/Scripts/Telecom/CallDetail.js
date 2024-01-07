$(document).ready(function () {
    loadCallDetailsData();
});


function loadCallDetailsData() {
    let organisationName = $('#organizationFilter').val();
    let callDirection = $('#callDirectionFilter').val();
    let callType = $('#callTypeFilter').val();
    let callResult = $('#callResultFilter').val();
    let hangUpSource = $('#hangUpSourceFilter').val();
    let BandWidthCallFilterViewModel = {
        OrganisationName: organisationName || '',
        CallDirection: callDirection || '',
        CallType: callType || '',
        CallResult: callResult || '',
        HangUpSource: hangUpSource || '',
    };
    let table = $('#BandwidthCallDetailsTable').DataTable();
    table.destroy();
    $('#BandwidthCallDetailsTable').DataTable(
        {
            ajax: {
                url: urls.TelecomReports.GetCallSummeryReportList,
                type: "POST",
                data: function (data) {
                    // You can pass additional data to the server if needed
                    data.searchValue = $('#BandwidthCallDetailsTable_filter input').val();
                    data.start = data.start || 0; // start parameter
                    data.length = data.length || 10; // length parameter
                    data.draw = data.draw || 1; // draw parameter
                    data.sortColumn = data.columns[data.order[0].column].data; // sort column
                    data.sortDirection = data.order[0].dir;
                    data.bandWidthCallFilter = BandWidthCallFilterViewModel;
                },
                error: function (xhr, status, error) {
                    if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                        ErrorMsg("Failed", "Unauthorized Access", "error");
                    }
                    else {
                        ErrorMsg("Failed", error, "error")
                    }
                }
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
                { data: "organizationName", name: "Sub Account Id" },
                { data: "startTime", name: "Start Time" },
                { data: "endTime", name: "End Time" },
                { data: "duration", name: "Duration" },
                { data: "callingNumber", name: "Calling Number" },
                { data: "calledNumber", name: "Called Number" },
                { data: "callDirection", name: "Call Direction" },
                { data: "callType", name: "Call Type" },
                { data: "callResult", name: "Call Result" },
                {
                    "data": null, orderable: false,
                    "render": function (data, type, full, row) {
                        return "<a href='#' onclick = showDetails('" + full.callId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='View Facilities' style='color: black' ><i class='ri-eye-line fs-6 pe-2 text-success'></i></a>"
                    }
                }
            ]
        }
    );
}

function showDetails(callId) {
    showLoader();
    $.ajax({
        url: urls.TelecomReports.GetCallDetailsById,
        type: 'Post',
        data: { callId: callId },
        success: function (result) {
            hideLoader();
            $('#GetCallDetails').modal('show');
            $('#callId').text(result.callId);
            $('#organizationName').text(result.subAccountId);
            $('#startTime').text(result.startTime);
            $('#endTime').text(result.endTime);
            $('#duration').text(result.duration);
            $('#accountId').text(result.accountId);
            $('#callingNumber').text(result.callingNumber);
            $('#calledNumber').text(result.calledNumber);
            $('#callDirection').text(result.callDirection);
            $('#callType').text(result.callType)
            $('#callResult').text(result.callResult);
            $('#sipResponseCode').text(result.sipResponseCode);
            $('#sipResponseDescription').text(result.sipResponseDescription);
            $('#cost').text("$ " + result.cost);
            $('#attestationIndication').text(result.attestationIndication);
            $('#hangUpSource').text(result.hangUpSource);
            $('#postDialDelay').text(result.postDialDelay);
            $('#pocketsSent').text(result.pocketsSent);
            $('#pocketsReceived').text(result.pocketsReceived);
            $('#isActive').text(result.isActive);
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

function clearFilter() {
    $('#organizationFilter').val(null).trigger('change');
    $('#callDirectionFilter').val(null).trigger('change');
    $('#callTypeFilter').val(null).trigger('change');
    $('#callResultFilter').val(null).trigger('change');
    $('#hangUpSourceFilter').val(null).trigger('change');
}


function DownloadCallDetailsCSV() {
    let date = new Date();
    let fileName = '';
    let organizationName= $('#organizationFilter option:selected').text().split('-')[0].trim();
    let callDirection = $('#callDirectionFilter').val();
    let callType = $('#callTypeFilter').val();
    let callResult = $('#callResultFilter').val();
    let hangUpSource = $('#hangUpSourceFilter').val();
    let searchValue = $('#BandwidthCallDetailsTable_filter input').val();
    if (organizationName == '' || organizationName == null || organizationName == 'Select Organization Name') {
        fileName = "All_Organizations_Call_Details_Report_";
        organizationName = null;
    }
    else {
        fileName = organizationName + "_Call_Details_Report_";
    }
    if (callDirection == '' || callDirection == null) {
        callDirection = null;
    }
    if (callType == '' || callType == null) {
        callType = null;
    }
    if (callResult == '' || callResult == null) {
        callResult = null;
    }
    if (hangUpSource == '' || hangUpSource == null) {
        hangUpSource = null;
    }
    let ExportCallHistoryRequest = {
        OrganizationName: organizationName,
        CallDirection: callDirection,
        CallType: callType,
        CallResult: callResult,
        HangUpSource: hangUpSource,
        SearchValue: searchValue,
        fileName: fileName
    }

    showLoader();
    $.ajax({
        url: urls.TelecomReports.DownloadCallDetails,
        type: 'GET',
        data: ExportCallHistoryRequest,
        xhrFields: {
            responseType: 'blob'
        },
        error: function (xhr, status, error) {
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access", "error");
            }
            else {
                ErrorMsg("Failed", error, "error")
            }
        },
        success: function (data) {
            hideLoader();
            data.arrayBuffer().then(buffer => {
                if (buffer.byteLength > 0) {
                    let url = window.URL.createObjectURL(data);
                    let link = document.createElement('a');
                    link.href = url;
                    link.download = fileName + '.csv';
                    link.click();

                    window.URL.revokeObjectURL(url);
                }
                else {
                    WarningMsg('Warning', 'No Data Found To Export', '');
                }
            });
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