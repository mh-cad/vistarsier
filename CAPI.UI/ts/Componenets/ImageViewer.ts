namespace VisTarsier.Components1 {
    export class ImageViewer {
        totalNoOfImages = 160;

        constructor(elems: HTMLElement[]) {
            elems.forEach((elem) => {
                $(`#${elem.id}`).bind("mousewheel DOMMouseScroll", (event) => {
                    const self = event.target;
                    const other = $(".imageViewerContainersRow").find(".imageViewer").not(`#${self.id}`);
                    const imgNum = parseInt($(self).data("imgNumber"));
                    const imagesAreLinked = $("#btnLinkNegAndPos").data("linked");

                    if ((event.originalEvent as any).wheelDelta > 0 || (event.originalEvent as any).detail < 0) {
                        // Up
                        if (imgNum > 0) {
                            const targetsArr = new Array(self);
                            if (imagesAreLinked) targetsArr.push(other[0]);
                            //displayPrevImage(targetsArr);
                        }
                    }
                    else {
                        // Down
                        if (imgNum < this.totalNoOfImages) {
                            const targetsArr = new Array(self);
                            if (imagesAreLinked) targetsArr.push(other[0]);
                            //displayNextImage(targetsArr); 
                        }
                    }
                    return false;
                });
            });
        }
    }
}