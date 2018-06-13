// Write your JavaScript code.
function doJsonPut(url, json, notifyText) {
    doJson("put", baseUrl+ url, json, notifyText);
}
function doJson(method, url, json, notifyText) {
    $.ajax({
        type: method,
        url: url,
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(json),
        dataType: "json",
        success: function (message) {
            var data = message;
            if (data.StatusCode === 1000) {
                alert(notifyText + "成功");
            } else {
                alert(notifyText + "失败");
            }
        },
        error: function (message) {
            alert(notifyText + "失败");
        }
    });
}
function doJsonPost(url, json, notifyText) {
    doJson("post", baseUrl+ url, json, notifyText);
}

function getPData(currentUrl, data, onSuccess) {
    $.getJSON(baseUrl + currentUrl, data, function (data, status) {
        if (data.StatusCode === 1000) {
            onSuccess(data);
        }
    });
}
function getData(currentUrl, onSuccess) {
    $.getJSON(baseUrl + currentUrl, function (data, status) {
        if (data.StatusCode === 1000) {
            onSuccess(data);
        }
    });
}


function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
};
function getUrlVar(name) {
    return getUrlVars()[name];
}