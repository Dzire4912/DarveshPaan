const HTTP_STATUS_FORBIDDEN = 403;
const HTTP_STATUS_TOOMANYREQUESTS = 429;
function SuccessMsg(Header, Msg, BtnName) {
    Swal.fire({
        title: Header,
        text: Msg,
        icon: "success",
        showCancelButton: !1,
        confirmButtonClass: "btn btn-primary w-xs me-2 mt-2",
        buttonsStyling: !1,
        showCloseButton: !0
    })
}

function SuccessMsgWithReload(Header, Msg, BtnName) {
    Swal.fire({
        title: Header,
        text: Msg,
        icon: "success",
        showCancelButton: !1,
        confirmButtonClass: "btn btn-primary w-xs me-2 mt-2",
        buttonsStyling: !1,
        showCloseButton: !0
    }).then((result) => {
        if (result.isConfirmed) {
            window.location.reload();
        }
    });
}

function ErrorMsg(Header, Msg, BtnName) {
    Swal.fire({
        title: Header,
        text: Msg,
        icon: 'error',
        showCancelButton: !1,
        confirmButtonClass: "btn btn-primary w-xs me-2 mt-2",
        buttonsStyling: !1,
        showCloseButton: !0
    })
}

function WarningMsg(Header, Msg, BtnName) {
    Swal.fire({
        title: Header,
        text: Msg,
        icon: 'warning',
        showCancelButton: !1,
        confirmButtonClass: "btn btn-primary w-xs me-2 mt-2",
        buttonsStyling: !1,
        showCloseButton: !0
    })
}

function ConfirmMsg(Header, Msg, BtnName, event, handleOkClick) {
    event.preventDefault();
    Swal.fire({
        title: Header,
        text: Msg,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#009ade',
        cancelButtonColor: '#d33',
        confirmButtonText: BtnName
    }).then((result) => {
        if (result.isConfirmed) {
            handleOkClick()
        }

    })
}

function ToastifyMsg(msg, color) {
    Toastify({
        offset: {
            x: 50, // horizontal axis - can be a number or a string indicating unity. eg: '2em'
            y: 70 // vertical axis - can be a number or a string indicating unity. eg: '2em'
        },
        text: msg,
        duration: 4000,
        newWindow: true,
        close: true,
        gravity: "top", // `top` or `bottom`
        positionLeft: false, // `true` or `false`
        positionRight: true,
        backgroundColor: color
    }).showToast();

}


//email validation
function validEmail(sEmail) {
    let filter = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;

    if (filter.test(sEmail)) {
        return true;
    }
    else {
        return false;
    }
}

//Securepassword validation
function PasswordCheck(password) {
    let pass = password;
    let strength = 1;
    let arr = [/.{5,}/, /[a-z]+/, /[0-9]+/, /[A-Z]+/];
    jQuery.map(arr, function (regexp) {
        if (pass.match(regexp))
            strength++;
    });
}

//password and reenterpassword match validation
function PasswordReenterPasswordCHeck(first, second) {
    if (first === second) {
        return true;
    }
    else {
        false;
    }
}

//mobilenumber validation
function mobileNumber(mobNum) {
    //var filter = /^\d*(?:\.\d{1,2})?$/;
    let filter = /^((?:[1-9][0-9 ().-]{5,12}[0-9])|(?:(00|0)( ){0,1}[1-9][0-9 ().-]{3,12}[0-9])|(?:(\\+)( ){0,1}[1-9][0-9 ().-]{4,12}[0-9]))$/gm;

    if (filter.test(mobNum)) {
        /* if (mobNum.length == 10) {*/
        return true;
    }
    else {
        return false;
    }

}

//Required field validation
function Requiredfield(input) {
    let val = input.trim() || '';
    if (val != '') {
        return true;
    }
    else {
        return false;
    }
}

//number only
function numberonly(input) {
    //return /^\d+$/.test(input);
    if (input.match(/^\d+$/)) {
        return true;
    }

    return false;

};



//phoneFormat
function phoneFormat(input) {
    // Strip all characters from the input except digits
    input = input.replace(/\D/g, '');

    // Trim the remaining input to ten characters, to preserve phone number format
    input = input.substring(0, 12);

    // Based upon the length of the string, we add formatting as necessary
    let size = input.length;
    if (size == 0) {
        input = input;
    } else if (size < 4) {
        input = '' + input;
    } else if (size < 7) {
        input = ' ' + input.substring(0, 2) + ' ' + input.substring(2, 6);
    } else if (size > 7) {
        input = ' ' + input.substring(0, 2) + ' ' + input.substring(2, 6) + input.substring(6, 12);
    }
    return input;
}

//tel_Format
function tel_Format(input) {
    input = input.replace(/\D/g, '');
    input = input.substring(0, 11);
    let size = input.length;
    if (size == 0) {
        input = input;
    } else if (size < 4) {
        input = '' + input;
    } else if (size < 7) {
        input = ' ' + input.substring(0, 3) + ' ' + input.substring(3, 7);
    } else if (size > 7) {

        input = ' ' + input.substring(0, 3) + ' ' + input.substring(3, 7) + ' ' + input.substring(7, 11);
    }
    return input;
}


function showLoader() {
    $('.preloader').css('display', 'block');
}

function hideLoader() {
    $('.preloader').css('display', 'none');
}

function draggableModel(id) {
    $(id + ' .modal-dialog').draggable({
        handle: ".modal-header" // Set the handle to the modal header
    });
}

