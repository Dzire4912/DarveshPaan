$('#fileUpload').change(function (e) {
    if ($('#fileUpload').val().trim() != '') {
        fileUpload(e);
        $('#fileUpload').val('');
    }
});

function fileUpload(e) {
    let file = e.target.files;
    let organizationId = $('#organizationName').val();
    if (organizationId == 'Select Organization Name' || organizationId == null || organizationId == '' || organizationId == '0') {
        ErrorMsg("Failed", "Please Select Organization Name.", "error")
        $(".input-file__delete").click();
        return false;
    }
    let facilityId = $('#facilityName').val();
    if (facilityId == 'Select Facility Name' || facilityId == null || facilityId == '' || facilityId == '0') {
        ErrorMsg("Failed", "Please Select Facility Name.", "error")
        $(".input-file__delete").click();
        return false;
    }

    let carrierName = $('#carrierName option:selected').text();
    if (carrierName == 'Select Carrier Name' || carrierName == null || carrierName == '' || carrierName == '0') {
        ErrorMsg("Failed", "Please Select Carrier Name.", "error")
        $(".input-file__delete").click();
        checkbox.checked = false;
        return false;
    }

    if (file.length > 0 && facilityId != null) {
        let data = new FormData();
        data.append("UploadedFile", file[0]);
        data.append("FacilityId", facilityId);
        data.append("OrganizationId", organizationId);
        data.append("CarrierName", carrierName);
        $.ajax({
            url: urls.TelecomReports.UploadFileToAzureBlobStorage,
            type: 'POST',
            data: data,
            processData: false,
            contentType: false,
            success: function (result) {
                if (result == 'success') {
                    SuccessMsg("Success", "File Uploaded successfully", "success");
                }
                else if (result == 'exists') {
                    ErrorMsg("Failed", "The file you are trying to upload already exists on the cloud.", "error");
                }
                else if (result == 'sizeexceeds') {
                    ErrorMsg("Failed", "File size exceeds the maximum allowed size (100 MB).", "error");
                }
                else if (result == 'invalidextension') {
                    ErrorMsg("Failed", "Invalid file extension. Only PDF, JPEG,JPG and PNG files are allowed.", "error");
                }
                else if (result == 'error' || result == 'failed' || result == 'Failed') {
                    ErrorMsg("Failed", "An error occurred while uploading the file to the cloud.", "error");
                }
                clearUploadFileDetails();
                $(".input-file__delete").click();
                let table = $('#clientFilesTable').DataTable();
                table.destroy();
                loadAllFilesFromAzureStorage();
            },
            error: function (xhr, status, error) {
                hideLoader();
                if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                    ErrorMsg("Failed", "Unauthorized Access");
                }
                else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                    ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                    clearUploadFileDetails();
                    $(".input-file__delete").click();
                    let table = $('#clientFilesTable').DataTable();
                    table.destroy();
                    loadAllFilesFromAzureStorage();
                }
            }
        });
    }
}

function getOrgFacilities() {
    let orgId = $('#organizationName').val();
    if (orgId != "" && orgId != 'Select Organization Name') {
        $.ajax({
            type: "Post",
            data: { orgId: orgId },
            async: false,
            cache: false,
            url: urls.TelecomReports.GetOrganizationFaciltyList,
            success: function (response) {
                let orgFacilities = $("#facilityName");
                orgFacilities.empty();
                orgFacilities.append('<option>Select Facility Name</option>');
                $.each(response, function (index, item) {
                    orgFacilities.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
                orgFacilities.val(null).trigger('change');
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
}

function getOrgFacilitiesForFilter() {
    let orgId = $('#organizationNameFilter').val();
    if (orgId != "" && orgId != 'Select Organization Name') {
        $.ajax({
            type: "Post",
            data: { orgId: orgId },
            async: false,
            cache: false,
            url: urls.TelecomReports.GetOrganizationFaciltyList,
            success: function (response) {
                let orgFacilities = $("#facilityNameFilter");
                orgFacilities.empty();
                orgFacilities.append('<option value="0">Select Facility Name</option>');
                $.each(response, function (index, item) {
                    orgFacilities.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
                orgFacilities.val('0').trigger('change');
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
}

function loadAllFilesFromAzureStorage() {
    let orgName = $('#organizationNameFilter option:selected').text();
    if (orgName == 'Select Organization Name')
        orgName = null;
    let facilityName = $('#facilityNameFilter option:selected').text();
    if (facilityName == 'Select Facility Name')
        facilityName = null;
    let carrierName = $('#carrierNameFilter option:selected').text();
    if (carrierName == 'Select Carrier Name')
        carrierName = null;

    let isThinkAnewUser = getLoggedInUserInfo();
    let orgId = "";
    if (!isThinkAnewUser) {
        orgId = getOrganizationUserDetails();
    }
    let formData = {
        orgName: orgName,
        facilityName: facilityName,
        carrierName: carrierName,
        orgId: orgId
    };
    $('#clientFilesTable').DataTable(
        {
            ajax: {
                url: urls.TelecomReports.GetAllUploadedFileList,
                type: "POST",
                data: function (data) {
                    // You can pass additional data to the server if needed
                    data.searchValue = $('#clientFilesTable_filter input').val();
                    data.start = data.start || 0; // start parameter
                    data.length = data.length || 10; // length parameter
                    data.draw = data.draw || 1; // draw parameter
                    data.sortColumn = data.columns[data.order[0].column].data; // sort column
                    data.sortDirection = data.order[0].dir;
                    data.orgName = formData.orgName;
                    data.facilityName = formData.facilityName;
                    data.carrierName = formData.carrierName;
                    data.orgId = formData.orgId;
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
                { data: "facilityName", name: "Facility Name" },
                { data: "carrierName", name: "Carrier Name" },
                { data: "fileName", name: "File Name" },
                {
                    data: "fileSize", name: "File Size",
                    render: function (data, type, full, meta) {
                        return formatFileSize(data);
                    }
                },
                { data: "createdDate", name: "Created Date" },
                {
                    data: "isProcessed", name: "IsProcessed", orderable: true,
                    render: function (data, type, full, row) {
                        let content = '';
                        let displayText = '';
                        let className = '';
                        switch (full.isProcessed) {
                            case false:
                                displayText = 'N';
                                className = 'btn-danger';
                                content = 'Not Processed';
                                break;
                            case true:
                                displayText = 'Y';
                                className = 'btn-success';
                                content = 'Processed';
                                break;
                            default:
                                displayText = 'NA';
                                className = 'btn-warning';
                                content = 'Not Applicable';
                        }
                        return `<span data-order="${full.isProcessed}" data-bs-toggle="tooltip" data-bs-placement="top" title="${content}" class="btn-circle btn ${className} text-white btn-circle-custom">
                            <span class="${className}">${displayText}</span>
                        </span>`;
                    }
                },
                {
                    data: null, orderable: false,
                    render: function (data, type, full, row) {
                        return "<a href='#' onclick=displayFile('" + encodeURIComponent(full.filename).replace(/'/g, '%27') + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='View Uploaded File' style='color: black' ><i class='ri-eye-line fs-6 pe-2 text-success'></i></a>"
                            + "<a href='#' onclick=downloadFile('" + encodeURIComponent(full.fileName) + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Download'><i class='ri-download-line fs-6 pe-2 text-info'></i></a>"
                            + " " + "<a href='#' onclick=deleteFile('" + encodeURIComponent(full.fileName) + "'); data-bs-toggle='tooltip' data-bs-placement='top' title='Delete'><i class='ri-delete-bin-line fs-6 text-danger'></i></a>";
                    }
                }
            ]
        }
    );
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

function downloadFile(fileName) {
    window.location = urls.TelecomReports.DownLoadFile + "?fileName=" + decodeURIComponent(fileName);
}

function displayFile(fileName) {
    debugger;
    $.ajax({
        url: urls.TelecomReports.DisplayFile,
        type: 'GET',
        data: { fileName: decodeURIComponent(fileName) },
        xhrFields: {
            responseType: 'blob'
        },
        success: function (response) {
            if (response instanceof Blob) {
                // Determine the file extension from the fileName
                let fileExtension = fileName.split('.').pop().toLowerCase();

                if (['pdf', 'png', 'jpg', 'jpeg'].includes(fileExtension)) {
                    // Display the PDF or image content in the modal
                    let fileURL = URL.createObjectURL(response);

                    if (fileExtension === 'pdf') {
                        $('#pdfViewer').attr('src', fileURL);
                        $('#fileModal').show();
                    } else {
                        // Handle image file types (png, jpg, jpeg)
                        // You can use an <img> element or any other method to display images
                        $('#imageViewer').attr('src', fileURL);
                        $('#imageModal').show();
                    }
                } else {
                    ErrorMsg("Error", "Unsupported file type. Unable to view file", "error");
                }
            }
        },
        error: function (error) {
            ErrorMsg("Error", "Something went wrong. Unable to view file", "error");
        }
    });
}

function deleteFile(fileName) {
    ConfirmMsg('Telecom Reporting: Data Service Management', 'Are you sure want to delete the file from cloud?', 'Continue', event, function () {
        $.ajax({
            url: urls.TelecomReports.DeleteFile,
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
                let table = $('#clientFilesTable').DataTable();
                table.destroy();
                loadAllFilesFromAzureStorage();
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

function getLoggedInUserInfo() {
    let isThinkAnewUser = false;
    showLoader();
    $.ajax({
        url: urls.TelecomReports.GetLoggedInUserInfo,
        type: 'GET',
        async: false,
        success: function (result) {
            hideLoader();
            if (result != null) {
                isThinkAnewUser = result;
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

    return isThinkAnewUser;
}

function getOrganizationUserDetails() {
    let organizationId = null;
    showLoader();
    $.ajax({
        url: urls.TelecomReports.GetOrganizationUserInfo,
        type: 'GET',
        async: false,
        success: function (result) {
            hideLoader();
            if (result != null) {
                organizationId = result;
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

    return organizationId;
}

function initSelect2ForUploadClientFile() {
    $('#organizationName').select2({
        placeholder: "Select Organization Name",
        dropdownParent: $('#organizationNameDiv'),
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#facilityName').select2({
        placeholder: "Select Facility Name",
        dropdownParent: $('#facilityNameDiv'),
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });

    $('#carrierName').select2({
        placeholder: "Select Carrier Name",
        dropdownParent: $('#carrierNameDiv'),
        width: '100%'
    }).next().addClass('form-control').on('select2:open', function () {

        $('.select2-dropdown--above').attr('id', 'fix');
        $('#fix').removeClass('select2-dropdown--above');
        $('#fix').addClass('select2-dropdown--below');

    });
}

function processOneDriveInUploadClientFile() {
    let checkbox = document.getElementById("onedrive");
    let organizationId = $('#organizationName').val();
    if (organizationId == 'Select Organization Name' || organizationId == null || organizationId == '0') {
        ErrorMsg("Failed", "Please Select Organization Name.", "error")
        $(".input-file__delete").click();
        checkbox.checked = false;
        return false;
    }
    let facilityId = $('#facilityName').val();
    if (facilityId == 'Select Facility Name' || facilityId == null || facilityId == '' || facilityId == '0') {
        ErrorMsg("Failed", "Please Select Facility Name.", "error")
        $(".input-file__delete").click();
        checkbox.checked = false;
        return false;
    }
    let carrierName = $('#carrierName option:selected').text();
    if (carrierName == 'Select Carrier Name' || carrierName == null || carrierName == '' || carrierName == '0') {
        ErrorMsg("Failed", "Please Select Carrier Name.", "error")
        $(".input-file__delete").click();
        checkbox.checked = false;
        return false;
    }
    let oneDriveOptions = {
        clientId: "3c39b444-9cde-4ddc-9878-a9c8b69197d2",
        action: "download",
        multiSelect: false,
        openInNewWindow: true,
        success: function (files) {
            /* success handler */
            $.each(files.value, function (index, element) {
                oneDriveFileUpload(element["@microsoft.graph.downloadUrl"], element.name);
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
    OneDrive.open(oneDriveOptions);
}

function oneDriveFileUpload(fileURL, fileName) {
    let organizationId = $('#organizationName').val();
    let facilityId = $('#facilityName').val();
    let carrierName = $('#carrierName').val();

    let data = new FormData();
    data.append("FacilityId", facilityId);
    data.append("OrganizationId", organizationId);
    data.append("CarrierName", carrierName);
    data.append("FileName", fileName);
    data.append("FileURL", fileURL);
    data.append("IsOneDrive", true);

    $.ajax({
        url: urls.TelecomReports.UploadFileToAzureBlobStorage,
        type: 'POST',
        data: data,
        processData: false,
        contentType: false,
        success: function (result) {
            if (result == 'success') {
                SuccessMsg("Success", "File Uploaded successfully", "success");
            }
            else if (result == 'exists') {
                ErrorMsg("Failed", "The file you are trying to upload already exists on the cloud.", "error");
            }
            else if (result == 'sizeexceeds') {
                ErrorMsg("Failed", "File size exceeds the maximum allowed size (100 MB).", "error");
            }
            else if (result == 'invalidextension') {
                ErrorMsg("Failed", "Invalid file extension. Only PDF, JPEG,JPG and PNG files are allowed.", "error");
            }
            else if (result == 'error' || result == 'failed' || result == 'Failed') {
                ErrorMsg("Failed", "An error occurred while uploading the file to the cloud.", "error");
            }
            let checkbox = document.getElementById("onedrive");
            checkbox.checked = false;
            clearUploadFileDetails();
            let table = $('#clientFilesTable').DataTable();
            table.destroy();
            loadAllFilesFromAzureStorage();
        },
        error: function (xhr, status, error) {
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
            else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                clearUploadFileDetails();
                $(".input-file__delete").click();
                let table = $('#clientFilesTable').DataTable();
                table.destroy();
                loadAllFilesFromAzureStorage();
            }
        }
    });
}

function clearUploadFileDetails() {
    $('#facilityName').val('0').trigger('change');
    $('#organizationName').val('0').trigger('change');
    $('#carrierName').val('0').trigger('change');
    $(".input-file__delete").click();
    $('#UploadFileModal').modal('hide');
}

function clearFilter() {
    $('#organizationNameFilter').val('0').trigger('change');
    $('#facilityNameFilter').val('0').trigger('change');
    $('#carrierNameFilter').val('0').trigger('change');
}

function closeFileViewModal() {
    $('#fileModal').hide();
    $('#imageModal').hide();
}