
const DataServiceStatusCode = {
    INVALID_FILE: 1001,
    FILE_EXIST: 1002,
    SUCCESS: 1003,
    FAILED: 1004,
    ACCOUNTNUMBER_EXIST: 1005,
    UPLOAD_FILE_ERROR: 1006
};
let DataServiceTypeArr = [];
$('document').ready(function () {
    getDataServiceList();
    initSelect2Review();
    getServiceTypeData();
})

function applyFilters() {
    getDataServiceList();
}

function getDataServiceList() {
    let table = $('#dataServiceTable').DataTable();
    table.destroy();
    let DataServiceListRequest = {
        facilityId: $('#facilityLocationId').val(),
        serviceId: $('#ServiceListId').val()
    };

    $('#dataServiceTable').DataTable(
        {
            ajax: {
                url: urls.TelecomDataService.GetDataServiceList,
                type: "POST",
                data: function (data) {
                    data.searchValue = $('#dataServiceTable_filter input').val();
                    data.start = data.start || 0;
                    data.length = data.length || 10;
                    data.draw = data.draw || 1;
                    data.sortColumn = data.columns[data.order[0].column].data;
                    data.sortDirection = data.order[0].dir;
                    data.serviceListRequest = DataServiceListRequest;
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
                { data: "facilityName", name: "Facility Name" },
                { data: "providerName", name: "Provider Name" },
                { data: "accountNumber", name: "Account Number" },
                { data: "serviceName", name: "Service Name" },
                { data: "contractStatus", name: "Status" },
                {
                    data: null, orderable: false,
                    "render": function (data, type, full, row) {
                        if (full.contractDocument == "NA" || full.contractDocument == "") {
                            return "<a href='#' onclick=editRecord('" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='edit'><i class='ri-edit-2-line text-info fs-6'></i> </a>" + "<a href='#' onclick=deleteRecord('" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line text-danger fs-6'></i></a>";
                        } else {
                            return "<a href='#' onclick=editRecord('" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='edit'><i class='ri-edit-2-line text-info fs-6'></i> </a>" + "<a href='#' onclick=deleteRecord('" + full.id + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line text-danger fs-6'></i></a>" + "<a href='#' onclick=downloadContractDocx('" + full.id + "','" + full.contractDocument + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Download Contract Document'><i class='ri-file-download-fill text-info fs-6'></i> </a>";
                        }
                    }
                }
            ]
        }
    );
}

function saveDataServiceFunction() {
    let token = $('input[name="__RequestVerificationToken"]', ('#DataServiceForm')).val();
    let flag = 0;
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

    let fileInput = $("input[type='file']")[0];
    let file = fileInput.files[0];
    if (file != undefined && file != '') {

        /* $('#ContractDocumentError').html('The ' + $('#ContractDocumentError').attr("title") + ' field is required.');
         flag = 1;
         return false;*/

        if (file.name.split('.')[1] != "pdf") {
            $('#ContractDocumentError').html('The file type is invalid.');
            flag = 1;
            return false;
        }
    }

    if (flag == 1) {
        return false;
    }
    let formdata = new FormData();
    formdata.append("Id", $('#dataSrviceId').val());
    formdata.append("FacilityId", $('#facilityLocationId').val());
    formdata.append("ProviderName", $('#ProviderName').val());
    formdata.append("AccountNumber", $('#AccountNumber').val());
    formdata.append("LocalIP", $('#LocalIP').val());
    formdata.append("ServicesId", $('#ServiceTypeId').val());
    formdata.append("SupportNumber", $('#SupportNumber').val());
    formdata.append("ContractEndDate", $('#ContractEndDate').val());
    formdata.append("ContractStatus", $('#ContractStatus').val());
    formdata.append("file", file);
    showLoader();
    $.ajax({
        url: urls.TelecomDataService.AddDataService,
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: formdata,
        contentType: false,
        processData: false,
        success: function (result) {
            hideLoader();
            if (result.statusCode == DataServiceStatusCode.SUCCESS) {
                $("#DataServiceModal").modal('hide');
                clearData();
                SuccessMsg("Success", result.message, "success");
                getDataServiceList();
            } else {
                ErrorMsg("Error", result.message, "success");
            }
        },
        error: function (result) {
            hideLoader();
            ErrorMsg("Alert!", "Failed");
        }
    });
}

function editRecord(Id) {
    let requestData = {
        Id: Id
    }
    $.ajax({
        url: urls.TelecomDataService.GetDataServiceById,
        type: 'POST',
        data: requestData,
        async: false,
        cache: false,
        success: function (result) {
            hideLoader();
            $('#ProviderName').val(result.providerName);
            $('#AccountNumber').val(result.accountNumber);
            $('#LocalIP').val(result.localIP);
            $('#SupportNumber').val(result.supportNumber);
            $('#dataSrviceId').val(result.id);
            let contractEndDate = (result.contractEndDate == "0001-01-01T00:00:00") ? result.contractEndDate : moment(result.contractEndDate).format("YYYY-MM-DD");
            $('#ContractEndDate').val(contractEndDate);
            $('#ServiceTypeId').val(result.servicesId).trigger('change');
            $('#LocationFacilityId').val(result.facilityId).trigger('change');
            $('#ContractStatus').val(result.contractStatus.toString()).trigger('change');
            $('#downloadContractDocxId').addClass("hide");
            if (result.contractDocument != "NA" || result.contractDocument != "") {
                $('#downloadContractDocxId').removeClass("hide");
            }

            $('#DataServiceModalLabel').html('Edit Data Service');
            $('#DataServiceModal').modal('show');
        },
        error: function (result) {
            hideLoader();
            ErrorMsg("Alert!", "Failed");
        }
    });
}

function getServiceTypeData() {

    $.ajax({
        url: urls.TelecomDataService.GetDataServiceTypeList,
        type: 'GET',
        async: false,
        cache: false,
        success: function (result) {
            hideLoader();
            DataServiceTypeArr = result;
            let serviceType = "";
            /*serviceType += "<option >select service</option>";*/
            $.each(result, function (i, item) {
                serviceType += "<option value='" + item.value + "'>" + item.text + "</option>";
            })
            $('#ServiceTypeId').html(serviceType);
        },
        error: function (result) {
            hideLoader();
            ErrorMsg("Alert!", "Failed");
        }
    });
}

function openDataServiceModal() {
    clearData();
    $('#dataSrviceId').val("0");
    $('#downloadContractDocxId').addClass("hide");
    $('#DataServiceModal').modal('show');
    $('#DataServiceModalLabel').html('Add Data Service');
}

function clearData() {
    $('#ProviderName').val('');
    $('#AccountNumber').val('');
    $('#LocalIP').val('');
    $('#SupportNumber').val('');
    $('#ContractEndDate').val();
    $('#ServiceTypeId').val(null).trigger('change');
    $('#ContractStatus').val("true").trigger('change');

}

function deleteRecord(Id) {

    ConfirmMsg('Delete', 'Are you sure want to delete the data?', 'Continue', event, function () {
        showLoader();
        let requestData = {
            Id: Id
        }
        $.ajax({
            url: urls.TelecomDataService.DeleteDataService,
            type: 'POST',
            data: requestData,
            async: false,
            Cache: false,
            success: function (result) {
                hideLoader();
                hideLoader();
                SuccessMsg("Success", "Record saved successfully", "success");
                getDataServiceList();
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


function initSelect2Review() {

    $('#LocationFacilityId').select2({
        placeholder: "Select Call Direction",
        width: '100%'
    });
}

function clearFilters() {
    location.reload();
}

function downloadContractDocx(dataServiceId, filename) {
    let DownloadDataServiceContractFile = {
        FacilityId: $('#facilityLocationId').val(),
        DataServiceId: dataServiceId
    }
    showLoader();
    $.ajax({
        url: urls.TelecomDataService.DownloadContractDocx,
        type: 'GET',
        data: DownloadDataServiceContractFile,
        contentType: 'application/json',
        xhrFields: {
            responseType: 'blob'
        },
        success: function (data) {
            hideLoader();
            let url = window.URL.createObjectURL(data);
            let link = document.createElement('a');
            link.href = url;
            link.download = filename;
            link.click();
            window.URL.revokeObjectURL(url);
        },
        error: function (result) {
            hideLoader();
            if (result.statusCode == 404) {
                ErrorMsg('Warning', "File Not Found", '');
            } else {
                ErrorMsg('Error', "Failed", '');
            }
        }
    });

}