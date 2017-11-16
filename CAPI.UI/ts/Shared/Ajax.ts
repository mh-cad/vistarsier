/// <reference path="../../Scripts/typings/jquery/jquery.d.ts"/>

namespace VisTarsier.Shared.Ajax {
    $(() => {
        $.ajaxSetup({ cache: false }); 
    });

    export function add(url, dataToSave, callback, onComplete): void {
        call(url, dataToSave, "POST", "Ajax Call: Item Added.", callback, onComplete);
    }

    export function update(url, dataToSave, successCallback, onComplete): void {
        call(url, dataToSave, "PUT", "Ajax Call: Item Updated.", successCallback, onComplete);
    }

    export function deleteIt (url, onComplete): void {
        call(url, null, "GET", "Ajax Call: Item Deleted.", null, onComplete);
    }

    export function read (url: string, callback: any, onComplete: any = null): void {
        call(url, null, "GET", "Ajax Call: Data Retrieved", callback, onComplete);
    }

    function call(url, dataToSave, httpVerb, successMessage, callback, onComplete): void {
        $.ajax(url, {
            data: dataToSave,
            type: httpVerb,
            accepts: "application/json",
            dataType: "json",
            contentType: "application/json",
            success: (data) => {
                console.log(successMessage);
                if (callback !== undefined && callback !== null) {
                    callback(data);
                }
            },
            error: (jqXhr, textStatus, errorThrown) => {
                console.log("Ajax Error Status:", textStatus, "|", "Error:", errorThrown);
                console.log("AJAX: Unexpected error.");
            },
            complete: () => {
                if (onComplete !== undefined && onComplete !== null) {
                    onComplete();
                }
            }
        });
    }
}