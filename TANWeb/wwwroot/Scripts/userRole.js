function searchUserRoles() {
	let userInput = {
		userId: $("#Uid").val(),
		appId: $("#usAppName").val(),
		roleId: $("#usRoleName").val(),
		userType: $("#userTypes").val()
	}
	$.ajax({
		type: 'Post',
		data: userInput,
		async: false,
		url: urls.UsrRoles.SearchUserData,
		success: function (response) {		

			$('#userRolesTable').html($(response).find("#userRolesTable").html());
			$('#userRolesTable').dataTable().fnDestroy();
			$('#userRolesTable').DataTable();
		}

	});
}
function GetusRoles() {
	let userInput = {
		OrgId: $("#usAppName").val()
	}
	if ($("#usAppName").val() != "") {
		$.ajax({
			type: 'Post',
			data: userInput,
			async: false,
			url: urls.UserManagement.GetroleList,
			success: function (response) {
				let applicationSelect = $("#usRoleName");
				applicationSelect.empty();
				applicationSelect.append('<option value=""> Select Role </option>');
				$.each(response, function (index, item) {
					applicationSelect.append('<option value="' + item.value + '">' + item.text + '</option>');
				});
				applicationSelect.trigger("change");
			}
		});
	}
	
}

function getuserRoles() {
	let userType = $('#userTypes').val();

	if (userType == 1) {
		$('#appFilter').hide();
		$.ajax({
			type: 'Post',
			async: false,
			url: urls.UsrRoles.GetTANRoleList,
			success: function (response) {
				let userRoles = $("#usRoleName");
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
		$('#appFilter').show();
		$('#usRoleName').val(null).trigger('change');
	}
}
function resetUserRoleFilter() {
    $("#usAppName").val("");
	$("#usRoleName").val("")
	$("#userTypes").val("");
	$("#usAppName").trigger("change");
	$("#usRoleName").trigger("change");
	$("#userTypes").trigger("change");
	let userInput = {
		userId: $("#Uid").val(),
		appId: $("#usAppName").val(),
		roleId: $("#usRoleName").val(),
	}
	$.ajax({
		type: 'Post',
		data: userInput,
		async: false,
		url: urls.UsrRoles.SearchUserData,
		success: function (response) {
			$("#usRoleName").html($(response).find("#usRoleName").html());
			$("#usRoleName").trigger("change");

			$('#userRolesTable').html($(response).find("#userRolesTable").html());
			$('#userRolesTable').dataTable().fnDestroy();
			$('#userRolesTable').DataTable();
		}

	});

}