function getFacilities() {
    showLoader();
    let organizationName = $('#organizationName').val();
    if ($("#organizationName").val() != "") {
        $.ajax({
            type: 'Post',
            data: {
                organizationName: organizationName
            },
            async: false,
            cache: false,
            url: urls.SubmissionHistoryManagement.GetFacilities,
            success: function (response) {
                hideLoader();
                let facilitySelect2 = $("#facilityName");
                facilitySelect2.empty();
                facilitySelect2.append('<option value="">- Select Facility Name -</option>');
                $.each(response, function (index, item) {
                    facilitySelect2.append('<option value="' + item.facilityName + '">' + item.facilityName + '</option>');
                });
                facilitySelect2.trigger("change");
            },
            failure: function () {
                hideLoader();
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
                $('#preloader').addClass('d-none');
            },
            error: function () {
                hideLoader();
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
                $('#preloader').addClass('d-none');
            }
        });
    }


}

function clearFilter() {
    $('#organizationName').val(null).trigger('change');
    $('#facilityName').val(null).trigger('change');
}

function initSelect2ForSubmissionHistory() {
    $('#organizationName').select2({
        placeholder: "Select Organization Name",
        width: '100%'
    }).on('select2:close', function () {
        let placeholderText = 'Select Organization Name'; // Replace with your desired placeholder text
        if ($(this).val() == null) {
            $(this).val('').trigger('change');
            $(this).next('.select2-container').find('.select2-search__field').attr('placeholder', placeholderText);
        }
    });

    $('#facilityName').select2({
        placeholder: "Select Facility Name",
        width: '100%'
    }).on('select2:close', function () {
        let placeholderText = 'Select Facility Name'; // Replace with your desired placeholder text
        if ($(this).val() == null) {
            $(this).val('').trigger('change');
            $(this).next('.select2-container').find('.select2-search__field').attr('placeholder', placeholderText);
        }
    });
}

function LoadSubmissionData() {
    let orgName = $('#organizationName').val();
    if (orgName == 'Select Organization Name')
        orgName = null;
    let facilityName = $('#facilityName').val();
    if (facilityName == 'Select Facility Name')
        facilityName = null;
    let formData = {
        orgName: orgName,
        facilityName: facilityName
    };

    $('#historyTable').DataTable({
        ajax: {
            url: urls.SubmissionHistoryManagement.GetSubmissions,
            type: "POST",
            data: function (data) {
                // You can pass additional data to the server if needed
                data.searchValue = $('#historyTable_filter input').val();
                data.start = data.start || 0; // start parameter
                data.length = data.length || 10; // length parameter
                data.draw = data.draw || 1; // draw parameter
                data.sortColumn = data.columns[data.order[0].column].data; // sort column
                data.sortDirection = data.order[0].dir;
                data.orgName = formData.orgName;
                data.facilityName = formData.facilityName;
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
            { data: "submissionDate", name: "Date" },
            {
                data: "fileName", name: "File Name", render: function (data, type, row, meta) {
                    // 'data' represents the value in the "fileName" column for each row
                    return '<a href="' + data + '" target="_blank" onclick="downloadFile(\'' + data + '\')">' + data + '</a>';
                }
            },
            {
                data: "organizationName", name: "Organization Name"
            },
            {
                data: "facilityName", name: "Facility Name"
            },
            {
                data: "status", name: "Status", render: function (data, type, row, meta) {
                    if (data === 0) {
                        return 'Pending';
                    } else if (data === 1) {
                        return 'Accepted';
                    } else if (data === 2) {
                        return 'Rejected';
                    }
                }
            },
            {
                data: null, orderable: false,
                render: function (data, type, full, row) {
                    return "<a href='#' onclick=editStatus('" + full.fileName + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Edit'><i class='ri-edit-2-line fs-6 pe-2 text-info'></i></a>"
                        + " " + "<a href='#' onclick=deleteSubmissionFileHistory('" + full.fileName + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
                }
            }
        ],
    });
}

function downloadFile(fileName) {
    window.location = urls.SubmissionHistoryManagement.DownloadFile + "?fileName=" + fileName;
}

function deleteSubmissionFileHistory(fileName) {
    let token = $('input[name="__RequestVerificationToken"]').val();
    ConfirmMsg('Submission History Management', 'Are you sure want to delete the history of submitted file?', 'Continue', event, function () {
        $.ajax({
            url: urls.SubmissionHistoryManagement.DeleteSubmision,
            type: 'POST',
            headers: { 'RequestVerificationToken': token },
            data: { fileName: fileName },
            async: false,
            success: function (result) {
                if (result == 'success') {
                    SuccessMsg("Success", "Record Deleted successfully", "success");
                }
                else {
                    ErrorMsg("Failed", error, "error")
                }
                let table = $('#historyTable').DataTable();
                table.destroy();
                LoadSubmissionData();
            },
            error: function (xhr, status, error) {
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Oops", "Unauthorized Access", "error")
                }
                else {
                    ErrorMsg("Failed", error, "error")
                }
            }
        });
    });
}

function editStatus(fileName) {
    showLoader();
    $.ajax({
        url: urls.SubmissionHistoryManagement.EditStatus,
        type: 'GET',
        data: { fileName: fileName },
        success: function (result) {
            hideLoader();
            $('#btnChangeStatus').trigger('click');
            $('#FileEditDate').val(result.createDate);
            $('#FileEditName').val(result.fileName);
            $('#StatusSelectBox').val(result.status).trigger('change');
            $('#FileID').val(result.id);
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

function clearEditStatusData() {
    $('#FileEditDate').val('');
    $('#FileEditName').val('');
    $('#StatusSelectBox').val(null).trigger('change');
    $('#SubmissionStatusModal').hide();
    window.location.reload();
    let table = $('#historyTable').DataTable();
    table.destroy();
    LoadSubmissionData();
}

function updateSubmissionData() {
    let token = $('input[name="__RequestVerificationToken"]').val();
    let fileId = $('#FileID').val();
    let fileDate = $('#FileEditDate').val();
    let fileName = $('#FileEditName').val();
    let fileStatus = $('#StatusSelectBox').val();

    let formData = {
        fileId: fileId,
        fileDate: fileDate,
        fileName: fileName,
        fileStatus: fileStatus
    };
    showLoader();
    $.ajax({
        url: urls.SubmissionHistoryManagement.UpdateSubmissionData,
        type: 'POST',
        headers: { 'RequestVerificationToken': token },
        data: formData,
        async: false,
        success: function (result) {
            hideLoader();
            if (result == 'success') {
                SuccessMsg("Success", "Record Added successfully", "success");
                $('#SubmissionStatusModal').modal('hide');
            }
            else {
                $('#SubmissionStatusModal').modal('hide');
                ErrorMsg("Failed", error, "error")
            }
            let table = $('#historyTable').DataTable();
            table.destroy();
            LoadSubmissionData();
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