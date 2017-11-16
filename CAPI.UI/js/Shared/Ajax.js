/// <reference path="../../Scripts/typings/jquery/jquery.d.ts"/>
var VisTarsier;
(function (VisTarsier) {
    var Shared;
    (function (Shared) {
        var Ajax;
        (function (Ajax) {
            $(function () {
                $.ajaxSetup({ cache: false });
            });
            function add(url, dataToSave, callback, onComplete) {
                call(url, dataToSave, "POST", "Ajax Call: Item Added.", callback, onComplete);
            }
            Ajax.add = add;
            function update(url, dataToSave, successCallback, onComplete) {
                call(url, dataToSave, "PUT", "Ajax Call: Item Updated.", successCallback, onComplete);
            }
            Ajax.update = update;
            function deleteIt(url, onComplete) {
                call(url, null, "GET", "Ajax Call: Item Deleted.", null, onComplete);
            }
            Ajax.deleteIt = deleteIt;
            function read(url, callback, onComplete) {
                if (onComplete === void 0) { onComplete = null; }
                call(url, null, "GET", "Ajax Call: Data Retrieved", callback, onComplete);
            }
            Ajax.read = read;
            function call(url, dataToSave, httpVerb, successMessage, callback, onComplete) {
                $.ajax(url, {
                    data: dataToSave,
                    type: httpVerb,
                    accepts: "application/json",
                    dataType: "json",
                    contentType: "application/json",
                    success: function (data) {
                        console.log(successMessage);
                        if (callback !== undefined && callback !== null) {
                            callback(data);
                        }
                    },
                    error: function (jqXhr, textStatus, errorThrown) {
                        console.log("Ajax Error Status:", textStatus, "|", "Error:", errorThrown);
                        console.log("AJAX: Unexpected error.");
                    },
                    complete: function () {
                        if (onComplete !== undefined && onComplete !== null) {
                            onComplete();
                        }
                    }
                });
            }
        })(Ajax = Shared.Ajax || (Shared.Ajax = {}));
    })(Shared = VisTarsier.Shared || (VisTarsier.Shared = {}));
})(VisTarsier || (VisTarsier = {}));
//# sourceMappingURL=D:/Dropbox/Projects/VT/VT-App/D:/Dropbox/Projects/CAPI/Application/CAPI.UI/js/Shared/Ajax.js.map