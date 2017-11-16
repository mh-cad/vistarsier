/// <reference path="../Scripts/typings/jquery/jquery.d.ts"/>

namespace VisTarsier.Home
{
    export class Initialise {
        static pageLoaded = () => {
            Initialise.initialiseComponents();
            Initialise.addEventHandlers();
        }

        private static initialiseComponents = () => {
            $("#btnLinkNegAndPos").data("linked", true);
        }

        private static addEventHandlers = () => {
            $("#btnRunAll").on("click", () => {
                Shared.Ajax.read("handlers/Scripts.ashx?p=runAll", data => { console.log(data); });
            });
            $("#btnStep1").on("click", () => {
                Shared.Ajax.read("handlers/Scripts.ashx?p=step1", data => { console.log(data); });
            });
            $("#btnStep2").on("click", () => {
                Shared.Ajax.read("handlers/Scripts.ashx?p=step2", data => { console.log(data); });
            });
            $("#btnStep3").on("click", () => {
                Shared.Ajax.read("handlers/Scripts.ashx?p=step3", data => { console.log(data); });
            });
            $("#btnStep4").on("click", () => {
                Shared.Ajax.read("handlers/Scripts.ashx?p=step4", data => { console.log(data); });
            });
            $("#btnStep5").on("click", () => {
                Shared.Ajax.read("handlers/Scripts.ashx?p=step5", data => { console.log(data); });
            });
            $("#btnStep6").on("click", () => {
                Shared.Ajax.read("handlers/Scripts.ashx?p=step6", data => { console.log(data); });
            });
            $("#btnLoadImages").on("click", () => {
                getImagesList();
            });


            $(".ImageNavigator").on("change", (event) => {
                const self = event.target;
                const thisImgViewer = $(self).closest(".imageViewerContainer").find(".imageViewer")[0];
                const otherImgViewer = $(".imageViewerContainersRow").find(".imageViewer").not(`#${thisImgViewer.id}`);
                const imagesAreLinked = $("#btnLinkNegAndPos").data("linked");

                const targetsArr = new Array(thisImgViewer);
                if (imagesAreLinked) targetsArr.push(otherImgViewer[0]);
                displayImage(parseInt($(self).val()), targetsArr);
            });

            $(".imageViewer").bind("mousewheel DOMMouseScroll", (event) => {
                const self = event.target;
                const other = $(".imageViewerContainersRow").find(".imageViewer").not(`#${self.id}`);
                const imgNum = parseInt($(self).data("imgNumber"));
                const imagesAreLinked = $("#btnLinkNegAndPos").data("linked");

                if ((event.originalEvent as any).wheelDelta > 0 || (event.originalEvent as any).detail < 0) {
                    // Up
                    if (imgNum > 0) {
                        const targetsArr = new Array(self);
                        if (imagesAreLinked) targetsArr.push(other[0]);
                        displayPrevImage(targetsArr);
                    }
                }
                else {
                    // Down
                    if (imgNum < totalNoOfImages) {
                        const targetsArr = new Array(self);
                        if (imagesAreLinked) targetsArr.push(other[0]);
                        displayNextImage(targetsArr);
                    }
                }
                return false;
            });

            $("#btnLinkNegAndPos").on("click", (event) => {
                const self = $(event.target).closest("button");
                if ($(self).data("linked")) {
                    $(self).data("linked", false);
                    $(self).find("i").removeClass("fa-chain").addClass("fa-chain-broken");
                } else {
                    $(self).data("linked", true);
                    $(self).find("i").removeClass("fa-chain-broken").addClass("fa-chain");
                }
            });
        }
    }
}
