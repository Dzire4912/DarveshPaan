$(document).ready(function () {
    loadDataServiceTypeList()
});
function validateDataServiceTypeForm() {
    return !($('#serviceName').val() == "" || $('.errormsg').text() != "");
}
function saveDataServiceType() {
    let serviceName = $('#serviceName').val();
    if (serviceName == null || serviceName == '' || serviceName == ' ') {
        $('#ServiceNameError').text('Data Service Type Name is required');
        return false;
    }
    else {
        let dataServiceTypeViewModel = {
            DataServiceTypeId: "0",
            DataServiceTypeName: $('#serviceName').val()
        };
        $.ajax({
            url: urls.DataServiceType.SaveDataServiceType,
            type: 'POST',
            data: dataServiceTypeViewModel || (dataServiceTypeViewModel = null),
            success: function (result) {
                hideLoader();
                if (result.statusCode == statuMessageCode.Success) {
                    SuccessMsg("Success", "Record Added successfully", "success");
                    $('#DataServiceModal').modal('hide');
                    clearDataServiceTypeData();
                }
                else if (result.statusCode == statuMessageCode.Exist) {
                    ErrorMsg("Failed", "Record Already Exist", "error");
                    $('#DataServiceModal').modal('hide');
                    clearDataServiceTypeData();
                }
                else if (result.statusCode == statuMessageCode.Updated) {
                    SuccessMsg("Success", "Record Updated successfully", "success");
                    $('#DataServiceModal').modal('hide');
                    clearDataServiceTypeData();
                }
                else if (result.statusCode == statuMessageCode.Failed) {
                    ErrorMsg("Failed", "Enter valid data", "error")
                    $('#DataServiceModal').modal('hide');
                    clearDataServiceTypeData();
                }
                else if (result.statusCode == statuMessageCode.Error) {
                    $('#ServiceNameError').text(result.message);
                    return false;
                }
                clearDataServiceTypeData();
                loadDataServiceTypeList();
                $('#ServiceNameError').text('');
            },
            error: function (xhr, status, error) {
                hideLoader();
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Failed", "Unauthorized Access");
                }
                else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                    ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                    $('#DataServiceModal').modal('hide');
                    clearDataServiceTypeData();
                    loadDataServiceTypeList();
                    $('#ServiceNameError').text('');
                }
                else {
                    ErrorMsg("Failed", error, "error");
                }
            }
        });
    }
}


function loadDataServiceTypeList() {
    clearDataServiceTypeData();
    let table = $('#dataServiceTypeTable').DataTable();
    table.destroy();
    $('#dataServiceTypeTable').DataTable(
        {
            ajax: {
                url: urls.DataServiceType.GetDataServiceTypeList,
                type: "POST",
                data: function (data) {
                    // You can pass additional data to the server if needed
                    data.searchValue = $('#dataServiceTypeTable_filter input').val();
                    data.start = data.start || 0; // start parameter
                    data.length = data.length || 10; // length parameter
                    data.draw = data.draw || 1; // draw parameter
                    data.sortColumn = data.columns[data.order[0].column].data; // sort column
                    data.sortDirection = data.order[0].dir;
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
                { data: "dataServiceTypeName", name: "Data Service Type Name" },
                {
                    data: null, orderable: false,
                    "render": function (data, type, full, row) {
                        return "<a href='#' onclick = editDataServiceType('" + full.dataServiceTypeId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='View Facilities' style='color: black' ><i class='ri-edit-2-line text-info fs-6'></i></a>"
                            + " " + "<a href='#' onclick=deleteDataServiceType('" + full.dataServiceTypeId + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";

                    }
                }
            ]
        }
    );
}


function clearDataServiceTypeData() {

    $('#serviceName').val('');
    $('#ServiceNameError').text('');
}

function editDataServiceType(dataServiceTypeId) {
    clearDataServiceTypeData();
    showLoader();
    $.ajax({
        url: urls.DataServiceType.GetDataServiceTypeById,
        type: 'GET',
        data: { dataServiceTypeId: dataServiceTypeId },
        success: function (serviceType) {
            hideLoader();
            $('#DataServiceModal').modal('show');
            draggableModel('#DataServiceModal');
            $('#dataServiceTypeId').val(serviceType.dataServiceTypeId);
            $('#serviceName').val(serviceType.dataServiceTypeName);
            $('#status').val(serviceType.IsActive);
            $('#FacilityModalLabel').text("Update Data Service Type");
            $('#btnSaveDataService').hide();
            $('#btnUpdateDataService').show();
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

function updateDataServiceType() {
    let dataServiceTypeViewModel = {
        DataServiceTypeId: $("#dataServiceTypeId").val(),
        DataServiceTypeName: $("#serviceName").val()
    };
    showLoader();
    $.ajax({
        url: urls.DataServiceType.SaveDataServiceType,
        type: 'POST',
        data: dataServiceTypeViewModel || (dataServiceTypeViewModel = null),
        success: function (result) {
            hideLoader();
            if (result.statusCode == statuMessageCode.Success) {
                SuccessMsg("Success", "Record Added successfully", "success");
                $('#DataServiceModal').modal('hide');
            }
            else if (result.statusCode == statuMessageCode.Exist) {
                ErrorMsg("Failed", "Record Already Exist", "error");
                $('#DataServiceModal').modal('hide');
            }
            else if (result.statusCode == statuMessageCode.Updated) {
                SuccessMsg("Success", "Record Updated successfully", "success");
                $('#DataServiceModal').modal('hide');
            }
            else if (result.statusCode == statuMessageCode.Error) {
                ErrorMsg("Failed", "Enter valid data", "error")
                $('#DataServiceModal').modal('hide');
            }
            clearDataServiceTypeData();
            loadDataServiceTypeList();
        },
        error: function (xhr, status, error) {
            hideLoader();
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
            else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                $('#DataServiceModal').modal('hide');
                clearDataServiceTypeData();
                loadDataServiceTypeList();
            }
            else {
                ErrorMsg("Failed", error, "error");
            }
        }
    });


}

function deleteDataServiceType(dataServiceTypeId) {
    clearDataServiceTypeData();
    ConfirmMsg('Data Service Type', 'Are you sure want to delete this data service type?', 'Continue', event, function () {
        $.ajax({
            url: urls.DataServiceType.DeleteDataServiceType,
            type: 'DELETE',
            data: { dataServiceTypeId: dataServiceTypeId },
            success: function (result) {
                hideLoader();
                if (result.statusCode == statuMessageCode.Deleted) {
                    SuccessMsg("Success", "Record Deleted successfully", "success");
                    $('#DataServiceModal').modal('hide');
                }
                else if (result.statusCode == statuMessageCode.NotExist) {
                    ErrorMsg("Failed", "Record Not Exist", "error");
                    $('#DataServiceModal').modal('hide');
                }
                clearDataServiceTypeData();
                loadDataServiceTypeList();
            },
            error: function (xhr, status, error) {
                hideLoader();
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Failed", "Unauthorized Access");
                }
                else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                    ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                    $('#DataServiceModal').modal('hide');
                }
                else {
                    ErrorMsg("Failed", error, "error");
                }
            }
        });
    });
}

const statuMessageCode = {
    Exist: 405,
    Success: 200,
    Updated: 203,
    Failed: 400,
    Deleted: 204,
    Error: 401,
    NotExist: 406
}
