function loadStorageData() {
    $('#storageTable').DataTable({
        ajax: {
            url: urls.StorageManagement.GetAllFiles,
            type: "POST",
            data: function (data) {
                // You can pass additional data to the server if needed
                data.searchValue = $('#storageTable_filter input').val();
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
            { data: "name", name: "File Name" },
            {
                data: "size", name: "Size",
                render: function (data, type, full, meta) {
                    return formatFileSize(data);
                }
            },
            {
                data: null, orderable: false,
                render: function (data, type, full, row) {
                    return "<a href='#' onclick=downloadFile('" + encodeURIComponent(full.name) + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Download'><i class='ri-download-line fs-6 pe-2 text-info'></i></a>"
                        + " " + "<a href='#' onclick=deleteFile('" + encodeURIComponent(full.name) + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
                }
            }
        ],

    });
}

function formatFileSize(sizeInBytes) {
    if (sizeInBytes >= 1073741824) {
        return (sizeInBytes / 1073741824).toFixed(2) + ' GB';
    } else if (sizeInBytes >= 1048576) {
        return (sizeInBytes / 1048576).toFixed(2) + ' MB';
    } else if (sizeInBytes >= 1024) {
        return (sizeInBytes / 1024).toFixed(2) + ' KB';
    } else {
        return sizeInBytes + ' Bytes';
    }
}

$('#fileUpload').change(function (e) {
    if ($('#fileUpload').val().trim() != '') {
        fileUpload(e);
        $('#fileUpload').val('');
    }
});

function fileUpload(e) {
    let file = e.target.files;
    if (file.length > 0) {
        let data = new FormData();
        data.append("file", file[0]);
        $.ajax({
            url: urls.StorageManagement.UploadFile,
            type: 'POST',
            data: data,
            processData: false,
            contentType: false,
            success: function (result) {
                if (result == 'success')
                {
                    SuccessMsg("Success", "File Uploaded successfully", "success");
                }
                else if (result == 'exists')
                {
                    ErrorMsg("Failed", "The file you are trying to upload already exists on the cloud.", "error")
                }
                else if (result == 'sizeexceeds')
                {
                    ErrorMsg("Failed", "File size exceeds the maximum allowed size (100 MB).", "error")
                }
                else if (result == 'invalidextension')
                {
                    ErrorMsg("Failed", "Invalid file extension. Only XML, CSV, ZIP, XLS, XLSX files are allowed.", "error")
                }
                else if (result == 'error')
                {
                    ErrorMsg("Failed", "An error occurred while uploading the file to the cloud.", "error")
                }
                let table = $('#storageTable').DataTable();
                table.destroy();
                loadStorageData();
                $(".input-file__delete").click();
            },
            error: function (xhr, status, error) {
                hideLoader();
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Failed", "Unauthorized Access");
                }
            }
        });
    }
}

function downloadFile(fileName) {
    window.location = urls.StorageManagement.DownLoadFile + "?fileName=" + decodeURIComponent(fileName);
}

function deleteFile(fileName) {
    ConfirmMsg('Storage Management', 'Are you sure want to delete the file from cloud?', 'Continue', event, function () {
        $.ajax({
            url: urls.StorageManagement.DeleteFile,
            type: 'POST',
            data: { fileName: decodeURIComponent(fileName) },
            async: false,
            success: function (result) {
                if (result) {
                    SuccessMsg("Success", "Record Deleted successfully", "success");
                }
                else {
                    ErrorMsg("Failed", error, "error");
                }
                let table = $('#storageTable').DataTable();
                table.destroy();
                loadStorageData();
            },
            error: function (xhr, status, error) {
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Oops", "Unauthorized Access", "error");
                }
                else {
                    ErrorMsg("Failed", error, "error");
                }
            }
        });
    });
}

function processOneDriveInStorage() {
    let odOptions = {
        clientId: "3c39b444-9cde-4ddc-9878-a9c8b69197d2",
        action: "download",
        multiSelect: false,
        openInNewWindow: true,
        success: function (files) {
            /* success handler */
            $.each(files.value, function (index, element) {
                oneDriveUploadInStorage(element["@microsoft.graph.downloadUrl"], element.name);
            });
        },
        cancel: function () {
            ErrorMsg("warning", "You cancelled your upload.", "warning");
            let checkbox = document.getElementById("onedrive");
            checkbox.checked = false;
        },
        error: function (e) {
            hideLoader();
            ErrorMsg("Error", "Something went wrong.", "error");
        }
    }
    OneDrive.open(odOptions);
}

function oneDriveUploadInStorage(fileURL, fileName) {
    let data = new FormData();
    data.append("Filename", fileName);
    data.append("FileURL", fileURL);
    data.append("IsOneDrive", true);
    $.ajax({
        url: urls.StorageManagement.UploadFile,
        type: 'POST',
        data: data,
        processData: false,
        contentType: false,
        success: function (result) {
            if (result == 'success')
            {
                SuccessMsg("Success", "File Uploaded successfully", "success");
            }
            else if (result == 'exists')
            {
                ErrorMsg("Failed", "The file you are trying to upload already exists on the cloud.", "error")
            }
            else if (result == 'sizeexceeds')
            {
                ErrorMsg("Failed", "File size exceeds the maximum allowed size (100 MB).", "error")
            }
            else if (result == 'invalidextension')
            {
                ErrorMsg("Failed", "Invalid file extension. Only XML, CSV, ZIP, XLS, XLSX files are allowed.", "error")
            }
            else if (result == 'error')
            {
                ErrorMsg("Failed", "An error occurred while uploading the file to the cloud.", "error")
            }
            let checkbox = document.getElementById("onedrive");
            checkbox.checked = false;
            let table = $('#storageTable').DataTable();
            table.destroy();
            loadStorageData();
        },
        error: function (xhr, status, error) {
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
        }
    });
}
