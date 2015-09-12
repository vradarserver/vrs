// Copied from http://stackoverflow.com/questions/24871041/how-to-change-text-for-selected-fields-to-uppercase-using-knockoutjs
ko.extenders.uppercase = function(target, option) {
    target.subscribe(function(newValue) {
       target(newValue.toUpperCase());
    });
    return target;
};
