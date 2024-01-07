var ListQuarters = [
    { key: '1', value: '1st Quarter (October 1 - December 31)' },
    { key: '2', value: '2nd Quarter (January 1 - March 31)' },
    { key: '3', value: '3rd Quarter (April 1 - June 30)' },
    { key: '4', value: '4th Quarter (July 1 - September 30)' }
];

var ListMonths = [{
    0: "Select All",
    10: "October",
    11: "November",
    12: "December"
},
{
    0: "Select All",
    1: "January",
    2: "February",
    3: "March"
},
{
    0: "Select All",
    4: "April",
    5: "May",
    6: "June"
}, {
    0: "Select All",
    7: "July",
    8: "August",
    9: "September"
}]

$('document').ready(function () {
    getYear();
    getQuaterList();
    getMonthList();  
})

function getQuaterList() {
    $('#QuarterList').html();
    var dateobj = new Date();
    var getMonth = dateobj.getMonth();
    var _key = findQuarter(getMonth + 1);
    var qtrList = "<option value='-1'>Select</option>";
    ListQuarters.forEach(function (value, key) {
        if (value.key == _key) {
            qtrList += "<option value='" + value.key + "' selected>" + value.value + "</option>";
        } else {
            qtrList += "<option value='" + value.key + "'>" + value.value + "</option>";
        }
    })
    $('#QuarterList').html(qtrList);
}

function findQuarter(month) {
    var value = 0;
    switch (month) {
        case 1:
        case 2:
        case 3:
            value = 2;
            break;
        case 4:
        case 5:
        case 6:
            value = 3;
            break;
        case 7:
        case 8:
        case 9:
            value = 4;
            break;
        case 10:
        case 11:
        case 12:
            value = 1;
            break;
    }
    return value;
}

function getMonthList() {
    $('#MonthList').html();
    var mnthList = "";
    var qrtId = $('#QuarterList').val();
    qrtId = qrtId - 1; 
    $.each(ListMonths[qrtId], function (i, item) {
        mnthList += "<option value='" + i + "'>" + item + "</option>";
    })
    $('#MonthList').html(mnthList);
}

function getYear() {
    var start_date = 2020;
    var dateobj = new Date();
    var end_date = dateobj.getFullYear();
    $('#YearList').html();
    var yrList = "";
    yrList += "<option value='0'>Select</option>";

    while (start_date <= end_date) {
        if (end_date == start_date) {
            yrList += "<option value='" + start_date + "' selected>" + start_date + "</option>";
        } else {
            yrList += "<option value='" + start_date + "'>" + start_date + "</option>";
        }
        start_date = start_date + 1;
    }
    $('#YearList').html(yrList);
}

function GenerateSubmitData() {
    if ($('#FacilityId').val() == "" || $('#FacilityId').val() == undefined) {
        WarningMsg("Warning!", 'Please select Facility', "Okay"); 
        return false;
    }
    ConfirmMsg('Generate', 'Are you sure want to Generate the data?', 'Generate', event, function () {
        let date = new Date();
        let fileName = $('#FacilityId :selected').text() + moment(date).format("YYYYMMDDHHMMSS");
        var GenerateXml = {
            FacilityId: $('#FacilityId').val(),
            Year: $('#YearList').val(),
            ReportQuarter: $('#QuarterList').val(),
            FileName: fileName
        } 
        showLoader();
        $.ajax({
            url: urls.Submit.GenerateXMLFile,
            type: 'GET',
            data: GenerateXml,
            xhrFields: {
                responseType: 'blob'
            },
            success: function (data) {
                hideLoader();
                let url = window.URL.createObjectURL(data); 
                let link = document.createElement('a');
                link.href = url;
                link.download = fileName + '.zip';
                link.click();

                window.URL.revokeObjectURL(url);
            }
        });

    })
}