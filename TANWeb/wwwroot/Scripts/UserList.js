function GetFacility() {
    
    let role = "";
    let OrgId = "";


    let filterInput = {
        OrgId: $("#organizationName").val(),
    }
    if ($("#organizationName").val() != "") {
        $.ajax({
            type: 'Post',
            data: filterInput,
            async: false,
            cache: false,
            url: urls.UserManagement.FaciltyList,
            success: function (response) {
                let applicationSelect = $("#facilityName");
                applicationSelect.empty();
                applicationSelect.append('<option value="">- Select Facility Name -</option>');
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


}

function ClearSpan() {
    $('.validation').empty();
}
function getRole() {
    let filterInput = {
        OrgId: $('#drpAppList').val()
    }

    $.ajax({
        type: 'Post',
        data: filterInput,
        async: false,
        url: urls.UserManagement.GetroleList,
        success: function (response) {
            let applicationSelect = $("#drproleList");
            applicationSelect.empty();
            applicationSelect.append('<option value=""> Select Role </option>');
            $.each(response, function (index, item) {
                applicationSelect.append('<option value="' + item.value + '">' + item.text + '</option>');
            });
            applicationSelect.trigger("change");
            $('#permissionTable').empty();
            $('#permissionTable').dataTable().fnDestroy();
            $('#permissionTable').DataTable({
                pagingType: "full_numbers",
                language: {
                    paginate: {
                        first: '<i class="fa fa-angle-double-left"></i>',
                        previous: '<i class="fa fa-angle-left"></i>',
                        next: '<i class="fa fa-angle-right"></i>',
                        last: '<i class="fa fa-angle-double-right"></i>',
                    },
                    emptyTable: "No data available, kindly ensure the module is selected"
                },
                pageLength: 50,
                columnDefs: [
                    { orderable: false, targets: [2] }
                ]
            });
            $("#drpModuleList").empty();

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

function FixType() {
    let type = $("#userType").val();
    if (type == 1) {
        $("#TypeFilter").hide();
        $("#organizationName").val("");
        $("#organizationName").trigger("change");
        $("#facilityName").val("");
        $("#facilityName").trigger("change");
        $('#facilityName').find('option').remove().end().trigger('change');
        $('#defApp').find('option').remove().end().trigger('change');

        $('#ThinkAnewTypeFilter').show();

    }
    else {
        $("#TypeFilter").show();
        $('#ThinkAnewTypeFilter').hide();
        $("#roleName").val("");
        $("#roleName").trigger("change");
        $("#appName").val("");
        $("#appName").trigger("change");
        $('#defApp').find('option').remove().end().trigger('change');
    }
    setTimeout(function () {
        GetApps();
    }, 0);

}

function GetFacilityApplication() {
    let selectedFacilities = $('#facilityName').val();
    if (selectedFacilities.length > 0) {
        if (selectedFacilities[0] != "") {
            $.ajax({
                url: urls.UserManagement.FaciltyAppList,
                type: 'GET',
                dataType: 'json',
                cache: false,
                traditional: true,
                data: { apps: selectedFacilities }, // Pass the data as an object with the appropriate parameter name
                contentType: 'application/json',
                success: function (response) {
                    // Handle the response from the controller
                    let applicationSelect = $("#defApp");
                    applicationSelect.empty();
                    applicationSelect.append('<option value="">- Select Application Name -</option>');
                    $.each(response, function (index, item) {
                        applicationSelect.append('<option value="' + item.value + '">' + item.text + '</option>');
                    });
                    applicationSelect.trigger("change");
                },
                error: function (error) {
                    // Handle any errors that occur during the AJAX request
                }
            });
        }
    }
}

function GetEditFacilityApplication() {

    let selectedFacilities = $('#facility').val();

    if (selectedFacilities.length > 0) {
        if (selectedFacilities[0] != "") {
            $.ajax({
                url: urls.UserManagement.FaciltyAppList,
                type: 'GET',
                dataType: 'json',
                cache: false,
                traditional: true,
                data: { apps: selectedFacilities }, // Pass the data as an object with the appropriate parameter name
                contentType: 'application/json',
                success: function (response) {
                    // Handle the response from the controller
                    let applicationSelect = $("#defApp");
                    applicationSelect.empty();
                    applicationSelect.append('<option value="">- Select Application Name -</option>');
                    $.each(response, function (index, item) {
                        applicationSelect.append('<option value="' + item.value + '">' + item.text + '</option>');
                    });
                    applicationSelect.trigger("change");
                },
                error: function (error) {
                    // Handle any errors that occur during the AJAX request
                }
            });
        }
    }
}
function searchUser() {
    $('#preloader').removeClass('d-none');
    let role = "";
    if ($("#RoleName :selected").val() != "") {
        role = $("#RoleName :selected").text();
    }
    let filterInput = {
        userName: $('#UserName').val(),
        role: role,
        fOrgId: $('#OrgName').val(),
        fFacilityId: $('#FacilityNameFilter').val()
    }

    $.ajax({
        type: 'Post',
        data: filterInput,
        async: false,
        url: urls.UserManagement.SearchUserData,
        success: function (response) {
            $("#PBJEmployeeList").html($(response).find("#PBJEmployeeList").html());
            $('#PBJEmployeeList').dataTable().fnDestroy();
            $('#PBJEmployeeList').dataTable({

                order: [],
                columnDefs: [
                    { orderable: false, targets: 4 }
                ]
            });
            $('#preloader').addClass('d-none');
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

function resetUserFilter() {
    $("#UserName").val('');
    $("#RoleName").val(null).trigger('change');
    $("#OrgName").val(null).trigger('change');
    $("#FacilityNameFilter").val(null).trigger('change');
    //window.location.reload();
    searchUser();
}
function editUser() {

    let form = document.getElementById('EditUser');
    let userInput = new FormData(form);
    $.ajax({
        type: 'Post',
        data: userInput,
        async: false,
        url: urls.UserManagement.UpdateUser,
        success: function (response) { }
    })

}


function updateActiveStatus(Id, flag) {

    let status = "";
    if (flag == '0') {
        status = "Deactivate";
    }
    else {
        status = "Activate";
    }
    let confirmationMsg = "Are you sure want to " + status + " this Employee?";
    ConfirmMsg('User Management', confirmationMsg, 'Continue', event, function () {

        let statusInput = {
            userId: Id,
            status: flag,
        }
        $.ajax({
            type: 'Post',
            data: statusInput,
            async: false,
            url: urls.UserManagement.UpdateActiveStatus,
            success: function (response) {
                if (response == "success") {
                    SuccessMsg('Success', "Active status has been changed", '');
                    window.location.reload();
                }
                else {
                    ErrorMsg('Error', "Unable to change status!Try again later", '');
                }
            },
            failure: function () {
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
            },
            error: function () {
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
            }
        });
    });

}

//function resetAdUserSearch() {
//    $("#txtUser").val('');
//    /*SearchUserFromAd();*/
//    refreshUserModel();
//}
function searchUserFromAd() {
    $('#preloader').removeClass('d-none');
    $("#AdMsg").val('');
    if ($("#txtUser").val() == "") {
        $("#txtUser").focus();
        return false;
    }
    let userInput = {
        userName: $("#txtUser").val()
    }
    $.ajax({
        type: 'Post',
        data: userInput,
        async: false,
        url: urls.UserManagement.SearchADUserData,
        success: function (response) {
            if (response != null && response.isAdAuthenticated == true) {
                $("#txtName").val(response.name);
                $("#txtEmail").val(response.email);
                $("#hdIsAdEnabled").val('true');
            }
            else {
                $("#AdMsg").text("This UserName is not authenticated from AD. ")
            }
            $('#preloader').addClass('d-none');
        },
        failure: function () {
            $('#preloader').addClass('d-none');
            $("#AdMsg").text("This UserName is not authenticated from AD. ")
        },
        error: function () {
            $('#preloader').addClass('d-none');
            $("#AdMsg").text("This UserName is not authenticated from AD. ")
        }
    });
}

function isValidEmail(email) {
    // Regular expression pattern for email format validation
    let emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailPattern.test(email);
}

function addUser() {
    const userTypesEnum = {
        Thinkanew: 1,
        Other: 2
    };

    let currentUser = $("#currentType").val();
    let currentUserOrg = $("#currentOrg").val();
    let firstName = $("#txtfName").val();
    let lastName = $("#txtlName").val();
    let email = $('#txtEmail').val();
    let userType = $("#userType").val();
    let organizationName = $("#organizationName").val();
    let facilityName = $("#facilityName").val();
    let defApp = $("#defApp").val();
    let roleName = $("#roleName").val();
    if (currentUser == userTypesEnum.Thinkanew) {
        // let appName = $("#appName").val();
        if (userType != userTypesEnum.Thinkanew) {
            if (validateOtherUser(firstName, lastName, email, userType, organizationName, facilityName, defApp)) {
                addUserInfo();
            }
        }
        else if (validateTANUser(firstName, lastName, email, userType, roleName, defApp)) {
            addUserInfo();
        }
    }
    else {

        if (validateOtherUser(firstName, lastName, email, '2', currentUserOrg, facilityName, defApp)) {
            $('#userType').val('2');

            addUserInfo();
            $('#userType').trigger('change');
            $('#organizationName').val(currentUserOrg);
            $('#organizationName').trigger('change');
        }
    }
}

function CheckEdit(event) {
    const userTypesEnum = {
        Thinkanew: 1,
        Other: 2
    };
    let firstName = $("#edtxtfName").val();
    if ($("#edtxtfName").val() == "") {

        $("#spedtxtfName").text("Enter First Name!");

        event.preventDefault();
    }
    else if (/\d/.test(firstName)) { // Check if there are any numbers
        $('#spedtxtfName').text('First Name should not contain numbers');
        // $("#txtfName").focus();
        return false;
    } else if (/[^A-Za-z\s\-']/.test(firstName)) {
        $('#spedtxtlName').text('First Name should not contain symbols other than hyphen (-)');
        event.preventDefault();
    }
    let lastName = $("#edtxtlName").val();
    if ($("#edtxtlName").val() == "") {

        $("#spedtxtlName").text("Enter Last Name!");
        //  $("#txtlName").focus();
        event.preventDefault();
    }
    else if (/\d/.test(lastName)) { // Check if there are any numbers
        $('#spedtxtlName').text('First Name should not contain numbers');
        //  $("#txtlName").focus();
        event.preventDefault();
    } else if (/[^A-Za-z\s\-']/.test(lastName)) {
        $('#spedtxtlName').text('Last Name should not contain symbols other than hyphen (-)');
        event.preventDefault();
    }

    let newemail = $('#txtEmail').val();
    if ($("#txtEmail").val() == "") {

        $("#spemailtxt").text("Enter Email!");
        // $("#txtEmail").focus();
        event.preventDefault();
    }

    else if (!isValidEmail(newemail)) { // Check email format using a regular expression
        $('#spemailtxt').text('Invalid email format');
        event.preventDefault();
    }
    let facility = $('#facility').val();
    let userType = $("#userType").val();
    if (userType != userTypesEnum.Thinkanew) {
        if (facility.length == 0) {
            $('#spapplication').text("Select Facility");
            event.preventDefault();
        }
        else if (facility[0] == "") {
            $('#spapplication').text("Select Facility");
            event.preventDefault();
        }
    }
    let defApp = $('#defApp').val();
    if (defApp == "") {
        $("#spdefaultApplication").text("Select Default Application!");
        event.preventDefault();
    }

}

function refreshUserModel() {
    // Reset the form
    $('#AddADUserInform')[0].reset();
    //chosen 
    $('#organizationName').val('').trigger('change');
    // Clear any error messages
    $('.validation').empty();
    $('#userType').val("");
    $('#userType').trigger('change');
    $("#organizationName").val("");
    $("#organizationName").trigger("change");
    $("#facilityName").val("");
    $("#facilityName").trigger("change");
    $('#facilityName').find('option').remove().end().trigger('change');
    $('#defApp').find('option').remove().end().trigger('change');
    // Clear any error messages
    $('.validation').empty();
}

function GetApps() {
    $.ajax({
        type: 'Post',
        async: false,
        cache: false,
        url: urls.UserManagement.GetAppList,
        success: function (response) {
            let applicationSelect = $("#defApp");
            applicationSelect.empty();
            applicationSelect.append('<option value="">- Select Application Name -</option>');
            $.each(response, function (index, item) {
                applicationSelect.append('<option value="' + item.value + '">' + item.text + '</option>');
            });
            applicationSelect.trigger("change");
        }
    });
}

function loadAddUserModalPopUp() {
    $('#btnAddUser').trigger('click');
    let organizationId = sessionStorage.getItem("organizationId").trim();
    let facilityId = sessionStorage.getItem("facilityId").trim();
    $('#organizationName').val(organizationId).trigger('change');
    $('#facilityName').val(facilityId).trigger('change');
    $('#userType').val('2');
    setTimeout(function () {
        $('#userType').trigger('change');
        $('#facilityName').trigger('change');
    }, 100);
}

$('#userType').on('change', function () {
    // Get the selected value
    let selectedValue = $(this).val();

    // Show the "TypeFilter" div if the selected value is "2"; otherwise, hide it
    if (selectedValue === '2') {
        $("#TypeFilter").show();
    } else {
        $("#TypeFilter").hide();
    }
});

function restrictInput(event) {
    let pattern = /^[A-Za-z\s\-']+$/;
    let key = event.key;
    if (pattern.test(key) || key === 'Backspace') {
        return;
    }
    event.preventDefault();
}

function checkForUniqueEmail() {
    let emailId = $('#txtEmail').val();
    let userId = $('#userId').val();
    $.ajax({
        url: urls.UserManagement.CheckForUniqueEmail,
        type: 'GET',
        data: { emailId: emailId, userId: userId },
        async: false,
        success: function (result) {
            if (result) {
                $('#sptxtEmail').text("Email Id already exists, please use another Email Id");
            }
            else {
                $('#sptxtEmail').text('');
            }
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
        }
    });
    let check = isValidEmail(emailId);
    if (!check) {
        $('#sptxtEmail').text("please enter valid Email Id");
    }
}


function addUserInfo() {
    let form = document.getElementById('AddADUserInform');
    let userInput = new FormData(form);
    $.ajax({
        type: 'Post',
        data: userInput,
        cache: false,
        contentType: false,
        processData: false,
        url: urls.UserManagement.AddTANUserData,
        success: function (response) {
            if (response == "success") {
                window.location.href = urls.UserManagement.SearchUserData;
            }
            else if (response == "exist") {
                $('#addmemberModal').modal('hide');
                WarningMsg('Warning', "This User has been already added", '');
            }
            else {
                $('#addmemberModal').modal('hide');
                refreshUserModel();
                ErrorMsg('Error', "Unable to add user!Try again Later", '');
            }
        },
        failure: function () {
            ErrorMsg('Error', "Something went wrong!Try again Later", '');
        },
        error: function (xhr, status, error) {
            if (status == HTTP_STATUS_FORBIDDEN || xhr.status == HTTP_STATUS_FORBIDDEN) {
                ErrorMsg("Failed", "Unauthorized Access");
            }
            else if (xhr.status == HTTP_STATUS_TOOMANYREQUESTS || status == HTTP_STATUS_TOOMANYREQUESTS) {
                ErrorMsg("Failed", "Too Many Requests, Please try after some time");
                $('#addmemberModal').modal('hide');
                refreshUserModel();
            }
            else {
                ErrorMsg('Error', "Something went wrong!Try again Later", '');
            }
        }
    });
}

function validateTANUser(firstName, lastName, email, userType, roleName, defApp) {
    let result = true;
    if (firstName == "") {
        $('#sptxtfName').text("Please enter first name");
        result = false;
    }
    if (lastName == "") {
        $('#sptxtlName').text("Please enter first name");
        result = false;
    }
    if (email == "") {
        $('#sptxtEmail').text("Please enter email id");
        result = false;
    }
    if (!isValidEmail(email)) {
        $('#sptxtEmail').text("Please enter valid email id");
        result = false;
    }
    if (userType == "") {
        $('#spuserType').text("Please select user type");
        result = false;
    }
    if (roleName == "") {
        $('#sproleName').text("Please select role");
        result = false;
    }
    if (defApp == "") {
        $('#spdefapplication').text("Please select default application name");
        result = false;
    }
    return result;
}

function validateOtherUser(firstName, lastName, email, userType, organizationName, facilityName, defApp) {
    let result = true;
    if (firstName == "") {
        $('#sptxtfName').text("Please enter first name");
        result = false;
    }
    if (lastName == "") {
        $('#sptxtlName').text("Please enter last name");
        result = false;
    }
    if (email == "") {
        $('#sptxtEmail').text("Please enter email id");
        result = false;
    }
    if (!isValidEmail(email)) {
        $('#sptxtEmail').text("Please enter valid email id");
        result = false;
    }
    if (userType == "") {
        $('#spuserType').text("Please select user type");
        result = false;
    }
    if (organizationName == "") {
        $('#sporanizationName').text("Please select organization name");
        result = false;
    }
    if (facilityName == "") {
        $('#spfacilityName').text("Please select facility name");
        result = false;
    }
    if (defApp == "") {
        $('#spdefapplication').text("Please select default application name");
        result = false;
    }
    return result;
}

function ResendMail(id) {
    let input = { UserId: id };
    $.ajax({
        url: urls.UserManagement.ResendUrl,
        type: 'POST',
        data: input,
        async: false,
        success: function (response) {
            if (response != "") {
                SuccessMsg('Success', response, '');
            }
            else {
                ErrorMsg('Failed', 'Something Went wrong', '')
            }
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
        }
    });
}

function GetOrgFacilities() {

    let OrgName = "";


    let filterInput = {
        OrgId: $("#OrgName").val(),
    }
    if ($("#OrgName").val() != "") {
        $.ajax({
            type: 'Post',
            data: filterInput,
            async: false,
            cache: false,
            url: urls.UserManagement.FaciltyList,
            success: function (response) {
                let orgFacilities = $("#FacilityNameFilter");
                orgFacilities.empty();
                orgFacilities.append('<option value="">- Select Facility Name -</option>');
                $.each(response, function (index, item) {
                    orgFacilities.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
                orgFacilities.trigger("change");
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