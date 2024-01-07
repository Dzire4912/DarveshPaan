function GetRole(Id) {
    
    
    $('#Edit_roleId').val(Id);
    $('#updatememberModal').modal('show');
    draggableModel('#updatememberModal');
    let reqParameter = {
        Id: Id
    }
    $.ajax({
        type: 'Post',
        data: reqParameter,
        async: false,
        url: urls.Role.GetRole,
        success: function (response) {

            $("#Edit_role").val(response.roleName)
            $('#Edit_departmentId').val(response.departmentName);
        },
        failure: function (response) {
            ErrorMsg('Error', response, '');
        }
    });
    $("#error_Edit_role").text('');
    $("#error_Edit_departmentId").text('');
}

function EditRole() {   
    
    let regex = /^[A-Za-z0-9 ]+$/
    let roleName = $('#Edit_role').val() || '';
    let DepartmentName = $('#Edit_departmentId :selected').val() || '';
    if (roleName == '') {
        $("#error_Edit_role").text("Please enter the rolename");
        $("#error_Edit_role").focus();

        return false;
    }


    if (regex.test($('#Edit_role').val()) == false) {
        $("#error_Edit_role").text("Please enter valid Role Name!");
        $("#error_Edit_role").focus();
        return false;
    }
    else {
        $("#error_Edit_role").text('');
    }
   
    let reqParameter = {
        roleName: roleName ,
        Id: $('#Edit_roleId').val()
        
    }
   
    $.ajax({
        type: 'Post',
        data: reqParameter,
        async: false,
        url: urls.Role.UpdateRole,
        success: function (response) {
            if (response == "Record Saved !") {
                /*SuccessMsg("Success", "Role Updated Successfully")*/
                window.location.reload();
            }
            else if (response == "RoleName is already exists!please try different role name.") {
                /*WarningMsg("Warning", response, '');*/
                window.location.reload();
            }
            else {
                ErrorMsg('Error', response, '');
                window.location.reload();
            }
        },
        failure: function (response) {
            ErrorMsg('Error', response, '');
        }
    });
}
function DeleteRole(Id) {
    ConfirmMsg('Role Management', 'Are you sure want to delete this Role?', 'Continue', event, function () {
        let reqParameter = {
            Id: Id
        }
        $.ajax({
            type: 'Post',
            data: reqParameter,
            async: false,
            url: urls.Role.DeleteRole,
            success: function (response) {
                if (response == "Record has been deleted successfully") {
                    window.location.reload();
                }
            },
            failure: function (response) {
                ErrorMsg('Error', response, '');
            }
        });
    })



}
function getappRole() {
    let filterInput = {
        OrgId: $('#appName').val()
    }

    $.ajax({
        type: 'Post',
        data: filterInput,
        async: false,
        url: urls.UserManagement.GetroleList,
        success: function (response) {
            let applicationSelect = $("#roleName");
            applicationSelect.empty();
            applicationSelect.append('<option value=""> Select Role </option>');
            $.each(response, function (index, item) {
                applicationSelect.append('<option value="' + item.value + '">' + item.text + '</option>');
            });
            applicationSelect.trigger("change");           

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
function searchRole() {

    let filterInput = {
        userType:$('#userTypes').val(),
        appId:$('#appName').val(),
        roleId:$('#roleName').val()
    }
    $.ajax({
        type: 'Post',
        data: filterInput,
        async: false,
        url: urls.Role.index,
        success: function (response) {
            $('#RoleList').html($(response).find("#RoleList").html());
            $('#RoleList').dataTable().fnDestroy();
            $('#RoleList').DataTable(
                {
                    language: {
                        paginate: {
                            first: '<i class="fa fa-angle-double-left"></i> First',
                            previous: '<i class="fa fa-angle-left"></i><i class="fa fa-angle-left"></i>',
                            next: '<i class="fa fa-angle-right"></i><i class="fa fa-angle-right"></i>',
                            last: 'Last <i class="fa fa-angle-double-right"></i>'
                        }
                    },
                    columnDefs: [
                        { "orderable": false, "targets": 2 }
                    ]
                });

        }
    });

}
function resetRoleFilter() {
    $('#userTypes').val(null).trigger('change');
    $('#appName').val("");
    $('#roleName').empty();

    let filterInput = {
        appId: $('#appName').val(),
        roleId: $('#roleName').val()
    }
    $.ajax({
        type: 'Post',
        data: filterInput,
        async: false,
        url: urls.Role.index,
        success: function (response) {
            $('#roleName').html($(response).find("#roleName").html());
            $('#RoleList').html($(response).find("#RoleList").html());
            $('#RoleList').dataTable().fnDestroy();
            $('#RoleList').DataTable({
                language: {
                    paginate: {
                        first: '<i class="fa fa-angle-double-left"></i> First',
                        previous: '<i class="fa fa-angle-left"></i><i class="fa fa-angle-left"></i>',
                        next: '<i class="fa fa-angle-right"></i><i class="fa fa-angle-right"></i>',
                        last: 'Last <i class="fa fa-angle-double-right"></i>'
                    }
                },
                columnDefs: [
                    { "orderable": false, "targets": 2 }
                ]
            });

        }
    });
}

function validateInputOnBlur(inputElement) {
    let inputValue = inputElement.value;
    let addRoleButton = $('#addNewMember');
    let hasCharacter = /[a-zA-Z]/.test(inputValue); // Check for at least one character
    if (!(hasCharacter)) {
        $('#roleName_Error').text("Input is invalid. It should contain at least one character");
        addRoleButton.prop('disabled', true);
    }
    else {
        $('#roleName_Error').text('');
        addRoleButton.prop('disabled', false);
    }
}

function validateInputOnBlurForUpdateRole(inputElement) {
    let inputValue = inputElement.value;
    let updateRoleButton = $('#updateRole');
    let hasCharacter = /[a-zA-Z]/.test(inputValue); // Check for at least one character
    if (!(hasCharacter)) {
        $('#error_Edit_role').text("Input is invalid. It should contain at least one character");
        updateRoleButton.prop('disabled', true);
    }
    else {
        $('#error_Edit_role').text('');
        updateRoleButton.prop('disabled', false);
    }
}

function getuserTypeRoles() {
    let userType = $('#userTypes').val();

    if (userType == 1) {
        $('#appNameFilter').hide();

        $.ajax({
            type: 'Post',
            async: false,
            url: urls.Role.GetTANRoleList,
            success: function (response) {
                let userRoles = $("#roleName");
                userRoles.empty();
                userRoles.append('<option value=""> Select Role </option>');
                $.each(response, function (index, item) {
                    userRoles.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
                userRoles.trigger("change");
            }
        });
    }
    else {
        $('#appNameFilter').show();
        let userRoles = $("#roleName");
        userRoles.empty();
        userRoles.append('<option value=""> Select Role </option>');
        userRoles.trigger("change");
        $('#appName').val(null).trigger('change');
    }
}

function clearForm() {
    $("#appNameSelect").val('');
    $("#roleNameSelect").val('');
    $('.errormsg').text('');
}

