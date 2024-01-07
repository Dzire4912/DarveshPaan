
$('.AllowNumbers').keyup(function (event) {
    this.value = this.value.replace(/[^0-9]/g, '');
});

$('.AllowCharacters').keyup(function (event) {
    this.value = this.value.replace(/[^A-z]/g, '');
});
$('.allowCharactersAndNumbers').keyup(function (event) { 
    let pattern = /^[a-zA-Z0-9]+$/;
    let key = event.key;
    if (pattern.test(key) || key === 'Backspace') {
        return;
    }
    event.preventDefault();
});

function allowAlphaNumericSpace(event) {
    let pattern = /^[a-zA-Z0-9\s]+$/;
    let key = event.key;
    if (pattern.test(key) || key === 'Backspace') {
        return;
    }
    event.preventDefault();
}

$('.maxInputNumberLength').keyup(function (event) {
    this.value = this.value.replace(/[^0-9]/g, '');
    if (this.value.length > 10) {
        this.value = this.value.substr(0, 10); // Truncate to a maximum length of 10
        $(this).val(this.value); // Update the input value with truncated value
    }
});

function allowAlphaNumericWithoutSpace(event) {
    let pattern = /^[a-zA-Z0-9]+$/;
    let key = event.key;
    if (pattern.test(key) || key === 'Backspace') {
        return;
    }
    event.preventDefault();
}
