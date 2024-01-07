function LoadThinkAnewTiles() {
    $.ajax({
        url: urls.Dashboard.GetThinkaNewTiles,
        type: 'GET', async: false,
        success: function (response) {

            $('#ThinkAnewTiles').html(response);
        }
    });
}

function LoadSecondaryTiles() {
    $.ajax({
        url: urls.Dashboard.GetSecondaryTiles,
        type: 'GET', async: false,
        success: function (response) {

            $('#SecondaryTiles').html(response);
        }
    });
}

function LoadFacilityTiles() {

    $.ajax({

        url: urls.Dashboard.GetFacilityListTiles,
        type: 'GET', async: false,
        success: function (response) {

            $('#GetFacilityTiles').html(response);
        }
    });
}
function LoadLabourTable() {
    $.ajax({
        url: urls.Dashboard.GetLabourcodeTables,
        type: 'GET', async: false,
        success: function (response) {
            $('#GetLabourCodeTable').html(response);
        }
    });
}
function LoadJobTable() {
    $.ajax({
        url: urls.Dashboard.GetjobTables,
        type: 'GET', async: false,
        success: function (response) {

            $('#GetTopJobTable').html(response);
        }
    });
}
function LoadStaffingData() {
    $.ajax({
        url: urls.Dashboard.GetstfData,
        type: 'GET', async: false,
        success: function (response) {
            
            $('#Staffingdata').html(response);
            //var parsedResponse = JSON.parse(response);

            //// Access the partialViewModel object
            //var partialViewModel = parsedResponse.partialViewModel;
            
            //generateBarChart(partialViewModel.data, partialViewModel.labels, "StaffingBar");
        }
    });
}
function resetstaff() {
    $.ajax({
        url: urls.Dashboard.GetstfData,
        type: 'GET', async: false,
        success: function (response) {
            
            $('#Staffingdata').html(response);
            //var parsedResponse = JSON.parse(response);

            //// Access the partialViewModel object
            //var partialViewModel = parsedResponse.partialViewModel;
            
            //generateBarChart(partialViewModel.data, partialViewModel.labels, "StaffingBar");
        }
    });
}
function loadFacilitydropdown() {

    $.ajax({
        type: 'Get',
        cache: false,
        async: false,
        url: urls.Dashboard.GetFacilityItemList,
        success: function (response) {
            let applicationSelect = $("#facilities");
            applicationSelect.empty();
            applicationSelect.append('<option value="">- Select Facility Name -</option>');
            $.each(response, function (index, item) {
                applicationSelect.append('<option value="' + item.value + '">' + item.text + '</option>');
            });
            applicationSelect.trigger("change");
        }
    });
}
function loadfilteredLabour() {
    let facilityId = $("#facilities").val();
    let fiscal = $("#fiscal").val();
    let year = $("#year").val();
    let month = $("#month").val();

    let filterInput = {
        facilityId: $("#facilities").val(),
        fiscal: $("#fiscal").val(),
        year: $("#year").val(),
        month: $("#month").val()
    }
    $.ajax({
        url: urls.Dashboard.GetLabourcodeTables,
        type: 'GET',
        data: filterInput,
        async: false,
        success: function (response) {
            $('#GetLabourCodeTable').html(response);
            $("#facilities").val(facilityId);
            $("#fiscal").val(fiscal);
            $("#year").val(year);
            $("#month").val(month);
        }
    });


}
function loadfilteredJob() {
    let jcfacilityId = $("#jcfacilities").val();
    let jcfiscal = $("#jcfiscal").val();
    let jcyear = $("#jcyear").val();
    let jcmonth = $("#jcmonth").val();

    let filterInput = {
        facilityId: $("#jcfacilities").val(),
        fiscal: $("#jcfiscal").val(),
        year: $("#jcyear").val(),
        month: $("#jcmonth").val()
    }
    $.ajax({
        url: urls.Dashboard.GetjobTables,
        type: 'GET',
        data: filterInput,
        async: false,
        success: function (response) {
            $('#GetTopJobTable').html(response);
            $("#jcfacilities").val(jcfacilityId);
            $("#jcfiscal").val(jcfiscal);
            $("#jcyear").val(jcyear);
            $("#jcmonth").val(jcmonth);
        }
    });


}
function LoadfilteredStaffingData() {
    let sfOrgId = $("#SOrg").val();
    let sffacilityId = $("#SFacilities").val();
    let jsfyear = $("#Syear").val();

    let filterInput = {
        orgId: $("#SOrg").val(),
        facilityId: $("#SFacilities").val(),
        year: $("#Syear").val(),

    }
    $.ajax({
        url: urls.Dashboard.GetstfData,
        type: 'GET',
        data: filterInput,
        async: false,
        success: function (response) {
            $('#Staffingdata').html(response);
            $("#SOrg").val(sfOrgId);
            $("#SFacilities").val(sffacilityId);
            $("#Syear").val(jsfyear);

        }
    });


}
function Loadblogs() {
    $.ajax({
        url: urls.Dashboard.GetBlogs,
        type: 'GET',
        async: false,
        success: function (response) {
            
            $('#blogs').html(response);
        }
    });
}
function LoadPendingStaffingData() {

    $.ajax({
        url: urls.Dashboard.GetPendingdata,
        type: 'GET',
        async: false,
        success: function (response) {
            $('#Pendingdata').html(response);
        }
    });

}

function LoadfilteredSubmissionSatus() {
    let sfOrgId = $("#SubOrg").val();



    let filterInput = {
        OrgId: $("#SubOrg").val(),



    }
    $.ajax({
        url: urls.Dashboard.GetSubmissionStatus,
        type: 'GET',
        async: false,
        data: filterInput,
        success: function (response) {
            $('#SubmissionStatus').html(response);
            $("#SubOrg").val(sfOrgId);



        }
    });





}
function LoadSubmissionSatus() {

    $.ajax({
        url: urls.Dashboard.GetSubmissionStatus,
        type: 'GET',
        async: false,
        success: function (response) {
            $('#SubmissionStatus').html(response);
        }
    });



}

function LoadPreviousSubmission() {



    $.ajax({
        url: urls.Dashboard.GetPreviousSubmission,
        type: 'GET',
        async: false,
        success: function (response) {
            $('#SubmissionSData').html(response);
        }
    });



}
function LoadpendingSubmission() {



    $.ajax({
        url: urls.Dashboard.GetpendingSubmissions,
        type: 'GET',
        async: false,
        success: function (response) {
            $('#PendingSubmission').html(response);
        }
    });



}
function resetsubStatus() {
    $.ajax({
        url: urls.Dashboard.GetSubmissionStatus,
        type: 'GET',
        async: false,
        success: function (response) {
            
            $('#SubmissionStatus').html(response);



        }
    });
}

function ResetJobCodes() {
     $("#jcfacilities").val('');
    $("#jcfiscal").val('');
    $("#jcyear").val('');
    $("#jcmonth").val('');

  
    $.ajax({
        url: urls.Dashboard.GetjobTables,
        type: 'GET',
        async: false,
     //   data: filterInput,
        success: function (response) {
            $('#GetTopJobTable').html(response);
           
        }
    });

}

function ResetLabourcodeTable() {
     $("#facilities").val('');
    $("#fiscal").val('');
    $("#year").val('');
    $("#month").val('');

   
    $.ajax({
        url: urls.Dashboard.GetLabourcodeTables,
        type: 'GET',
        async: false,
      //  data: filterInput,
        success: function (response) {
            $('#GetLabourCodeTable').html(response);
           
        }
    });


}